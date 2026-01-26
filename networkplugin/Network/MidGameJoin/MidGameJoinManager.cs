

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using BepInEx.Logging;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
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
    #region 私有类型定义
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

        public void Dispose()
        {
            WaitHandle.Dispose();
        }
    }

    #endregion

    #region 字段声明

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

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化中途加入管理器
    /// </summary>
    /// <param name="config">中途加入配置</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="serviceProvider">服务提供者</param>
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

    #endregion

    #region 公共方法

    /// <summary>
    /// 初始化中途加入管理器
    /// 说明：订阅网络事件并维护 Host/玩家列表快照。
    /// </summary>
    public void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1) // 原子操作确保只初始化一次
        {
            _logger.LogDebug("[MidGameJoinManager] Initialize skipped (already initialized)");
            return;
        }

        try
        {
            _client = _serviceProvider.GetService<INetworkClient>();
            _concreteClient = _client as NetworkClient; // 保存具体类型引用用于事件注入

            if (_client == null)
            {
                _logger.LogWarning("[MidGameJoinManager] Initialize skipped: INetworkClient not available");
                Interlocked.Exchange(ref _initialized, 0); // 重置初始化状态
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(_client); // 确保网络身份跟踪器已订阅

            _client.OnGameEventReceived += OnGameEventReceived; // 订阅游戏事件
            _client.OnConnectionStateChanged += OnConnectionStateChanged; // 订阅连接状态变更

            _logger.LogInfo("[MidGameJoinManager] Initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Initialize failed: {ex.Message}");
            Interlocked.Exchange(ref _initialized, 0); // 异常时重置初始化状态
        }
    }

    /// <summary>
    /// 请求中途加入游戏
    /// 说明：最小可用实现（校验连接/PlayerId/HostId），通过 DirectMessage 向 Host 发起请求。
    /// </summary>
    /// <param name="roomId">房间ID</param>
    /// <param name="playerName">玩家名称</param>
    /// <returns>加入请求结果（Pending/Denied）</returns>
    public JoinRequestResult RequestJoin(string roomId, string playerName)      
    {
        try
        {
            if (!_config.AllowMidGameJoin) // 配置禁用则直接拒绝
            {
                return JoinRequestResult.Denied("Mid-game joining is disabled");
            }

            if (string.IsNullOrWhiteSpace(roomId)) // roomId 为空则拒绝
            {
                return JoinRequestResult.Denied("Missing roomId");
            }

            if (string.IsNullOrWhiteSpace(playerName)) // playerName 为空则拒绝
            {
                return JoinRequestResult.Denied("Missing playerName");
            }

            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client?.IsConnected != true) // 未连接则拒绝
            {
                return JoinRequestResult.Denied("Not connected");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return JoinRequestResult.Denied("Missing self playerId (wait for Welcome/PlayerListUpdate)");
            }

            string? hostId;
            lock (_lock) // 线程安全读取上次记录的房主ID
            {
                hostId = _lastKnownHostPlayerId;
            }

            if (string.IsNullOrWhiteSpace(hostId)) // 未获取到房主则拒绝
            {
                return JoinRequestResult.Denied("Host not found (join room first and wait for PlayerListUpdate)");
            }

            string requestId = GenerateRequestId(); // 生成唯一请求ID
            _logger.LogInfo($"[MidGameJoinManager] RequestJoin => host={hostId}, room={roomId}, requestId={requestId}");

            SendDirectMessage(hostId, NetworkMessageTypes.MidGameJoinRequest, new // 通过 DirectMessage 向房主发起加入请求
            {
                RequestId = requestId,
                RoomId = roomId,
                PlayerName = playerName,
                ClientPlayerId = selfId,
                ClientTimeUtcTicks = DateTime.UtcNow.Ticks
            });

            return JoinRequestResult.Pending(requestId); // 返回待处理状态
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
    /// <param name="requestId">请求ID</param>
    /// <param name="approvedByPlayerId">批准者玩家ID</param>
    /// <returns>批准结果（Success/Failed）</returns>
    public ApproveJoinResult ApproveJoin(string requestId, string approvedByPlayerId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(requestId)) // requestId 为空则失败
            {
                return ApproveJoinResult.Failed("Missing requestId");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return ApproveJoinResult.Failed("Missing self playerId");
            }

            if (!string.Equals(selfId, approvedByPlayerId, StringComparison.Ordinal)) // 仅允许本人批准（防冒用）
            {
                return ApproveJoinResult.Failed("approvedByPlayerId mismatch");
            }

            if (!NetworkIdentityTracker.GetSelfIsHost()) // 非房主不允许批准
            {
                return ApproveJoinResult.Failed("Only host can approve join requests");
            }

            CleanupExpired_NoLock(); // 清理过期请求与令牌

            GameJoinRequest? request;
            lock (_lock) // 线程安全查找待处理请求
            {
                request = _pendingRequests.FirstOrDefault(r => string.Equals(r.RequestId, requestId, StringComparison.Ordinal));
            }

            if (request == null) // 请求不存在
            {
                return ApproveJoinResult.Failed("Request not found");
            }

            string joinToken = GenerateJoinToken(); // 生成一次性加入令牌
            long expiresAtUtcTicks = DateTime.UtcNow.AddMinutes(_config.JoinRequestTimeoutMinutes).Ticks; // 计算过期时间

            FullStateSnapshot snapshot = TryCreateFullSnapshot(); // 创建完整快照用于估算进度
            int progress = CalculateGameProgress(snapshot.GameState); // 计算游戏进度

            PlayerBootstrappedState bootstrapped = new() // 创建玩家引导状态
            {
                PlayerId = request.ClientPlayerId,
                GameProgress = progress,
                Level = CalculateAppropriateLevel(progress),
                MaxHealth = CalculateAppropriateHealth(progress),
                Health = CalculateAppropriateHealth(progress),
                Gold = CalculateAppropriateGold(progress),
                LastEventIndex = snapshot.EventIndex
            };

            if (_config.EnableCompensation) // 启用补偿时生成起始资源（当前安全默认为空）
            {
                bootstrapped.Cards = GenerateStartingCards(progress);
                bootstrapped.Exhibits = GenerateStartingExhibits(progress);
                bootstrapped.Potions = GenerateStartingPotions(progress);
            }

            lock (_lock) // 线程安全存储令牌并移除请求
            {
                _issuedJoinTokens[joinToken] = new IssuedJoinToken
                {
                    JoinToken = joinToken,
                    ClientPlayerId = request.ClientPlayerId,
                    RoomId = request.RoomId,
                    ExpiresAtUtcTicks = expiresAtUtcTicks
                };

                _pendingRequests.Remove(request); // 移除已处理的请求
            }

            _logger.LogInfo($"[MidGameJoinManager] Join request approved: requestId={requestId}, joinToken={joinToken}, joiner={request.ClientPlayerId}");

            SendDirectMessage(request.ClientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new // 向请求方发送批准响应
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

            return ApproveJoinResult.Success(joinToken, bootstrapped); // 返回成功结果
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
    /// <param name="joinToken">加入令牌</param>
    /// <returns>加入执行结果（Success/Failed）</returns>
    public JoinExecutionResult ExecuteJoin(string joinToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinToken)) // joinToken 为空则失败
            {
                return JoinExecutionResult.Failed("Missing joinToken");
            }

            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client?.IsConnected != true) // 未连接则无法执行加入
            {
                return JoinExecutionResult.Failed("Not connected");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
            if (string.IsNullOrWhiteSpace(selfId))
            {
                return JoinExecutionResult.Failed("Missing self playerId");
            }

            ApprovedJoin approvedJoin;
            lock (_lock) // 线程安全读取已批准的加入信息
            {
                if (!_approvedJoins.TryGetValue(joinToken, out approvedJoin!)) // 查找批准的加入请求
                {
                    return JoinExecutionResult.Failed("Invalid joinToken (not approved or already consumed)");
                }

                if (DateTime.UtcNow.Ticks > approvedJoin.ExpiresAt) // 过期则清理并失败
                {
                    _approvedJoins.Remove(joinToken); // 清理过期令牌
                    return JoinExecutionResult.Failed("JoinToken expired");
                }

                if (!string.Equals(approvedJoin.ClientPlayerId, selfId, StringComparison.Ordinal)) // 验证令牌归属当前玩家
                {
                    return JoinExecutionResult.Failed("JoinToken does not belong to this player");
                }
            }

            _fastSyncService.SyncPlayerState(selfId, approvedJoin.BootstrappedState); // 应用引导状态到本地玩家

            (FullStateSnapshot? snapshot, List<GameEvent> missedEvents, string? error) = RequestFullStateSync( // 请求完整状态同步
                approvedJoin.RoomId,
                selfId,
                approvedJoin.BootstrappedState.LastEventIndex,
                joinToken,
                approvedJoin.HostPlayerId);

            if (!string.IsNullOrWhiteSpace(error)) // 同步失败直接返回错误
            {
                return JoinExecutionResult.Failed("Failed to sync full state: " + error);
            }

            if (snapshot != null) // 根据快照刷新进度与事件索引
            {
                approvedJoin.BootstrappedState.GameProgress = CalculateGameProgress(snapshot.GameState);
                approvedJoin.BootstrappedState.LastEventIndex = snapshot.EventIndex;
                _logger.LogInfo($"[MidGameJoinManager] FullSnapshot received: eventIndex={snapshot.EventIndex}, progress={approvedJoin.BootstrappedState.GameProgress}%");

                // 将 MapState 暂存给追赶执行器：本地 GameRun 可用后再尽力应用。
                try
                {
                    _serviceProvider.GetService<MapCatchUpOrchestrator>()?.SetPendingFullSnapshot(snapshot);
                }
                catch
                {
                    // ignored
                }
            }

            CatchUpResult catchUp = ApplyCatchUpEvents(missedEvents); // 尽力回放错过事件
            if (!catchUp.IsSuccess)
            {
                _logger.LogWarning($"[MidGameJoinManager] Catch-up replay degraded: {catchUp.ErrorMessage}");
            }

            lock (_lock) // 线程安全移除已消费的批准记录
            {
                _approvedJoins.Remove(joinToken);
            }

            _logger.LogInfo($"[MidGameJoinManager] ExecuteJoin completed: playerId={selfId}, applied={catchUp.EventsApplied}");
            return JoinExecutionResult.Success(selfId, approvedJoin.BootstrappedState); // 返回成功结果
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
    /// <param name="gameState">游戏状态快照</param>
    /// <returns>游戏进度百分比（0-100）</returns>
    private int CalculateGameProgress(GameStateSnapshot gameState)
    {
        if (gameState == null) // 空快照视为未开始
        {
            return 0;
        }

        if (gameState.GameEnded) // 游戏结束视为 100%
        {
            return 100;
        }

        if (!gameState.GameStarted) // 游戏未开始视为 0%
        {
            return 0;
        }

        int act = Math.Clamp(gameState.CurrentAct, 1, 4); // Act 范围裁剪到 1-4
        int floor = Math.Clamp(gameState.CurrentFloor, 0, 20); // Floor 范围裁剪到 0-20

        // 粗略映射：每个 Act 25%，楼层在该 Act 内占 25%。
        double actBase = (act - 1) * 25.0; // Act 基础进度
        double floorProgress = (floor / 20.0) * 25.0; // Act 内楼层进度

        int progress = (int)Math.Round(actBase + floorProgress); // 合并并四舍五入
        return Math.Clamp(progress, 0, 99); // 约束在 0-99（结束态单独返回 100）
    }

    /// <summary>
    /// 生成初始卡牌
    /// 说明：当前为安全默认实现，返回空集合（避免生成非法 ID）；如需启用需接入白名单/游戏数据校验。
    /// </summary>
    /// <param name="progress">游戏进度百分比</param>
    /// <returns>初始卡牌ID列表（当前返回空列表）</returns>
    private List<string> GenerateStartingCards(int progress)
    {
        // 安全默认：不生成卡牌，避免非法 ID 导致异常。
        return [];
    }

    /// <summary>
    /// 生成初始宝物
    /// 说明：当前为安全默认实现，返回空集合（避免生成非法 ID）；如需启用需接入白名单/游戏数据校验。
    /// </summary>
    /// <param name="progress">游戏进度百分比</param>
    /// <returns>初始宝物ID列表（当前返回空列表）</returns>
    private List<string> GenerateStartingExhibits(int progress)
    {
        return [];
    }

    /// <summary>
    /// 生成初始药水
    /// 说明：当前为安全默认实现，返回空集合（避免生成非法 ID）；如需启用需接入白名单/游戏数据校验。
    /// </summary>
    /// <param name="progress">游戏进度百分比</param>
    /// <returns>初始药水ID到数量的字典（当前返回空字典）</returns>
    private Dictionary<string, int> GenerateStartingPotions(int progress)       
    {
        return [];
    }

    /// <summary>
    /// 应用追赶事件（回放错过的游戏事件）
    /// </summary>
    /// <param name="missedEvents">错过的游戏事件列表</param>
    /// <returns>追赶结果，包含成功应用的事件数量</returns>
    private CatchUpResult ApplyCatchUpEvents(List<GameEvent> missedEvents)
    {
        if (missedEvents == null || missedEvents.Count == 0) // 无错过事件则无需回放
        {
            return CatchUpResult.Success(0);
        }

        NetworkClient? concrete = _concreteClient ?? (_client as NetworkClient); // 获取 NetworkClient 以支持本地注入
        if (concrete == null) // 不支持注入则降级为仅快照
        {
            return CatchUpResult.Failed("NetworkClient does not support local event injection");
        }

        int applied = 0; // 成功应用的事件计数
        int failed = 0; // 失败事件计数

        foreach (GameEvent e in missedEvents.OrderBy(e => e.Timestamp)) // 按时间戳顺序回放
        {
            if (!ShouldReplayEventType(e.EventType)) // 检查事件类型是否应该回放
            {
                continue;
            }

            try
            {
                concrete.InjectLocalGameEvent(e.EventType, e.Data); // 注入本地事件（触发同一套事件处理）
                applied++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning($"[MidGameJoinManager] Replay failed: type={e.EventType}, err={ex.Message}");
                if (failed >= 3) // 失败次数过多时停止回放并降级
                {
                    return CatchUpResult.Failed("Too many replay failures; degraded to snapshot-only");
                }
            }

            if (_config.CatchUpBatchSize > 0 && applied % _config.CatchUpBatchSize == 0) // 按批次输出进度日志
            {
                _logger.LogDebug($"[MidGameJoinManager] Catch-up batch applied: {applied}");
            }
        }

        return CatchUpResult.Success(applied); // 返回成功应用的事件数量
    }

    /// <summary>
    /// 开始AI托管（代管掉线玩家）
    /// </summary>
    /// <param name="playerId">要托管的玩家ID</param>
    public void StartAIControl(string playerId)
    {
        AIController.StartControlling(playerId);
        _logger.LogInfo($"[MidGameJoinManager] AI started controlling player {playerId}");
    }

    /// <summary>
    /// 停止AI托管（玩家重新连接后）
    /// </summary>
    /// <param name="playerId">要停止托管的玩家ID</param>
    public void StopAIControl(string playerId)
    {
        AIController.StopControlling(playerId);
        _logger.LogInfo($"[MidGameJoinManager] AI stopped controlling player {playerId}");
    }

    /// <summary>
    /// 生成请求ID（GUID无横线格式）
    /// </summary>
    /// <returns>请求ID字符串</returns>
    private static string GenerateRequestId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// 生成加入令牌（GUID无横线格式）
    /// </summary>
    /// <returns>加入令牌字符串</returns>
    private static string GenerateJoinToken() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// 计算适当等级（基于游戏进度）
    /// </summary>
    /// <param name="progress">游戏进度百分比</param>
    /// <returns>1到6之间的等级</returns>
    private int CalculateAppropriateLevel(int progress)
        => Math.Clamp(1 + progress / 20, 1, 6);

    /// <summary>
    /// 计算适当生命值（基于游戏进度）
    /// </summary>
    /// <param name="progress">游戏进度百分比</param>
    /// <returns>60到120之间的生命值</returns>
    private int CalculateAppropriateHealth(int progress)
        => Math.Clamp(60 + progress / 2, 60, 120);

    /// <summary>
    /// 计算适当金币（基于游戏进度）
    /// </summary>
    /// <param name="progress">游戏进度百分比</param>
    /// <returns>50到300之间的金币数量</returns>
    private int CalculateAppropriateGold(int progress)
        => Math.Clamp(50 + progress * 2, 50, 300);

    /// <summary>
    /// 清理过期的请求和令牌（非线程安全，需在锁内调用）
    /// </summary>
    private void CleanupExpired_NoLock()
    {
        long now = DateTime.UtcNow.Ticks; // 当前时间戳

        _pendingRequests.RemoveAll(r => now - r.RequestTime > TimeSpan.FromMinutes(_config.JoinRequestTimeoutMinutes).Ticks); // 清理超时的加入请求

        foreach ((string key, IssuedJoinToken issued) in _issuedJoinTokens.ToList()) // 遍历已颁发的令牌
        {
            if (now > issued.ExpiresAtUtcTicks) // 检查令牌是否过期
            {
                _issuedJoinTokens.Remove(key); // 移除过期的加入令牌
            }
        }
    }

    /// <summary>
    /// 尝试创建完整状态快照（用于中途加入）
    /// </summary>
    /// <returns>完整状态快照；失败时返回空快照</returns>
    private FullStateSnapshot TryCreateFullSnapshot()
    {
        try
        {
            ReconnectionManager? reconnection = _serviceProvider.GetService<ReconnectionManager>(); // 获取重连管理器
            return reconnection?.CreateFullSnapshot() ?? new FullStateSnapshot // 优先使用重连系统的快照
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
            return new FullStateSnapshot // 异常时返回空快照
            {
                Timestamp = DateTime.UtcNow.Ticks,
                GameState = new GameStateSnapshot(),
                PlayerStates = [],
                MapState = new MapStateSnapshot(),
                EventIndex = 0,
            };
        }
    }

    /// <summary>
    /// 发送直接消息给指定玩家
    /// </summary>
    /// <param name="targetPlayerId">目标玩家ID</param>
    /// <param name="innerType">内部消息类型</param>
    /// <param name="innerPayload">内部消息载荷</param>
    private void SendDirectMessage(string targetPlayerId, string innerType, object innerPayload)
    {
        INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>(); // 获取网络客户端
        if (client?.IsConnected != true) // 检查连接状态
        {
            _logger.LogWarning($"[MidGameJoinManager] DirectMessage dropped (not connected): type={innerType}");
            return;
        }

        client.SendGameEventData("DirectMessage", new // 统一用 DirectMessage 封装点对点消息
        {
            TargetPlayerId = targetPlayerId,
            Type = innerType,
            Payload = innerPayload,
        });
    }

    /// <summary>
    /// 请求完整状态同步（用于中途加入后追赶）
    /// </summary>
    /// <param name="roomId">房间ID</param>
    /// <param name="targetPlayerId">目标玩家ID（请求者）</param>
    /// <param name="lastKnownEventIndex">最后已知事件索引</param>
    /// <param name="joinToken">加入令牌</param>
    /// <param name="hostPlayerId">房主玩家ID</param>
    /// <returns>快照、错过的事件列表和错误消息（如果有）</returns>
    private (FullStateSnapshot? snapshot, List<GameEvent> missedEvents, string? error) RequestFullStateSync(
        string roomId,
        string targetPlayerId,
        long lastKnownEventIndex,
        string joinToken,
        string hostPlayerId)
    {
        if (string.IsNullOrWhiteSpace(hostPlayerId)) // 验证房主ID
        {
            return (null, new List<GameEvent>(), "Missing host playerId");
        }

        string requestId = GenerateRequestId(); // 生成请求ID
        PendingFullSyncRequest pending = new(); // 创建待处理请求

        lock (_lock) // 线程安全存储待处理请求
        {
            _pendingFullSyncRequests[requestId] = pending;
        }

        try
        {
            _logger.LogInfo($"[MidGameJoinManager] FullStateSyncRequest => host={hostPlayerId}, requestId={requestId}, lastIndex={lastKnownEventIndex}");
            SendDirectMessage(hostPlayerId, NetworkMessageTypes.FullStateSyncRequest, new // 发送完整状态同步请求
            {
                RequestId = requestId,
                RoomId = roomId,
                TargetPlayerId = targetPlayerId,
                LastKnownEventIndex = lastKnownEventIndex,
                JoinToken = joinToken
            });

            bool signaled = pending.WaitHandle.Wait(TimeSpan.FromSeconds(10)); // 等待 FullStateSyncResponse（超时 10 秒）
            if (!signaled) // 超时处理
            {
                return (null, [], "FullStateSyncResponse timeout");
            }

            if (!string.IsNullOrWhiteSpace(pending.ErrorMessage)) // 检查错误消息
            {
                return (null, [], pending.ErrorMessage);
            }

            return (pending.FullSnapshot, pending.MissedEvents ?? [], null); // 返回结果
        }
        finally
        {
            lock (_lock) // 清理待处理请求
            {
                _pendingFullSyncRequests.Remove(requestId);
            }
            pending.Dispose(); // 释放等待句柄
        }
    }

    /// <summary>
    /// 尝试消费已颁发的加入令牌（验证并移除）
    /// </summary>
    /// <param name="joinToken">加入令牌</param>
    /// <param name="targetPlayerId">目标玩家ID</param>
    /// <param name="roomId">房间ID</param>
    /// <param name="reason">失败原因（如果返回false）</param>
    /// <returns>令牌是否有效且被消费</returns>
    private bool TryConsumeIssuedJoinToken(string joinToken, string targetPlayerId, string roomId, out string? reason)
    {
        lock (_lock) // 线程安全操作
        {
            if (!_issuedJoinTokens.TryGetValue(joinToken, out IssuedJoinToken? issued)) // 查找令牌
            {
                reason = "Invalid joinToken";
                return false;
            }

            if (DateTime.UtcNow.Ticks > issued.ExpiresAtUtcTicks) // 检查令牌过期
            {
                _issuedJoinTokens.Remove(joinToken); // 清理过期令牌
                reason = "JoinToken expired";
                return false;
            }

            if (!string.Equals(issued.ClientPlayerId, targetPlayerId, StringComparison.Ordinal)) // 验证玩家ID匹配
            {
                reason = "JoinToken target mismatch";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(roomId) && !string.Equals(issued.RoomId, roomId, StringComparison.Ordinal)) // 验证房间ID匹配
            {
                reason = "JoinToken room mismatch";
                return false;
            }

            _issuedJoinTokens.Remove(joinToken); // 消费令牌（移除）
            reason = null;
            return true;
        }
    }

    /// <summary>
    /// 连接状态变更事件处理（断开时清理状态）
    /// </summary>
    /// <param name="connected">是否已连接</param>
    private void OnConnectionStateChanged(bool connected)
    {
        if (connected) // 连接建立，无需清理
        {
            return;
        }

        lock (_lock) // 线程安全清理所有状态
        {
            _lastKnownHostPlayerId = null; // 清空房主ID
            _pendingRequests.Clear(); // 清空待处理请求
            _approvedJoins.Clear(); // 清空已批准加入
            _issuedJoinTokens.Clear(); // 清空已颁发令牌

            foreach (PendingFullSyncRequest pending in _pendingFullSyncRequests.Values) // 通知所有待处理请求
            {
                pending.ErrorMessage = "Disconnected";
                pending.WaitHandle.Set(); // 设置等待句柄以唤醒等待线程
            }

            _pendingFullSyncRequests.Clear(); // 清空待处理请求字典
        }
    }

    /// <summary>
    /// 游戏事件接收处理（分发到对应的处理方法）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件载荷</param>
    private void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root)) // 转换载荷为JSON元素
        {
            try
            {
                string payloadType = payload?.GetType().FullName ?? "<null>";
                string head = payload as string;
                if (head != null)
                {
                    head = head.Replace("\r", " ").Replace("\n", " ");
                    if (head.Length > 200)
                    {
                        head = head.Substring(0, 200);
                    }
                }

                Plugin.Logger?.LogWarning($"[MidGameJoin] Ignore event with invalid payload: type={eventType}, payloadType={payloadType}, head200={head}");
            }
            catch
            {
                // ignored
            }

            return;
        }

        switch (eventType) // 根据事件类型分发处理
        {
            case NetworkMessageTypes.PlayerListUpdate: // 玩家列表更新
                TryUpdateHostFromPlayerListUpdate(root);
                return;

            case NetworkMessageTypes.OnGameStart: // 游戏开始
                _logger.LogDebug("[MidGameJoinManager] OnGameStart received");
                return;

            case NetworkMessageTypes.MidGameJoinRequest: // 中途加入请求
                HandleMidGameJoinRequest(root);
                return;

            case NetworkMessageTypes.MidGameJoinResponse: // 中途加入响应
                HandleMidGameJoinResponse(root);
                return;

            case NetworkMessageTypes.FullStateSyncRequest: // 完整状态同步请求
                HandleFullStateSyncRequest(root);
                return;

            case NetworkMessageTypes.FullStateSyncResponse: // 完整状态同步响应
                HandleFullStateSyncResponse(root);
                return;
        }
    }

    /// <summary>
    /// 尝试从玩家列表更新中提取房主ID。
    /// </summary>
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

    /// <summary>
    /// 处理中途加入请求（房主侧）。
    /// </summary>
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

        // 最小可用实现：自动批准（后续可改为 UI/投票）。
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

    /// <summary>
    /// 处理中途加入响应（客户端侧）。
    /// </summary>
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
        }

        _logger.LogInfo($"[MidGameJoinManager] Join approved: joinToken={joinToken}, host={hostPlayerId}");
    }

    /// <summary>
    /// 处理完整状态同步请求（房主侧）
    /// </summary>
    /// <param name="root">完整状态同步请求事件的JSON根</param>
    private void HandleFullStateSyncRequest(JsonElement root)
    {
        if (!NetworkIdentityTracker.GetSelfIsHost()) // 仅房主处理请求
        {
            return;
        }

        string? requestId = TryGetString(root, "RequestId"); // 提取请求ID
        string? roomId = TryGetString(root, "RoomId"); // 提取房间ID
        string? targetPlayerId = TryGetString(root, "TargetPlayerId"); // 提取目标玩家ID
        long lastKnownEventIndex = TryGetLong(root, "LastKnownEventIndex") ?? 0; // 提取最后已知事件索引
        string? joinToken = TryGetString(root, "JoinToken"); // 提取加入令牌

        if (string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(targetPlayerId) ||
            string.IsNullOrWhiteSpace(joinToken)) // 验证必需字段
        {
            return;
        }

        if (!TryConsumeIssuedJoinToken(joinToken, targetPlayerId, roomId ?? string.Empty, out string? denial)) // 验证并消费一次性令牌
        {
            SendDirectMessage(targetPlayerId, NetworkMessageTypes.FullStateSyncResponse, new // 发送拒绝响应
            {
                RequestId = requestId,
                TargetPlayerId = targetPlayerId,
                ErrorMessage = denial ?? "Denied",
                ServerTimeUtcTicks = DateTime.UtcNow.Ticks
            });
            return;
        }

        ReconnectionManager? reconnection = _serviceProvider.GetService<ReconnectionManager>(); // 获取重连管理器
        FullStateSnapshot snapshot = reconnection?.CreateFullSnapshot() ?? TryCreateFullSnapshot(); // 生成 FullSnapshot
        List<GameEvent> missed = reconnection?.GetMissedEvents(lastKnownEventIndex) ?? []; // 提供 lastKnownEventIndex 之后的事件用于追赶

        SendDirectMessage(targetPlayerId, NetworkMessageTypes.FullStateSyncResponse, new // 发送完整状态响应
        {
            RequestId = requestId,
            TargetPlayerId = targetPlayerId,
            FullSnapshot = snapshot,
            MissedEvents = missed,
            ServerTimeUtcTicks = DateTime.UtcNow.Ticks
        });
    }

    /// <summary>
    /// 处理完整状态同步响应（客户端侧）
    /// </summary>
    /// <param name="root">完整状态同步响应事件的JSON根</param>
    private void HandleFullStateSyncResponse(JsonElement root)
    {
        string? requestId = TryGetString(root, "RequestId"); // 提取请求ID
        string? targetPlayerId = TryGetString(root, "TargetPlayerId"); // 提取目标玩家ID
        if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(targetPlayerId)) // 验证必需字段
        {
            return;
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
        if (string.IsNullOrWhiteSpace(selfId) || !string.Equals(selfId, targetPlayerId, StringComparison.Ordinal)) // 验证响应目标
        {
            return;
        }

        PendingFullSyncRequest? pending;
        lock (_lock) // 线程安全查找待处理请求
        {
            _pendingFullSyncRequests.TryGetValue(requestId, out pending);
        }

        if (pending == null) // 请求不存在或已超时
        {
            return;
        }

        pending.ErrorMessage = TryGetString(root, "ErrorMessage"); // 设置错误消息

        if (root.TryGetProperty("FullSnapshot", out JsonElement snapElem) && snapElem.ValueKind == JsonValueKind.Object) // 提取完整快照
        {
            pending.FullSnapshot = TryDeserialize<FullStateSnapshot>(snapElem);
        }

        if (root.TryGetProperty("MissedEvents", out JsonElement eventsElem) && eventsElem.ValueKind == JsonValueKind.Array) // 提取错过的事件
        {
            pending.MissedEvents = TryDeserialize<List<GameEvent>>(eventsElem) ?? [];
        }

        pending.WaitHandle.Set(); // 唤醒等待线程
    }

    /// <summary>
    /// 尝试将对象转换为JsonElement（用于事件载荷处理）
    /// </summary>
    /// <param name="payload">事件载荷对象</param>
    /// <param name="root">输出的JsonElement</param>
    /// <returns>转换是否成功</returns>
    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je) // 已是 JsonElement 直接返回
            {
                root = je;
                return true;
            }

            if (payload is string s) // string 载荷按 JSON 解析
            {
                root = JsonSerializer.Deserialize<JsonElement>(s);
                return true;
            }

            root = JsonCompat.ToJsonElement(payload); // 其他类型：序列化后再解析成 JsonElement
            return true;
        }
        catch
        {
            root = default; // 转换失败返回 default
            return false;
        }
    }

    /// <summary>
    /// 尝试从JsonElement中获取字符串属性
    /// </summary>
    /// <param name="root">JSON根元素</param>
    /// <param name="property">属性名</param>
    /// <returns>字符串值（如果存在），否则为null</returns>
    private static string? TryGetString(JsonElement root, string property)
    {
        if (root.ValueKind == JsonValueKind.Object && // 确保是对象类型
            root.TryGetProperty(property, out JsonElement p) && // 尝试获取属性
            p.ValueKind == JsonValueKind.String) // 确保属性值是字符串类型
        {
            return p.GetString(); // 读取 string 属性值
        }

        return null; // 属性不存在或类型不匹配
    }

    /// <summary>
    /// 尝试从JsonElement中获取布尔属性
    /// </summary>
    /// <param name="root">JSON根元素</param>
    /// <param name="property">属性名</param>
    /// <returns>布尔值（如果存在），否则为null</returns>
    private static bool? TryGetBool(JsonElement root, string property)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(property, out JsonElement p)) // 验证对象类型和属性存在
        {
            return null;
        }

        return p.ValueKind switch // 根据值类型处理
        {
            JsonValueKind.True => true, // 直接true值
            JsonValueKind.False => false, // 直接false值
            JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) ? b : null, // 字符串类型尝试解析
            _ => null // 其他类型返回null
        };
    }

    /// <summary>
    /// 尝试从JsonElement中获取长整型属性
    /// </summary>
    /// <param name="root">JSON根元素</param>
    /// <param name="property">属性名</param>
    /// <returns>长整型值（如果存在），否则为null</returns>
    private static long? TryGetLong(JsonElement root, string property)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(property, out JsonElement p)) // 验证对象类型和属性存在
        {
            return null;
        }

        return p.ValueKind switch // 根据值类型处理
        {
            JsonValueKind.Number when p.TryGetInt64(out long l) => l, // 数字类型直接获取
            JsonValueKind.String when long.TryParse(p.GetString(), out long l) => l, // 字符串类型尝试解析
            _ => null // 其他类型返回null
        };
    }

    /// <summary>
    /// 尝试反序列化JsonElement为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="elem">JSON元素</param>
    /// <returns>反序列化的对象（如果成功），否则为null</returns>
    private static T? TryDeserialize<T>(JsonElement elem) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(elem.GetRawText()); // 使用 raw json 反序列化
        }
        catch
        {
            return null; // 反序列化失败返回 null
        }
    }

    /// <summary>
    /// 判断事件类型是否应该被回放（用于追赶）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>是否应该回放该事件</returns>
    private static bool ShouldReplayEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) // 空类型不回放
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
            string.Equals(eventType, NetworkMessageTypes.HostChanged, StringComparison.Ordinal)) // 系统/握手/管理类消息不回放
        {
            return false;
        }

        return eventType.StartsWith("On", StringComparison.Ordinal) || // 统一的游戏状态事件
               eventType.StartsWith("Mana", StringComparison.Ordinal) || // 法力相关事件
               eventType.StartsWith("Gap", StringComparison.Ordinal) || // 间隙相关事件
               eventType.StartsWith("Battle", StringComparison.Ordinal) || // 战斗相关事件
               string.Equals(eventType, NetworkMessageTypes.EnemySpawned, StringComparison.Ordinal); // 敌人生成事件
    }

    #endregion
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
