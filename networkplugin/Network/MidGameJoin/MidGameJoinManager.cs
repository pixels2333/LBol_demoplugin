

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using BepInEx.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.MidGameJoin.Result;
using NetworkPlugin.Network.Reconnection;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 中途加入管理器 - 允许玩家在开始后加入游戏
/// 依赖: 断线重连系统
/// </summary>
public sealed class MidGameJoinManager
{
    private readonly ManualLogSource _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MidGameJoinConfig _config;
    private INetworkClient? _client;
    private NetworkClient? _concreteClient;
    private int _initialized;
    private string? _lastKnownHostPlayerId;

    private sealed class IssuedJoinToken
    {
        public string JoinToken { get; set; } = string.Empty;
        public string ClientPlayerId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public long ExpiresAtUtcTicks { get; set; }
    }

    private sealed class PendingFullSyncRequest : IDisposable
    {
        public ManualResetEventSlim WaitHandle { get; } = new(false);
        public FullStateSnapshot? FullSnapshot { get; set; }
        public List<GameEvent> MissedEvents { get; set; } = [];
        public string? ErrorMessage { get; set; }
        public void Dispose() => WaitHandle.Dispose();
    }

    /// <summary>
    /// 待处理的中途加入请求
    /// </summary>
    private readonly List<GameJoinRequest> _pendingRequests;

    /// <summary>
    /// 已批准但尚未加入的玩家
    /// </summary>
    private readonly Dictionary<string, ApprovedJoin> _approvedJoins;
    private readonly Dictionary<string, IssuedJoinToken> _issuedJoinTokens = [];
    private readonly Dictionary<string, PendingFullSyncRequest> _pendingFullSyncRequests = [];

    /// <summary>
    /// 用于线程安全
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 快速同步服务
    /// </summary>
    private readonly FastSyncService _fastSyncService;

    /// <summary>
    /// 托管AI服务（用于玩家断线时代理操作）
    /// </summary>
    public AIPlayerController AIController { get; private set; }

    public MidGameJoinManager(MidGameJoinConfig config, ManualLogSource logger, IServiceProvider serviceProvider)
    {
        _config = config ?? new MidGameJoinConfig();
        _logger = logger ?? Plugin.Logger;
        _serviceProvider = serviceProvider;
        _pendingRequests = [];
        _approvedJoins = [];
        _fastSyncService = new FastSyncService(_logger);
        AIController = new AIPlayerController(_logger);
    }

    /// <summary>
    /// 初始化中途加入管理器
    /// 说明：订阅网络事件并维护 Host/玩家列表快照。
    /// </summary>
    public void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            _logger.LogDebug("[MidGameJoinManager] Initialize skipped (already initialized)");
            return;
        }

        try
        {
            _client = _serviceProvider.GetService<INetworkClient>();
            _concreteClient = _client as NetworkClient;

            if (_client == null)
            {
                _logger.LogWarning("[MidGameJoinManager] Initialize skipped: INetworkClient not available");
                Interlocked.Exchange(ref _initialized, 0);
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(_client);

            _client.OnGameEventReceived += OnGameEventReceived;
            _client.OnConnectionStateChanged += OnConnectionStateChanged;

            _logger.LogInfo("[MidGameJoinManager] Initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Initialize failed: {ex.Message}");
            Interlocked.Exchange(ref _initialized, 0);
        }
    }

    /// <summary>
    /// 请求中途加入游戏
    /// 说明：最小可用实现（校验连接/PlayerId/HostId），通过 DirectMessage 向 Host 发起请求。
    /// </summary>
    public JoinRequestResult RequestJoin(string roomId, string playerName)      
    {
        try
        {
            if (!_config.AllowMidGameJoin)
            {
                return JoinRequestResult.Denied("Mid-game joining is disabled");
            }

            if (string.IsNullOrWhiteSpace(roomId))
            {
                return JoinRequestResult.Denied("Missing roomId");
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                return JoinRequestResult.Denied("Missing playerName");
            }

            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client?.IsConnected != true)
            {
                return JoinRequestResult.Denied("Not connected");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return JoinRequestResult.Denied("Missing self playerId (wait for Welcome/PlayerListUpdate)");
            }

            string? hostId;
            lock (_lock)
            {
                hostId = _lastKnownHostPlayerId;
            }

            if (string.IsNullOrWhiteSpace(hostId))
            {
                return JoinRequestResult.Denied("Host not found (join room first and wait for PlayerListUpdate)");
            }

            string requestId = GenerateRequestId();
            _logger.LogInfo($"[MidGameJoinManager] RequestJoin => host={hostId}, room={roomId}, requestId={requestId}");

            SendDirectMessage(hostId, NetworkMessageTypes.MidGameJoinRequest, new
            {
                RequestId = requestId,
                RoomId = roomId,
                PlayerName = playerName,
                ClientPlayerId = selfId,
                ClientTimeUtcTicks = DateTime.UtcNow.Ticks
            });

            return JoinRequestResult.Pending(requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error processing join request: {ex.Message}");
            return JoinRequestResult.Denied($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 批准中途加入请求（由房主调用）
    /// 说明：最小可用实现（host-only + 超时清理 + 颁发 JoinToken + 生成 BootstrappedState）。
    /// </summary>
    public ApproveJoinResult ApproveJoin(string requestId, string approvedByPlayerId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return ApproveJoinResult.Failed("Missing requestId");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return ApproveJoinResult.Failed("Missing self playerId");
            }

            if (!string.Equals(selfId, approvedByPlayerId, StringComparison.Ordinal))
            {
                return ApproveJoinResult.Failed("approvedByPlayerId mismatch");
            }

            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return ApproveJoinResult.Failed("Only host can approve join requests");
            }

            CleanupExpired_NoLock();

            GameJoinRequest? request;
            lock (_lock)
            {
                request = _pendingRequests.FirstOrDefault(r => string.Equals(r.RequestId, requestId, StringComparison.Ordinal));
            }

            if (request == null)
            {
                return ApproveJoinResult.Failed("Request not found");
            }

            string joinToken = GenerateJoinToken();
            long expiresAtUtcTicks = DateTime.UtcNow.AddMinutes(_config.JoinRequestTimeoutMinutes).Ticks;

            FullStateSnapshot snapshot = TryCreateFullSnapshot();
            int progress = CalculateGameProgress(snapshot.GameState);

            PlayerBootstrappedState bootstrapped = new()
            {
                PlayerId = request.ClientPlayerId,
                GameProgress = progress,
                Level = CalculateAppropriateLevel(progress),
                MaxHealth = CalculateAppropriateHealth(progress),
                Health = CalculateAppropriateHealth(progress),
                Gold = CalculateAppropriateGold(progress),
                LastEventIndex = snapshot.EventIndex
            };

            if (_config.EnableCompensation)
            {
                bootstrapped.Cards = GenerateStartingCards(progress);
                bootstrapped.Exhibits = GenerateStartingExhibits(progress);
                bootstrapped.Potions = GenerateStartingPotions(progress);
            }

            lock (_lock)
            {
                _issuedJoinTokens[joinToken] = new IssuedJoinToken
                {
                    JoinToken = joinToken,
                    ClientPlayerId = request.ClientPlayerId,
                    RoomId = request.RoomId,
                    ExpiresAtUtcTicks = expiresAtUtcTicks
                };

                _pendingRequests.Remove(request);
            }

            _logger.LogInfo($"[MidGameJoinManager] Join request approved: requestId={requestId}, joinToken={joinToken}, joiner={request.ClientPlayerId}");

            SendDirectMessage(request.ClientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new
            {
                RequestId = requestId,
                Approved = true,
                Reason = (string?)null,
                JoinToken = joinToken,
                ExpiresAtUtcTicks = expiresAtUtcTicks,
                HostPlayerId = approvedByPlayerId,
                RoomId = request.RoomId,
                PlayerName = request.PlayerName,
                BootstrappedState = bootstrapped
            });

            return ApproveJoinResult.Success(joinToken, bootstrapped);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error approving join: {ex.Message}");
            return ApproveJoinResult.Failed($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行中途加入（玩家调用）
    /// 说明：最小可用实现（应用 BootstrappedState + FullSync 快照 + 尽力回放 MissedEvents）。
    /// </summary>
    public JoinExecutionResult ExecuteJoin(string joinToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinToken))
            {
                return JoinExecutionResult.Failed("Missing joinToken");
            }

            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client?.IsConnected != true)
            {
                return JoinExecutionResult.Failed("Not connected");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return JoinExecutionResult.Failed("Missing self playerId");
            }

            ApprovedJoin approvedJoin;
            lock (_lock)
            {
                if (!_approvedJoins.TryGetValue(joinToken, out approvedJoin!))
                {
                    return JoinExecutionResult.Failed("Invalid joinToken (not approved or already consumed)");
                }

                if (DateTime.UtcNow.Ticks > approvedJoin.ExpiresAt)
                {
                    _approvedJoins.Remove(joinToken);
                    return JoinExecutionResult.Failed("JoinToken expired");
                }

                if (!string.Equals(approvedJoin.ClientPlayerId, selfId, StringComparison.Ordinal))
                {
                    return JoinExecutionResult.Failed("JoinToken does not belong to this player");
                }
            }

            _fastSyncService.SyncPlayerState(selfId, approvedJoin.BootstrappedState);

            (FullStateSnapshot? snapshot, List<GameEvent> missedEvents, string? error) = RequestFullStateSync(
                approvedJoin.RoomId,
                selfId,
                approvedJoin.BootstrappedState.LastEventIndex,
                joinToken,
                approvedJoin.HostPlayerId);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return JoinExecutionResult.Failed("Failed to sync full state: " + error);
            }

            if (snapshot != null)
            {
                approvedJoin.BootstrappedState.GameProgress = CalculateGameProgress(snapshot.GameState);
                approvedJoin.BootstrappedState.LastEventIndex = snapshot.EventIndex;
                _logger.LogInfo($"[MidGameJoinManager] FullSnapshot received: eventIndex={snapshot.EventIndex}, progress={approvedJoin.BootstrappedState.GameProgress}%");
            }

            CatchUpResult catchUp = ApplyCatchUpEvents(missedEvents);
            if (!catchUp.IsSuccess)
            {
                _logger.LogWarning($"[MidGameJoinManager] Catch-up replay degraded: {catchUp.ErrorMessage}");
            }

            lock (_lock)
            {
                _approvedJoins.Remove(joinToken);
            }

            _logger.LogInfo($"[MidGameJoinManager] ExecuteJoin completed: playerId={selfId}, applied={catchUp.EventsApplied}");
            return JoinExecutionResult.Success(selfId, approvedJoin.BootstrappedState);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error executing join: {ex.Message}");
            return JoinExecutionResult.Failed($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 计算游戏进度（百分比）。
    /// 说明：不追求严格语义，只提供一个可用于补偿/日志的粗略估计。
    /// </summary>
    private int CalculateGameProgress(GameStateSnapshot gameState)
    {
        if (gameState == null)
        {
            return 0;
        }

        if (gameState.GameEnded)
        {
            return 100;
        }

        if (!gameState.GameStarted)
        {
            return 0;
        }

        int act = Math.Clamp(gameState.CurrentAct, 1, 4);
        int floor = Math.Clamp(gameState.CurrentFloor, 0, 20);

        // 粗略映射：每个 Act 25%，楼层在该 Act 内占 25%。
        double actBase = (act - 1) * 25.0;
        double floorProgress = (floor / 20.0) * 25.0;

        int progress = (int)Math.Round(actBase + floorProgress);
        return Math.Clamp(progress, 0, 99);
    }

    /// <summary>
    /// 生成初始卡牌
    /// 说明：当前为安全默认实现，返回空集合（避免生成非法 ID）；如需启用需接入白名单/游戏数据校验。
    /// </summary>
    private List<string> GenerateStartingCards(int progress)
    {
        // 无法在不依赖游戏数据库的前提下保证卡牌 ID 合法；
        // 为避免生成非法内容，当前默认不补偿卡牌（后续可接入白名单/数据库校验）。
        return [];
    }

    /// <summary>
    /// 生成初始宝物
    /// 说明：当前为安全默认实现，返回空集合（避免生成非法 ID）；如需启用需接入白名单/游戏数据校验。
    /// </summary>
    private List<string> GenerateStartingExhibits(int progress)
    {
        return [];
    }

    /// <summary>
    /// 生成初始药水
    /// 说明：当前为安全默认实现，返回空集合（避免生成非法 ID）；如需启用需接入白名单/游戏数据校验。
    /// </summary>
    private Dictionary<string, int> GenerateStartingPotions(int progress)       
    {
        return [];
    }

    private CatchUpResult ApplyCatchUpEvents(List<GameEvent> missedEvents)
    {
        if (missedEvents == null || missedEvents.Count == 0)
        {
            return CatchUpResult.Success(0);
        }

        NetworkClient? concrete = _concreteClient ?? (_client as NetworkClient);
        if (concrete == null)
        {
            return CatchUpResult.Failed("NetworkClient does not support local event injection");
        }

        int applied = 0;
        int failed = 0;

        foreach (GameEvent e in missedEvents.OrderBy(e => e.Timestamp))
        {
            if (!ShouldReplayEventType(e.EventType))
            {
                continue;
            }

            try
            {
                concrete.InjectLocalGameEvent(e.EventType, e.Data);
                applied++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning($"[MidGameJoinManager] Replay failed: type={e.EventType}, err={ex.Message}");
                if (failed >= 3)
                {
                    return CatchUpResult.Failed("Too many replay failures; degraded to snapshot-only");
                }
            }

            if (_config.CatchUpBatchSize > 0 && applied % _config.CatchUpBatchSize == 0)
            {
                _logger.LogDebug($"[MidGameJoinManager] Catch-up batch applied: {applied}");
            }
        }

        return CatchUpResult.Success(applied);
    }

    /// <summary>
    /// 开始AI托管（代管掉线玩家）
    /// </summary>
    public void StartAIControl(string playerId)
    {
        AIController.StartControlling(playerId);
        _logger.LogInfo($"[MidGameJoinManager] AI started controlling player {playerId}");
    }

    /// <summary>
    /// 停止AI托管（玩家重新连接后）
    /// </summary>
    public void StopAIControl(string playerId)
    {
        AIController.StopControlling(playerId);
        _logger.LogInfo($"[MidGameJoinManager] AI stopped controlling player {playerId}");
    }

    private static string GenerateRequestId() => Guid.NewGuid().ToString("N");
    private static string GenerateJoinToken() => Guid.NewGuid().ToString("N");

    private int CalculateAppropriateLevel(int progress)
        => Math.Clamp(1 + progress / 20, 1, 6);

    private int CalculateAppropriateHealth(int progress)
        => Math.Clamp(60 + progress / 2, 60, 120);

    private int CalculateAppropriateGold(int progress)
        => Math.Clamp(50 + progress * 2, 50, 300);

    private void CleanupExpired_NoLock()
    {
        long now = DateTime.UtcNow.Ticks;

        _pendingRequests.RemoveAll(r => now - r.RequestTime > TimeSpan.FromMinutes(_config.JoinRequestTimeoutMinutes).Ticks);

        foreach ((string key, IssuedJoinToken issued) in _issuedJoinTokens.ToList())
        {
            if (now > issued.ExpiresAtUtcTicks)
            {
                _issuedJoinTokens.Remove(key);
            }
        }
    }

    private FullStateSnapshot TryCreateFullSnapshot()
    {
        try
        {
            ReconnectionManager? reconnection = _serviceProvider.GetService<ReconnectionManager>();
            return reconnection?.CreateFullSnapshot() ?? new FullStateSnapshot
            {
                Timestamp = DateTime.UtcNow.Ticks,
                GameState = new GameStateSnapshot(),
                PlayerStates = [],
                MapState = new MapStateSnapshot(),
                EventIndex = 0,
            };
        }
        catch
        {
            return new FullStateSnapshot
            {
                Timestamp = DateTime.UtcNow.Ticks,
                GameState = new GameStateSnapshot(),
                PlayerStates = [],
                MapState = new MapStateSnapshot(),
                EventIndex = 0,
            };
        }
    }

    private void SendDirectMessage(string targetPlayerId, string innerType, object innerPayload)
    {
        INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
        if (client?.IsConnected != true)
        {
            _logger.LogWarning($"[MidGameJoinManager] DirectMessage dropped (not connected): type={innerType}");
            return;
        }

        client.SendGameEventData("DirectMessage", new
        {
            TargetPlayerId = targetPlayerId,
            Type = innerType,
            Payload = innerPayload,
        });
    }

    private (FullStateSnapshot? snapshot, List<GameEvent> missedEvents, string? error) RequestFullStateSync(
        string roomId,
        string targetPlayerId,
        long lastKnownEventIndex,
        string joinToken,
        string hostPlayerId)
    {
        if (string.IsNullOrWhiteSpace(hostPlayerId))
        {
            return (null, [], "Missing host playerId");
        }

        string requestId = GenerateRequestId();
        PendingFullSyncRequest pending = new();

        lock (_lock)
        {
            _pendingFullSyncRequests[requestId] = pending;
        }

        try
        {
            _logger.LogInfo($"[MidGameJoinManager] FullStateSyncRequest => host={hostPlayerId}, requestId={requestId}, lastIndex={lastKnownEventIndex}");
            SendDirectMessage(hostPlayerId, NetworkMessageTypes.FullStateSyncRequest, new
            {
                RequestId = requestId,
                RoomId = roomId,
                TargetPlayerId = targetPlayerId,
                LastKnownEventIndex = lastKnownEventIndex,
                JoinToken = joinToken
            });

            bool signaled = pending.WaitHandle.Wait(TimeSpan.FromSeconds(10));
            if (!signaled)
            {
                return (null, [], "FullStateSyncResponse timeout");
            }

            if (!string.IsNullOrWhiteSpace(pending.ErrorMessage))
            {
                return (null, [], pending.ErrorMessage);
            }

            return (pending.FullSnapshot, pending.MissedEvents ?? [], null);
        }
        finally
        {
            lock (_lock)
            {
                _pendingFullSyncRequests.Remove(requestId);
            }
            pending.Dispose();
        }
    }

    private bool TryConsumeIssuedJoinToken(string joinToken, string targetPlayerId, string roomId, out string? reason)
    {
        lock (_lock)
        {
            if (!_issuedJoinTokens.TryGetValue(joinToken, out IssuedJoinToken? issued))
            {
                reason = "Invalid joinToken";
                return false;
            }

            if (DateTime.UtcNow.Ticks > issued.ExpiresAtUtcTicks)
            {
                _issuedJoinTokens.Remove(joinToken);
                reason = "JoinToken expired";
                return false;
            }

            if (!string.Equals(issued.ClientPlayerId, targetPlayerId, StringComparison.Ordinal))
            {
                reason = "JoinToken target mismatch";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(roomId) && !string.Equals(issued.RoomId, roomId, StringComparison.Ordinal))
            {
                reason = "JoinToken room mismatch";
                return false;
            }

            _issuedJoinTokens.Remove(joinToken);
            reason = null;
            return true;
        }
    }

    private void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (_lock)
        {
            _lastKnownHostPlayerId = null;
            _pendingRequests.Clear();
            _approvedJoins.Clear();
            _issuedJoinTokens.Clear();

            foreach (PendingFullSyncRequest pending in _pendingFullSyncRequests.Values)
            {
                pending.ErrorMessage = "Disconnected";
                pending.WaitHandle.Set();
            }

            _pendingFullSyncRequests.Clear();
        }
    }

    private void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.PlayerListUpdate:
                TryUpdateHostFromPlayerListUpdate(root);
                return;

            case NetworkMessageTypes.OnGameStart:
                _logger.LogDebug("[MidGameJoinManager] OnGameStart received");
                return;

            case NetworkMessageTypes.MidGameJoinRequest:
                HandleMidGameJoinRequest(root);
                return;

            case NetworkMessageTypes.MidGameJoinResponse:
                HandleMidGameJoinResponse(root);
                return;

            case NetworkMessageTypes.FullStateSyncRequest:
                HandleFullStateSyncRequest(root);
                return;

            case NetworkMessageTypes.FullStateSyncResponse:
                HandleFullStateSyncResponse(root);
                return;
        }
    }

    private void TryUpdateHostFromPlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (JsonElement p in playersElem.EnumerateArray())
        {
            if (p.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!p.TryGetProperty("IsHost", out JsonElement isHostElem) || isHostElem.ValueKind != JsonValueKind.True)
            {
                continue;
            }

            if (!p.TryGetProperty("PlayerId", out JsonElement idElem) || idElem.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            string? hostId = idElem.GetString();
            if (!string.IsNullOrWhiteSpace(hostId))
            {
                lock (_lock)
                {
                    _lastKnownHostPlayerId = hostId;
                }
            }

            return;
        }
    }

    private void HandleMidGameJoinRequest(JsonElement root)
    {
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string? requestId = TryGetString(root, "RequestId");
        string? roomId = TryGetString(root, "RoomId");
        string? playerName = TryGetString(root, "PlayerName");
        string? clientPlayerId = TryGetString(root, "ClientPlayerId");

        if (string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(roomId) ||
            string.IsNullOrWhiteSpace(playerName) ||
            string.IsNullOrWhiteSpace(clientPlayerId))
        {
            _logger.LogWarning("[MidGameJoinManager] Ignore invalid MidGameJoinRequest");
            return;
        }

        if (!_config.AllowMidGameJoin)
        {
            SendDirectMessage(clientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new
            {
                RequestId = requestId,
                Approved = false,
                Reason = "Mid-game joining is disabled"
            });
            return;
        }

        lock (_lock)
        {
            CleanupExpired_NoLock();

            int activeCount = _pendingRequests.Count(r => string.Equals(r.RoomId, roomId, StringComparison.Ordinal));
            if (activeCount >= _config.MaxJoinRequestsPerRoom)
            {
                SendDirectMessage(clientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new
                {
                    RequestId = requestId,
                    Approved = false,
                    Reason = "Too many pending join requests"
                });
                return;
            }

            _pendingRequests.Add(new GameJoinRequest
            {
                RequestId = requestId,
                RoomId = roomId,
                PlayerName = playerName,
                ClientPlayerId = clientPlayerId,
                RequestTime = DateTime.UtcNow.Ticks,
                Status = JoinRequestStatus.Pending
            });
        }

        // 可用优先：自动批准。
        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(selfId))
        {
            return;
        }

        ApproveJoin(requestId, selfId);
    }

    private void HandleMidGameJoinResponse(JsonElement root)
    {
        bool approved = TryGetBool(root, "Approved") == true;
        string? requestId = TryGetString(root, "RequestId");
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return;
        }

        if (!approved)
        {
            string? reason = TryGetString(root, "Reason");
            _logger.LogInfo($"[MidGameJoinManager] Join denied: requestId={requestId}, reason={reason ?? "unknown"}");
            return;
        }

        string? joinToken = TryGetString(root, "JoinToken");
        string? hostPlayerId = TryGetString(root, "HostPlayerId");
        string? roomId = TryGetString(root, "RoomId");
        long expiresAtUtcTicks = TryGetLong(root, "ExpiresAtUtcTicks") ?? 0;

        if (string.IsNullOrWhiteSpace(joinToken) ||
            string.IsNullOrWhiteSpace(hostPlayerId) ||
            string.IsNullOrWhiteSpace(roomId))
        {
            _logger.LogWarning($"[MidGameJoinManager] Invalid MidGameJoinResponse: requestId={requestId}");
            return;
        }

        if (!root.TryGetProperty("BootstrappedState", out JsonElement bsElem) || bsElem.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning($"[MidGameJoinManager] Missing BootstrappedState: requestId={requestId}");
            return;
        }

        PlayerBootstrappedState? bootstrapped = TryDeserialize<PlayerBootstrappedState>(bsElem);
        if (bootstrapped == null)
        {
            _logger.LogWarning($"[MidGameJoinManager] Invalid BootstrappedState: requestId={requestId}");
            return;
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(selfId))
        {
            return;
        }

        ApprovedJoin approvedJoin = new()
        {
            RequestId = requestId,
            RoomId = roomId,
            PlayerName = TryGetString(root, "PlayerName") ?? string.Empty,
            HostPlayerId = hostPlayerId,
            ClientPlayerId = selfId,
            JoinToken = joinToken,
            ApprovedAt = DateTime.UtcNow.Ticks,
            ExpiresAt = expiresAtUtcTicks > 0 ? expiresAtUtcTicks : DateTime.UtcNow.AddMinutes(_config.JoinRequestTimeoutMinutes).Ticks,
            BootstrappedState = bootstrapped,
        };

        lock (_lock)
        {
            _approvedJoins[joinToken] = approvedJoin;
        }

        _logger.LogInfo($"[MidGameJoinManager] Join approved: joinToken={joinToken}, host={hostPlayerId}");
    }

    private void HandleFullStateSyncRequest(JsonElement root)
    {
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string? requestId = TryGetString(root, "RequestId");
        string? roomId = TryGetString(root, "RoomId");
        string? targetPlayerId = TryGetString(root, "TargetPlayerId");
        long lastKnownEventIndex = TryGetLong(root, "LastKnownEventIndex") ?? 0;
        string? joinToken = TryGetString(root, "JoinToken");

        if (string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(targetPlayerId) ||
            string.IsNullOrWhiteSpace(joinToken))
        {
            return;
        }

        if (!TryConsumeIssuedJoinToken(joinToken, targetPlayerId, roomId ?? string.Empty, out string? denial))
        {
            SendDirectMessage(targetPlayerId, NetworkMessageTypes.FullStateSyncResponse, new
            {
                RequestId = requestId,
                TargetPlayerId = targetPlayerId,
                ErrorMessage = denial ?? "Denied",
                ServerTimeUtcTicks = DateTime.UtcNow.Ticks
            });
            return;
        }

        ReconnectionManager? reconnection = _serviceProvider.GetService<ReconnectionManager>();
        FullStateSnapshot snapshot = reconnection?.CreateFullSnapshot() ?? TryCreateFullSnapshot();
        List<GameEvent> missed = reconnection?.GetMissedEvents(lastKnownEventIndex) ?? [];

        SendDirectMessage(targetPlayerId, NetworkMessageTypes.FullStateSyncResponse, new
        {
            RequestId = requestId,
            TargetPlayerId = targetPlayerId,
            FullSnapshot = snapshot,
            MissedEvents = missed,
            ServerTimeUtcTicks = DateTime.UtcNow.Ticks
        });
    }

    private void HandleFullStateSyncResponse(JsonElement root)
    {
        string? requestId = TryGetString(root, "RequestId");
        string? targetPlayerId = TryGetString(root, "TargetPlayerId");
        if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(targetPlayerId))
        {
            return;
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(selfId) || !string.Equals(selfId, targetPlayerId, StringComparison.Ordinal))
        {
            return;
        }

        PendingFullSyncRequest? pending;
        lock (_lock)
        {
            _pendingFullSyncRequests.TryGetValue(requestId, out pending);
        }

        if (pending == null)
        {
            return;
        }

        pending.ErrorMessage = TryGetString(root, "ErrorMessage");

        if (root.TryGetProperty("FullSnapshot", out JsonElement snapElem) && snapElem.ValueKind == JsonValueKind.Object)
        {
            pending.FullSnapshot = TryDeserialize<FullStateSnapshot>(snapElem);
        }

        if (root.TryGetProperty("MissedEvents", out JsonElement eventsElem) && eventsElem.ValueKind == JsonValueKind.Array)
        {
            pending.MissedEvents = TryDeserialize<List<GameEvent>>(eventsElem) ?? [];
        }

        pending.WaitHandle.Set();
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            if (payload is string s)
            {
                root = JsonSerializer.Deserialize<JsonElement>(s);
                return true;
            }

            root = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(payload));
            return true;
        }
        catch
        {
            root = default;
            return false;
        }
    }

    private static string? TryGetString(JsonElement root, string property)
    {
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(property, out JsonElement p) &&
            p.ValueKind == JsonValueKind.String)
        {
            return p.GetString();
        }

        return null;
    }

    private static bool? TryGetBool(JsonElement root, string property)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(property, out JsonElement p))
        {
            return null;
        }

        return p.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) ? b : null,
            _ => null
        };
    }

    private static long? TryGetLong(JsonElement root, string property)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(property, out JsonElement p))
        {
            return null;
        }

        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetInt64(out long l) => l,
            JsonValueKind.String when long.TryParse(p.GetString(), out long l) => l,
            _ => null
        };
    }

    private static T? TryDeserialize<T>(JsonElement elem) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(elem.GetRawText());
        }
        catch
        {
            return null;
        }
    }

    private static bool ShouldReplayEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return false;
        }

        if (string.Equals(eventType, NetworkMessageTypes.FullStateSyncRequest, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.FullStateSyncResponse, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.MidGameJoinRequest, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.MidGameJoinResponse, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.Welcome, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.PlayerListUpdate, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.PlayerJoined, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.PlayerLeft, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.HostChanged, StringComparison.Ordinal))
        {
            return false;
        }

        return eventType.StartsWith("On", StringComparison.Ordinal) ||
               eventType.StartsWith("Mana", StringComparison.Ordinal) ||
               eventType.StartsWith("Gap", StringComparison.Ordinal) ||
               eventType.StartsWith("Battle", StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.EnemySpawned, StringComparison.Ordinal);
    }
}

/// <summary>
/// 中途加入配置
/// </summary>
public enum JoinRequestStatus
{
    Pending,
    Approved,
    Denied,
    Expired
}
