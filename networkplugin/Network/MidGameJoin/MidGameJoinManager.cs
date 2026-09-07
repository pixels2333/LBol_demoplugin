#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using BepInEx.Logging;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.MidGameJoin.Result;
using NetworkPlugin.Network.Reconnection;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.MidGameJoin;

public sealed class MidGameJoinManager
{
    #region 内部字段（类自己的数据）

        private readonly ManualLogSource _logger;
        private readonly INetworkClient _client;
        private readonly ReconnectionManager _reconnectionManager;
        private readonly MapCatchUpOrchestrator _mapCatchUp;
        private readonly MidGameJoinConfig _config;
        private NetworkClient? _concreteClient;
        private int _initialized;
        private string? _lastKnownHostPlayerId;

    #endregion

    #region 内部嵌套类型

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
                public void Dispose()
        {
            WaitHandle.Dispose();
        }
    }

    #endregion

    #region 数据集合

        private readonly List<GameJoinRequest> _pendingRequests;
        private readonly Dictionary<string, ApprovedJoin> _approvedJoins;
        private readonly Dictionary<string, IssuedJoinToken> _issuedJoinTokens = [];
        private readonly Dictionary<string, PendingFullSyncRequest> _pendingFullSyncRequests = [];

        private readonly Dictionary<string, string> _approvedByRequestId = [];

        private readonly object _lock = new();

        private readonly FastSyncService _fastSyncService;

    #endregion

    #region 构造函数

        public MidGameJoinManager(
        MidGameJoinConfig config,
        ManualLogSource logger,
        INetworkClient networkClient,
        ReconnectionManager reconnectionManager,
        MapCatchUpOrchestrator mapCatchUp)
    {
        _config = config ?? new MidGameJoinConfig();
        _logger = logger ?? Plugin.Logger;
        _client = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
        _reconnectionManager = reconnectionManager ?? throw new ArgumentNullException(nameof(reconnectionManager));
        _mapCatchUp = mapCatchUp ?? throw new ArgumentNullException(nameof(mapCatchUp));
        _pendingRequests = [];
        _approvedJoins = [];
        _fastSyncService = new FastSyncService(_logger);
    }

    #endregion

    #region 公共方法

        public void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            _logger.LogDebug("[MidGameJoinManager] Initialize skipped (already initialized)");
            return;
        }

        try
        {
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

        public JoinRequestResult RequestJoin(string roomId, string playerName)
    {
        try
        {
            if (!_config.AllowMidGameJoin)
            {
                return JoinRequestResult.Denied("已禁用中途加入");
            }

            if (string.IsNullOrWhiteSpace(roomId))
            {
                return JoinRequestResult.Denied("缺少 roomId");
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                return JoinRequestResult.Denied("缺少 playerName");
            }

            INetworkClient? client = _client;
            if (client?.IsConnected != true)
            {
                return JoinRequestResult.Denied("未连接到服务器");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return JoinRequestResult.Denied("缺少自身 playerId（请等待 Welcome/PlayerListUpdate）");
            }

            string? hostId;
            lock (_lock)
            {
                hostId = _lastKnownHostPlayerId;
            }

            if (string.IsNullOrWhiteSpace(hostId))
            {
                return JoinRequestResult.Denied("未找到房主（请先加入房间并等待 PlayerListUpdate）");
            }

            if (string.Equals(hostId, selfId, StringComparison.Ordinal))
            {
                return JoinRequestResult.Denied("你已是房主");
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
                bootstrapped.ToolCards = GenerateStartingToolCards(progress);
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

        public JoinExecutionResult ExecuteJoin(string joinToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinToken))
            {
                return JoinExecutionResult.Failed("缺少 joinToken");
            }

            INetworkClient? client = _client;
            if (client?.IsConnected != true)
            {
                return JoinExecutionResult.Failed("未连接到服务器");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return JoinExecutionResult.Failed("缺少自身 playerId");
            }

            ApprovedJoin approvedJoin;
            lock (_lock)
            {
                if (!_approvedJoins.TryGetValue(joinToken, out approvedJoin!))
                {
                    return JoinExecutionResult.Failed("joinToken 无效（未批准或已被消耗）");
                }

                if (DateTime.UtcNow.Ticks > approvedJoin.ExpiresAt)
                {
                    _approvedJoins.Remove(joinToken);
                    return JoinExecutionResult.Failed("joinToken 已过期");
                }

                if (!string.Equals(approvedJoin.ClientPlayerId, selfId, StringComparison.Ordinal))
                {
                    return JoinExecutionResult.Failed("joinToken 不属于当前玩家");
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
                return JoinExecutionResult.Failed("完整状态同步失败: " + error);
            }

            if (snapshot != null)
            {
                approvedJoin.BootstrappedState.GameProgress = CalculateGameProgress(snapshot.GameState);
                approvedJoin.BootstrappedState.LastEventIndex = snapshot.EventIndex;
                _logger.LogInfo($"[MidGameJoinManager] FullSnapshot received: eventIndex={snapshot.EventIndex}, progress={approvedJoin.BootstrappedState.GameProgress}%");

                try
                {
                    _mapCatchUp.SetPendingFullSnapshot(snapshot);
                }
                catch
                {

                }
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

        public string? GetLastKnownHostPlayerId()
    {
        lock (_lock)
        {
            return _lastKnownHostPlayerId;
        }
    }

        public bool TryGetApprovedJoinTokenByRequestId(string requestId, out string joinToken, bool consume = false)
    {
        joinToken = string.Empty;

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return false;
        }

        lock (_lock)
        {
            if (!_approvedByRequestId.TryGetValue(requestId, out joinToken) || string.IsNullOrWhiteSpace(joinToken))
            {
                joinToken = string.Empty;
                return false;
            }

            if (consume)
            {
                _approvedByRequestId.Remove(requestId);
            }

            return true;
        }
    }

        public void BeginReconnectAndCatchUp(
        string roomId,
        string playerName,
        Action<string>? onStatus,
        Action<JoinExecutionResult>? onCompleted,
        int timeoutSeconds = 20)
    {
        try
        {
            INetworkClient? client = _client;
            if (client?.IsConnected != true)
            {
                Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("未连接到服务器")));
                return;
            }

            JoinRequestResult req = RequestJoin(roomId, playerName);
            if (req == null || string.IsNullOrWhiteSpace(req.RequestId))
            {
                Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed(req?.ErrorMessage ?? "加入请求失败")));
                return;
            }

            string requestId = req.RequestId;
            Plugin.RunOnMainThread(() => onStatus?.Invoke($"已发送加入请求 (requestId={requestId})"));

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    long start = DateTime.UtcNow.Ticks;
                    long timeoutTicks = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)).Ticks;

                    string token = string.Empty;
                    while (DateTime.UtcNow.Ticks - start < timeoutTicks)
                    {
                        if (TryGetApprovedJoinTokenByRequestId(requestId, out token, consume: true) && !string.IsNullOrWhiteSpace(token))
                        {
                            break;
                        }

                        Thread.Sleep(50);
                    }

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("加入审批超时")));
                        return;
                    }

                    Plugin.RunOnMainThread(() => onStatus?.Invoke("加入已批准，正在同步..."));
                    JoinExecutionResult result = ExecuteJoin(token);
                    Plugin.RunOnMainThread(() => onCompleted?.Invoke(result));
                }
                catch (Exception ex)
                {
                    Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("重连失败: " + ex.Message)));
                }
            });
        }
        catch (Exception ex)
        {
            Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("重连失败: " + ex.Message)));
        }
    }

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

        double actBase = (act - 1) * 25.0;
        double floorProgress = (floor / 20.0) * 25.0;

        int result = (int)Math.Round(actBase + floorProgress);
        return Math.Clamp(result, 0, 99);
    }

        private List<string> GenerateStartingCards(int progress)
    {

        return [];
    }

        private List<string> GenerateStartingExhibits(int progress)
    {
        return [];
    }

        private Dictionary<string, int> GenerateStartingToolCards(int progress)
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
            ReconnectionManager? reconnection = _reconnectionManager;
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
        INetworkClient? client = _client;
        if (client?.IsConnected != true)
        {
            _logger.LogWarning($"[MidGameJoinManager] DirectMessage 已丢弃（未连接）: type={innerType}");
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
            return (null, new List<GameEvent>(), "Missing host playerId");
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
                reason = "JoinToken 房间不匹配";
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
                pending.ErrorMessage = "连接已断开";
                pending.WaitHandle.Set();
            }

            _pendingFullSyncRequests.Clear();
        }
    }
    #endregion

    #region 私有方法 — 网络事件处理

        private void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            try
            {
                string payloadType = payload?.GetType().FullName ?? "<null>";
                string? head = payload as string;
                if (head != null)
                {
                    head = head.Replace("\r", " ").Replace("\n", " ");
                    if (head.Length > 200)
                    {
                        head = head.Substring(0, 200);
                    }
                }

                Plugin.Logger?.LogWarning($"[MidGameJoin] 忽略无效载荷事件: type={eventType}, payloadType={payloadType}, head200={head}");
            }
            catch
            {

            }

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

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(selfId))
        {
            return;
        }

        ApproveJoinResult approved = ApproveJoin(requestId, selfId);
        if (string.IsNullOrWhiteSpace(approved.JoinToken))
        {
            SendDirectMessage(clientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new
            {
                RequestId = requestId,
                Approved = false,
                Reason = approved.ErrorMessage ?? "Join request denied"
            });
        }
    }

        private void HandleMidGameJoinResponse(JsonElement root)
    {
        string? requestId = TryGetString(root, "RequestId");
        bool? approved = TryGetBool(root, "Approved");

        if (string.IsNullOrWhiteSpace(requestId) || approved != true)
        {
            string? reason = TryGetString(root, "Reason");
            _logger.LogWarning($"[MidGameJoinManager] Join denied: requestId={requestId}, reason={reason}");
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

            if (!string.IsNullOrWhiteSpace(requestId))
            {
                _approvedByRequestId[requestId] = joinToken;
            }
        }

        _logger.LogInfo($"[MidGameJoinManager] Join approved: joinToken=<已脱敏>, host={hostPlayerId}");
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

        ReconnectionManager? reconnection = _reconnectionManager;
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

            root = JsonCompat.ToJsonElement(payload);
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

        if (eventType.StartsWith("On", StringComparison.Ordinal) ||
            eventType.StartsWith("Mana", StringComparison.Ordinal) ||
            eventType.StartsWith("Gap", StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(eventType, NetworkMessageTypes.BattlePlayerDamageReport, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerDamageBroadcast, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerHealReport, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerHealBroadcast, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerStatusEffectsDeltaReport, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerStatusEffectsDeltaBroadcast, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerStatusEffectsFullReport, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattlePlayerStatusEffectsFullBroadcast, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattleEnemySpawned, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.EnemySpawned, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattleEnemyIntentChanged, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.BattleEnemyStateChanged, StringComparison.Ordinal) ||
               string.Equals(eventType, NetworkMessageTypes.EnemyStateUpdate, StringComparison.Ordinal);
    }

    #endregion
}

public enum JoinRequestStatus
{
        Pending,
        Approved,
        Denied,
        Expired
}
