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

/// <summary>
/// 中途加入管理器。让玩家在游戏开始后加入。
/// 流程：加入者向房主发请求 → 房主批准 → 加入者获取完整快照 → MapCatchUpOrchestrator 应用地图状态。
/// 依赖 ReconnectionManager 生成快照和记录错过的事件。
/// </summary>
public sealed class MidGameJoinManager
{
    #region 内部字段（类自己的数据）

    /// <summary>写日志用的，方便排查问题</summary>
    private readonly ManualLogSource _logger;
    /// <summary>网络客户端，发消息收消息全靠它</summary>
    private readonly INetworkClient _client;
    /// <summary>断线重连管理器，用来生成快照、获取漏掉的事件</summary>
    private readonly ReconnectionManager _reconnectionManager;
    /// <summary>地图追赶执行器，把房主的地图快照逐步应用到本地</summary>
    private readonly MapCatchUpOrchestrator _mapCatchUp;
    /// <summary>中途加入的配置项（比如是否允许中途加入、超时多久等）</summary>
    private readonly MidGameJoinConfig _config;
    /// <summary>NetworkClient 的具体实现，在注入本地事件时需要用到</summary>
    private NetworkClient? _concreteClient;
    /// <summary>是否已经初始化过了（用原子操作防重复注册）</summary>
    private int _initialized;
    /// <summary>最近一次从玩家列表里拿到的房主 ID</summary>
    private string? _lastKnownHostPlayerId;

    #endregion

    #region 内部嵌套类型

    /// <summary>
    /// 房主颁发的一次性加入令牌。用于身份验证，用后即销毁。
    /// </summary>
    private sealed class IssuedJoinToken
    {
        /// <summary>令牌字符串（GUID）</summary>
        public string JoinToken { get; set; } = string.Empty; // 一次性令牌，用后即销毁
        /// <summary>持有该令牌的玩家 ID</summary>
        public string ClientPlayerId { get; set; } = string.Empty; // 令牌只能被该玩家使用
        /// <summary>对应的房间 ID</summary>
        public string RoomId { get; set; } = string.Empty; // 限制令牌仅在该房间生效
        /// <summary>过期时间（UTC 刻度），过期后令牌不可用</summary>
        public long ExpiresAtUtcTicks { get; set; } // 避免令牌被无限期持有
    }

    /// <summary>
    /// 客户端等待房主返回 FullStateSyncResponse 时的占位对象。
    /// 发送 FullStateSyncRequest 后通过 WaitHandle 阻塞等待。
    /// </summary>
    private sealed class PendingFullSyncRequest : IDisposable
    {
        /// <summary>阻塞等待句柄，收到响应时 Set() 唤醒等待线程</summary>
        public ManualResetEventSlim WaitHandle { get; } = new(false); // 未收到响应前为阻塞状态
        /// <summary>房主返回的完整快照</summary>
        public FullStateSnapshot? FullSnapshot { get; set; } // 由 HandleFullStateSyncResponse 写入
        /// <summary>加入期间错过的事件列表</summary>
        public List<GameEvent> MissedEvents { get; set; } = []; // 用于回放追赶
        /// <summary>错误消息，非空时表示同步失败</summary>
        public string? ErrorMessage { get; set; } // 连接断开或令牌无效时设置
        /// <summary>释放 WaitHandle 资源</summary>
        public void Dispose()
        {
            WaitHandle.Dispose(); // 防止资源泄漏
        }
    }

    #endregion

    #region 数据集合

    /// <summary>待处理的加入请求队列（房主视角）</summary>
    private readonly List<GameJoinRequest> _pendingRequests;
    /// <summary>已批准但尚未执行的加入信息（加入者视角）</summary>
    private readonly Dictionary<string, ApprovedJoin> _approvedJoins;
    /// <summary>房主已颁发但尚未消耗的一次性令牌</summary>
    private readonly Dictionary<string, IssuedJoinToken> _issuedJoinTokens = [];
    /// <summary>客户端等待 FullStateSyncResponse 的请求（requestId → PendingFullSyncRequest）</summary>
    private readonly Dictionary<string, PendingFullSyncRequest> _pendingFullSyncRequests = [];

    /// <summary>requestId → joinToken 映射，方便 UI 层通过 requestId 查找令牌</summary>
    private readonly Dictionary<string, string> _approvedByRequestId = [];

    /// <summary>线程锁，保护所有共享数据集合</summary>
    private readonly object _lock = new();

    /// <summary>用于将引导状态（血量、等级、金币）同步到本地玩家</summary>
    private readonly FastSyncService _fastSyncService;

    #endregion

    #region 构造函数

    /// <summary>
    /// 创建中途加入管理器。注入外部依赖。
    /// </summary>
    public MidGameJoinManager(
        MidGameJoinConfig config, // 中途加入配置项
        ManualLogSource logger, // 日志记录器
        INetworkClient networkClient, // 网络客户端
        ReconnectionManager reconnectionManager, // 断线重连管理器
        MapCatchUpOrchestrator mapCatchUp) // 地图追赶执行器
    {
        _config = config ?? new MidGameJoinConfig(); // 配置未传入时使用默认值
        _logger = logger ?? Plugin.Logger; // 日志未传入时使用插件全局日志
        _client = networkClient ?? throw new ArgumentNullException(nameof(networkClient)); // 网络客户端为必需依赖
        _reconnectionManager = reconnectionManager ?? throw new ArgumentNullException(nameof(reconnectionManager)); // 重连管理器为必需依赖
        _mapCatchUp = mapCatchUp ?? throw new ArgumentNullException(nameof(mapCatchUp)); // 地图追赶器为必需依赖
        _pendingRequests = []; // 初始化空请求队列
        _approvedJoins = []; // 初始化空批准列表
        _fastSyncService = new FastSyncService(_logger); // 创建快速同步服务
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 初始化管理器，注册网络事件回调。
    /// 通过原子操作确保只初始化一次，多线程安全。
    /// </summary>
    public void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1) // 已初始化过，跳过
        {
            _logger.LogDebug("[MidGameJoinManager] Initialize skipped (already initialized)"); // 记录跳过日志
            return; // 避免重复注册
        }

        try
        {
            _concreteClient = _client as NetworkClient; // 保留具体类型引用，用于事件注入

            if (_client == null) // 网络客户端不可用
            {
                _logger.LogWarning("[MidGameJoinManager] Initialize skipped: INetworkClient not available"); // 记录警告日志
                Interlocked.Exchange(ref _initialized, 0); // 重置标记，允许下次重试
                return; // 退出初始化流程
            }

            NetworkIdentityTracker.EnsureSubscribed(_client); // 确保网络身份跟踪器已订阅

            _client.OnGameEventReceived += OnGameEventReceived; // 注册游戏事件回调
            _client.OnConnectionStateChanged += OnConnectionStateChanged; // 注册连接状态回调

            _logger.LogInfo("[MidGameJoinManager] Initialized"); // 记录初始化完成日志
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Initialize failed: {ex.Message}"); // 记录异常日志
            Interlocked.Exchange(ref _initialized, 0); // 异常时重置标记，允许下次重试
        }
    }

    /// <summary>
    /// 请求中途加入游戏。检查配置、连接、自身 ID 和房主 ID 后，向房主发送 DirectMessage。
    /// </summary>
    /// <param name="roomId">房间 ID</param>
    /// <param name="playerName">玩家名称</param>
    /// <returns>Pending（等待批准）或 Denied（拒绝）</returns>
    public JoinRequestResult RequestJoin(string roomId, string playerName)      
    {
        try
        {
            if (!_config.AllowMidGameJoin) // 配置禁用则直接拒绝
            {
                return JoinRequestResult.Denied("已禁用中途加入");
            }

            if (string.IsNullOrWhiteSpace(roomId)) // roomId 为空则拒绝
            {
                return JoinRequestResult.Denied("缺少 roomId");
            }

            if (string.IsNullOrWhiteSpace(playerName)) // playerName 为空则拒绝
            {
                return JoinRequestResult.Denied("缺少 playerName");
            }

            INetworkClient? client = _client; // 获取网络客户端
            if (client?.IsConnected != true) // 未连接则拒绝
            {
                return JoinRequestResult.Denied("未连接到服务器"); // 连接不可用，无法请求加入
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
            if (string.IsNullOrWhiteSpace(selfId)) // 未获取到自身 ID（未收到 Welcome 消息）
            {
                return JoinRequestResult.Denied("缺少自身 playerId（请等待 Welcome/PlayerListUpdate）"); // 身份未就绪，无法请求
            }

            string? hostId; // 临时变量：房主玩家 ID
            lock (_lock) // 线程安全读取上次记录的房主ID
            {
                hostId = _lastKnownHostPlayerId; // 从缓存读取房主 ID
            }

            if (string.IsNullOrWhiteSpace(hostId)) // 未获取到房主则拒绝
            {
                return JoinRequestResult.Denied("未找到房主（请先加入房间并等待 PlayerListUpdate）"); // 缺少房主信息，无法发起请求
            }

            // 如果房主就是自己，说明已经是房主了，不需要中途加入。
            if (string.Equals(hostId, selfId, StringComparison.Ordinal)) // 房主 ID 与自身 ID 一致
            {
                return JoinRequestResult.Denied("你已是房主"); // 房主不能请求加入自己的房间
            }

            string requestId = GenerateRequestId(); // 生成唯一请求ID
            _logger.LogInfo($"[MidGameJoinManager] RequestJoin => host={hostId}, room={roomId}, requestId={requestId}"); // 记录请求日志

            SendDirectMessage(hostId, NetworkMessageTypes.MidGameJoinRequest, new // 通过 DirectMessage 向房主发起加入请求
            {
                RequestId = requestId, // 请求唯一标识
                RoomId = roomId, // 目标房间 ID
                PlayerName = playerName, // 请求者玩家名
                ClientPlayerId = selfId, // 请求者玩家 ID
                ClientTimeUtcTicks = DateTime.UtcNow.Ticks // 请求发起时间
            });

            return JoinRequestResult.Pending(requestId); // 返回待处理状态
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error processing join request: {ex.Message}"); // 记录加入请求异常
            return JoinRequestResult.Denied($"Error: {ex.Message}"); // 异常时返回拒绝结果
        }
    }

    /// <summary>
    /// 房主批准中途加入请求。
    /// 检查身份后清理过期请求，生成一次性令牌和引导状态（血量/等级/金币/卡牌等），通过 DirectMessage 回复请求方。
    /// </summary>
    /// <param name="requestId">请求 ID</param>
    /// <param name="approvedByPlayerId">批准者玩家 ID（必须是房主本人）</param>
    /// <returns>批准结果，包含令牌和引导状态</returns>
    public ApproveJoinResult ApproveJoin(string requestId, string approvedByPlayerId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(requestId)) // requestId 为空，无法处理
            {
                return ApproveJoinResult.Failed("Missing requestId");
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
            if (string.IsNullOrWhiteSpace(selfId)) // 未获取到自身 ID
            {
                return ApproveJoinResult.Failed("Missing self playerId");
            }

            if (!string.Equals(selfId, approvedByPlayerId, StringComparison.Ordinal)) // 仅允许本人批准（防冒用）
            {
                return ApproveJoinResult.Failed("approvedByPlayerId mismatch"); // 批准者 ID 不匹配，拒绝
            }

            if (!NetworkIdentityTracker.GetSelfIsHost()) // 非房主不允许批准
            {
                return ApproveJoinResult.Failed("Only host can approve join requests"); // 只有房主有权批准
            }

            CleanupExpired_NoLock(); // 清理过期请求与令牌

            GameJoinRequest? request; // 临时变量：待处理请求
            lock (_lock) // 线程安全查找待处理请求
            {
                request = _pendingRequests.FirstOrDefault(r => string.Equals(r.RequestId, requestId, StringComparison.Ordinal)); // 按 requestId 查找
            }

            if (request == null) // 请求不存在或已过期
            {
                return ApproveJoinResult.Failed("Request not found"); // 找不到对应请求
            }

            string joinToken = GenerateJoinToken(); // 生成一次性加入令牌
            long expiresAtUtcTicks = DateTime.UtcNow.AddMinutes(_config.JoinRequestTimeoutMinutes).Ticks; // 计算过期时间

            FullStateSnapshot snapshot = TryCreateFullSnapshot(); // 创建完整快照用于估算进度
            int progress = CalculateGameProgress(snapshot.GameState); // 计算游戏进度百分比

            PlayerBootstrappedState bootstrapped = new() // 创建玩家引导状态
            {
                PlayerId = request.ClientPlayerId, // 设置加入者 ID
                GameProgress = progress, // 当前游戏进度
                Level = CalculateAppropriateLevel(progress), // 根据进度计算推荐等级
                MaxHealth = CalculateAppropriateHealth(progress), // 根据进度计算推荐最大血量
                Health = CalculateAppropriateHealth(progress), // 当前血量设为最大血量
                Gold = CalculateAppropriateGold(progress), // 根据进度计算推荐金币
                LastEventIndex = snapshot.EventIndex // 快照中的事件索引
            };

            if (_config.EnableCompensation) // 启用补偿时生成起始资源（当前安全默认为空）
            {
                bootstrapped.Cards = GenerateStartingCards(progress); // 生成初始卡牌（当前返回空列表）
                bootstrapped.Exhibits = GenerateStartingExhibits(progress); // 生成初始遗物（当前返回空列表）
                bootstrapped.ToolCards = GenerateStartingToolCards(progress); // 生成初始工具牌（当前返回空字典）
            }

            lock (_lock) // 线程安全存储令牌并移除请求
            {
                _issuedJoinTokens[joinToken] = new IssuedJoinToken // 创建并存储一次性令牌
                {
                    JoinToken = joinToken, // 令牌字符串
                    ClientPlayerId = request.ClientPlayerId, // 被批准的玩家 ID
                    RoomId = request.RoomId, // 目标房间 ID
                    ExpiresAtUtcTicks = expiresAtUtcTicks // 过期时间
                };

                _pendingRequests.Remove(request); // 从待处理列表中移除已处理的请求
            }

            _logger.LogInfo($"[MidGameJoinManager] Join request approved: requestId={requestId}, joinToken={joinToken}, joiner={request.ClientPlayerId}"); // 记录批准日志

            SendDirectMessage(request.ClientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new // 向请求方发送批准响应
            {
                RequestId = requestId, // 原始请求 ID
                Approved = true, // 批准标记
                Reason = (string?)null, // 拒绝原因为空（批准）
                JoinToken = joinToken, // 颁发的一次性令牌
                ExpiresAtUtcTicks = expiresAtUtcTicks, // 令牌过期时间
                HostPlayerId = approvedByPlayerId, // 房主玩家 ID
                RoomId = request.RoomId, // 房间 ID
                PlayerName = request.PlayerName, // 加入者玩家名
                BootstrappedState = bootstrapped // 引导状态
            });

            return ApproveJoinResult.Success(joinToken, bootstrapped); // 返回成功结果
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error approving join: {ex.Message}"); // 记录批准异常
            return ApproveJoinResult.Failed($"Error: {ex.Message}"); // 异常时返回失败结果
        }
    }

    /// <summary>
    /// 加入者执行中途加入。
    /// 步骤：验证令牌 → 应用引导状态 → 请求完整快照 → 快照传给 MapCatchUpOrchestrator → 回放错过事件 → 清理令牌。
    /// </summary>
    /// <param name="joinToken">一次性加入令牌</param>
    /// <returns>加入执行结果</returns>
    public JoinExecutionResult ExecuteJoin(string joinToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinToken)) // joinToken 为空则失败
            {
                return JoinExecutionResult.Failed("缺少 joinToken");
            }

            INetworkClient? client = _client; // 获取网络客户端
            if (client?.IsConnected != true) // 未连接则无法执行加入
            {
                return JoinExecutionResult.Failed("未连接到服务器"); // 连接不可用
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家ID
            if (string.IsNullOrWhiteSpace(selfId)) // 未获取到自身 ID
            {
                return JoinExecutionResult.Failed("缺少自身 playerId"); // 身份未就绪
            }

            ApprovedJoin approvedJoin; // 临时变量：已批准的加入信息
            lock (_lock) // 线程安全读取已批准的加入信息
            {
                if (!_approvedJoins.TryGetValue(joinToken, out approvedJoin!)) // 查找批准的加入请求
                {
                    return JoinExecutionResult.Failed("joinToken 无效（未批准或已被消耗）"); // 令牌不存在
                }

                if (DateTime.UtcNow.Ticks > approvedJoin.ExpiresAt) // 过期则清理并失败
                {
                    _approvedJoins.Remove(joinToken); // 清理过期令牌
                    return JoinExecutionResult.Failed("joinToken 已过期"); // 令牌已失效
                }

                if (!string.Equals(approvedJoin.ClientPlayerId, selfId, StringComparison.Ordinal)) // 验证令牌归属当前玩家
                {
                    return JoinExecutionResult.Failed("joinToken 不属于当前玩家"); // 令牌被其他玩家冒用
                }
            }

            _fastSyncService.SyncPlayerState(selfId, approvedJoin.BootstrappedState); // 应用引导状态到本地玩家

            (FullStateSnapshot? snapshot, List<GameEvent> missedEvents, string? error) = RequestFullStateSync( // 请求完整状态同步
                approvedJoin.RoomId, // 目标房间 ID
                selfId, // 自身玩家 ID
                approvedJoin.BootstrappedState.LastEventIndex, // 最后已知事件索引
                joinToken, // 一次性加入令牌
                approvedJoin.HostPlayerId); // 房主玩家 ID

            if (!string.IsNullOrWhiteSpace(error)) // 同步失败直接返回错误
            {
                return JoinExecutionResult.Failed("完整状态同步失败: " + error);
            }

            if (snapshot != null) // 根据快照刷新进度与事件索引
            {
                approvedJoin.BootstrappedState.GameProgress = CalculateGameProgress(snapshot.GameState); // 从快照计算最新进度
                approvedJoin.BootstrappedState.LastEventIndex = snapshot.EventIndex; // 更新最后事件索引
                _logger.LogInfo($"[MidGameJoinManager] FullSnapshot received: eventIndex={snapshot.EventIndex}, progress={approvedJoin.BootstrappedState.GameProgress}%"); // 记录收到快照日志

                // 将 MapState 暂存给追赶执行器：本地 GameRun 可用后再尽力应用。
                try // 保护 SetPendingFullSnapshot，避免异常打断执行流程
                {
                    _mapCatchUp.SetPendingFullSnapshot(snapshot); // 将快照暂存到地图追赶执行器
                }
                catch // 追赶执行器暂时不可用时忽略，下次主线程驱动会再尝试
                {
                    // TODO: 应记录异常信息
                    // ignored
                }
            }

            CatchUpResult catchUp = ApplyCatchUpEvents(missedEvents); // 回放错过的事件
            if (!catchUp.IsSuccess) // 回放过程有降级
            {
                _logger.LogWarning($"[MidGameJoinManager] Catch-up replay degraded: {catchUp.ErrorMessage}"); // 记录回放降级日志
            }

            lock (_lock) // 线程安全移除已消费的批准记录
            {
                _approvedJoins.Remove(joinToken); // 令牌已使用，从批准列表移除
            }

            _logger.LogInfo($"[MidGameJoinManager] ExecuteJoin completed: playerId={selfId}, applied={catchUp.EventsApplied}"); // 记录执行完成日志
            return JoinExecutionResult.Success(selfId, approvedJoin.BootstrappedState); // 返回成功结果
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error executing join: {ex.Message}"); // 记录执行异常
            return JoinExecutionResult.Failed($"Error: {ex.Message}"); // 返回失败结果
        }
    }

    /// <summary>
    /// 获取最近记录的房主玩家 ID（来自 PlayerListUpdate 事件）。
    /// </summary>
    public string? GetLastKnownHostPlayerId()
    {
        lock (_lock) // 线程安全读取房主 ID 缓存
        {
            return _lastKnownHostPlayerId; // 返回缓存的房主 ID
        }
    }

    /// <summary>
    /// 根据 requestId 查找已批准的加入令牌。
    /// 当 consume=true 时查找后会从映射中移除。
    /// </summary>
    public bool TryGetApprovedJoinTokenByRequestId(string requestId, out string joinToken, bool consume = false)
    {
        joinToken = string.Empty; // 默认输出为空

        if (string.IsNullOrWhiteSpace(requestId)) // 无效的 requestId
        {
            return false; // 无法查找
        }

        lock (_lock) // 线程安全操作
        {
            if (!_approvedByRequestId.TryGetValue(requestId, out joinToken) || string.IsNullOrWhiteSpace(joinToken)) // 查找映射
            {
                joinToken = string.Empty; // 未找到或令牌为空
                return false; // 返回失败
            }

            if (consume) // 需要消费（移除映射）
            {
                _approvedByRequestId.Remove(requestId); // 移除 requestId 到令牌的映射
            }

            return true; // 成功找到令牌
        }
    }

    /// <summary>
    /// 非阻塞式中途重连入口。
    /// 流程：发送加入请求 → 轮询等待批准 → 在后台线程执行 ExecuteJoin。
    /// ExecuteJoin 当前是同步的，最多阻塞 10 秒，所以放到后台线程避免卡 UI。
    /// </summary>
    public void BeginReconnectAndCatchUp(
        string roomId,
        string playerName,
        Action<string>? onStatus,
        Action<JoinExecutionResult>? onCompleted,
        int timeoutSeconds = 20)
    {
        try
        {
            INetworkClient? client = _client; // 获取网络客户端
            if (client?.IsConnected != true) // 未连接
            {
                Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("未连接到服务器"))); // 在主线程回调失败
                return; // 退出
            }

            // 第一步：发送加入请求
            JoinRequestResult req = RequestJoin(roomId, playerName); // 发起加入请求
            if (req == null || string.IsNullOrWhiteSpace(req.RequestId)) // 请求失败
            {
                Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed(req?.ErrorMessage ?? "加入请求失败"))); // 主线程回调失败
                return; // 退出
            }

            string requestId = req.RequestId; // 记录请求 ID
            Plugin.RunOnMainThread(() => onStatus?.Invoke($"已发送加入请求 (requestId={requestId})")); // 主线程更新状态

            // 第二步：在后台线程等待批准并执行加入，避免卡 UI
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    long start = DateTime.UtcNow.Ticks; // 记录开始时间
                    long timeoutTicks = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)).Ticks; // 计算超时刻度

                    string token = string.Empty; // 令牌临时变量
                    while (DateTime.UtcNow.Ticks - start < timeoutTicks) // 循环轮询直到超时
                    {
                        if (TryGetApprovedJoinTokenByRequestId(requestId, out token, consume: true) && !string.IsNullOrWhiteSpace(token)) // 检查是否已批准
                        {
                            break; // 已批准，退出轮询
                        }

                        Thread.Sleep(50); // 每 50 毫秒轮询一次
                    }

                    if (string.IsNullOrWhiteSpace(token)) // 超时后仍未收到批准
                    {
                        Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("加入审批超时"))); // 主线程回调超时
                        return; // 退出
                    }

                    Plugin.RunOnMainThread(() => onStatus?.Invoke("加入已批准，正在同步...")); // 主线程更新状态
                    JoinExecutionResult result = ExecuteJoin(token); // 执行加入（同步阻塞）
                    Plugin.RunOnMainThread(() => onCompleted?.Invoke(result)); // 主线程回调执行结果
                }
                catch (Exception ex)
                {
                    Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("重连失败: " + ex.Message))); // 主线程回调异常
                }
            }); // 后台线程结束
        }
        catch (Exception ex)
        {
            Plugin.RunOnMainThread(() => onCompleted?.Invoke(JoinExecutionResult.Failed("重连失败: " + ex.Message))); // 主线程回调异常
        }
    }

    /// <summary>
    /// 粗略计算游戏进度百分比。不追求精确，用于补偿和日志。
    /// 映射：每个 Act 占 25%，当前 Act 内的楼层占该 Act 的 25%。
    /// </summary>
    /// <param name="gameState">游戏状态快照</param>
    /// <returns>进度百分比（0~100），结束态返回 100</returns>
    private int CalculateGameProgress(GameStateSnapshot gameState)
    {
        if (gameState == null) // 空快照，无进度
        {
            return 0;
        }

        if (gameState.GameEnded) // 游戏已结束
        {
            return 100;
        }

        if (!gameState.GameStarted) // 游戏未开始
        {
            return 0;
        }

        int act = Math.Clamp(gameState.CurrentAct, 1, 4); // 裁剪 Act 到 1~4
        int floor = Math.Clamp(gameState.CurrentFloor, 0, 20); // 裁剪楼层到 0~20

        double actBase = (act - 1) * 25.0; // 当前 Act 的基础进度
        double floorProgress = (floor / 20.0) * 25.0; // 当前 Act 内楼层贡献的进度

        int result = (int)Math.Round(actBase + floorProgress); // 合并取整
        return Math.Clamp(result, 0, 99); // 约束在 0~99，结束态由上层单独返回 100
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
    /// 生成初始遗物。当前安全实现返回空列表，需要时接入白名单校验。
    /// </summary>
    private List<string> GenerateStartingExhibits(int progress)
    {
        return [];
    }

    /// <summary>
    /// 生成初始工具牌。当前安全实现返回空字典，需要时接入白名单校验。
    /// </summary>
    private Dictionary<string, int> GenerateStartingToolCards(int progress)
    {
        return [];
    }

    /// <summary>
    /// 按时间戳顺序回放错过的事件。
    /// 通过 NetworkClient.InjectLocalGameEvent 注入本地事件。
    /// 连续失败超过 3 次则停止回放，降级为仅快照模式。
    /// </summary>
    private CatchUpResult ApplyCatchUpEvents(List<GameEvent> missedEvents)
    {
        if (missedEvents == null || missedEvents.Count == 0) // 无错过事件，无需回放
        {
            return CatchUpResult.Success(0);
        }

        NetworkClient? concrete = _concreteClient ?? (_client as NetworkClient); // 获取 NetworkClient 具体类型用于事件注入
        if (concrete == null) // 不支持本地事件注入，降级为仅快照
        {
            return CatchUpResult.Failed("NetworkClient does not support local event injection");
        }

        int applied = 0; // 成功回放的事件数
        int failed = 0; // 回放失败的事件数

        foreach (GameEvent e in missedEvents.OrderBy(e => e.Timestamp)) // 按时间戳顺序逐个回放
        {
            if (!ShouldReplayEventType(e.EventType)) // 跳过不需要回放的事件类型
            {
                continue;
            }

            try
            {
                concrete.InjectLocalGameEvent(e.EventType, e.Data); // 将事件注入本地，触发同套处理逻辑
                applied++;
            }
            catch (Exception ex)
            {
                failed++; // 回放失败计数加一
                _logger.LogWarning($"[MidGameJoinManager] Replay failed: type={e.EventType}, err={ex.Message}"); // 记录回放失败详情
                if (failed >= 3) // 连续失败超过 3 次，停止回放
                {
                    return CatchUpResult.Failed("Too many replay failures; degraded to snapshot-only"); // 返回降级结果
                }
            }

            if (_config.CatchUpBatchSize > 0 && applied % _config.CatchUpBatchSize == 0) // 按批次输出进度日志
            {
                _logger.LogDebug($"[MidGameJoinManager] Catch-up batch applied: {applied}"); // 记录批次进度
            }
        }

        return CatchUpResult.Success(applied); // 返回成功应用的事件数量
    }

    /// <summary>
    /// 生成请求 ID（GUID 无横线格式）。
    /// </summary>
    private static string GenerateRequestId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// 生成一次性加入令牌（GUID 无横线格式）。
    /// </summary>
    private static string GenerateJoinToken() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// 根据进度计算推荐等级（1~6）。
    /// </summary>
    private int CalculateAppropriateLevel(int progress)
        => Math.Clamp(1 + progress / 20, 1, 6);

    /// <summary>
    /// 根据进度计算推荐生命值（60~120）。
    /// </summary>
    private int CalculateAppropriateHealth(int progress)
        => Math.Clamp(60 + progress / 2, 60, 120);

    /// <summary>
    /// 根据进度计算推荐金币（50~300）。
    /// </summary>
    private int CalculateAppropriateGold(int progress)
        => Math.Clamp(50 + progress * 2, 50, 300);

    /// <summary>
    /// 清理过期的加入请求和令牌。非线程安全，调用方需在锁内调用。
    /// </summary>
    private void CleanupExpired_NoLock()
    {
        long now = DateTime.UtcNow.Ticks; // 当前 UTC 时间

        _pendingRequests.RemoveAll(r => now - r.RequestTime > TimeSpan.FromMinutes(_config.JoinRequestTimeoutMinutes).Ticks); // 移除超时的请求

        foreach ((string key, IssuedJoinToken issued) in _issuedJoinTokens.ToList()) // 遍历令牌列表
        {
            if (now > issued.ExpiresAtUtcTicks) // 已过期
            {
                _issuedJoinTokens.Remove(key); // 移除过期令牌
            }
        }
    }

    /// <summary>
    /// 创建完整状态快照。优先使用断线重连管理器的快照，失败时返回空快照。
    /// </summary>
    private FullStateSnapshot TryCreateFullSnapshot()
    {
        try
        {
            ReconnectionManager? reconnection = _reconnectionManager; // 获取重连管理器
            return reconnection?.CreateFullSnapshot() ?? new FullStateSnapshot // 优先使用重连系统的快照
            {
                Timestamp = DateTime.UtcNow.Ticks, // 当前时间戳
                GameState = new GameStateSnapshot(), // 空游戏状态
                PlayerStates = [], // 空玩家状态列表
                MapState = new MapStateSnapshot(), // 空地图状态
                EventIndex = 0, // 事件索引归零
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
    /// 通过 DirectMessage 向目标玩家发送点对点消息。未连接时丢弃并记警告。
    /// </summary>
    private void SendDirectMessage(string targetPlayerId, string innerType, object innerPayload)
    {
        INetworkClient? client = _client; // 获取网络客户端
        if (client?.IsConnected != true) // 未连接，消息无法发送
        {
            _logger.LogWarning($"[MidGameJoinManager] DirectMessage 已丢弃（未连接）: type={innerType}"); // 记录丢弃日志
            return; // 退出
        }

        client.SendGameEventData("DirectMessage", new // 用 DirectMessage 封装点对点消息
        {
            TargetPlayerId = targetPlayerId, // 目标玩家 ID
            Type = innerType, // 消息类型
            Payload = innerPayload, // 消息负载
        });
    }

    /// <summary>
    /// 向房主请求完整状态同步。发送请求后阻塞最多 10 秒等待响应。
    /// </summary>
    private (FullStateSnapshot? snapshot, List<GameEvent> missedEvents, string? error) RequestFullStateSync(
        string roomId,
        string targetPlayerId,
        long lastKnownEventIndex,
        string joinToken,
        string hostPlayerId)
    {
        if (string.IsNullOrWhiteSpace(hostPlayerId)) // 房主 ID 为空
        {
            return (null, new List<GameEvent>(), "Missing host playerId"); // 无法发送请求
        }

        string requestId = GenerateRequestId(); // 生成请求 ID
        PendingFullSyncRequest pending = new(); // 创建待处理的同步请求占位对象

        lock (_lock) // 线程安全存储
        {
            _pendingFullSyncRequests[requestId] = pending; // 注册到待处理字典
        }

        try
        {
            _logger.LogInfo($"[MidGameJoinManager] FullStateSyncRequest => host={hostPlayerId}, requestId={requestId}, lastIndex={lastKnownEventIndex}"); // 记录请求日志
            SendDirectMessage(hostPlayerId, NetworkMessageTypes.FullStateSyncRequest, new // 发送完整状态同步请求
            {
                RequestId = requestId, // 请求唯一标识
                RoomId = roomId, // 目标房间 ID
                TargetPlayerId = targetPlayerId, // 请求者玩家 ID
                LastKnownEventIndex = lastKnownEventIndex, // 最后已知事件索引
                JoinToken = joinToken // 一次性令牌
            });

            bool signaled = pending.WaitHandle.Wait(TimeSpan.FromSeconds(10)); // 阻塞等待 FullStateSyncResponse（最多 10 秒）
            if (!signaled) // 超时
            {
                return (null, [], "FullStateSyncResponse timeout"); // 返回超时错误
            }

            if (!string.IsNullOrWhiteSpace(pending.ErrorMessage)) // 有错误消息
            {
                return (null, [], pending.ErrorMessage); // 返回错误消息
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
                reason = "Invalid joinToken"; // 令牌不存在
                return false; // 验证失败
            }

            if (DateTime.UtcNow.Ticks > issued.ExpiresAtUtcTicks) // 检查令牌过期
            {
                _issuedJoinTokens.Remove(joinToken); // 清理过期令牌
                reason = "JoinToken expired"; // 令牌已过期
                return false; // 验证失败
            }

            if (!string.Equals(issued.ClientPlayerId, targetPlayerId, StringComparison.Ordinal)) // 验证玩家ID匹配
            {
                reason = "JoinToken target mismatch"; // 玩家 ID 不匹配
                return false; // 验证失败
            }

            if (!string.IsNullOrWhiteSpace(roomId) && !string.Equals(issued.RoomId, roomId, StringComparison.Ordinal)) // 房间 ID 不匹配
            {
                reason = "JoinToken 房间不匹配"; // 房间 ID 不一致
                return false; // 验证失败
            }

            _issuedJoinTokens.Remove(joinToken); // 消费令牌，从中途加入管理器移除
            reason = null; // 验证通过，无错误
            return true; // 验证成功
        }
    }

    /// <summary>
    /// 连接断开时清理所有中途加入相关的状态（请求、批准、令牌、待处理同步）。
    /// </summary>
    private void OnConnectionStateChanged(bool connected)
    {
        if (connected) // 连接建立，不需要清理
        {
            return;
        }

        lock (_lock) // 线程安全清理
        {
            _lastKnownHostPlayerId = null; // 清空房主 ID
            _pendingRequests.Clear(); // 清空待处理请求
            _approvedJoins.Clear(); // 清空已批准加入
            _issuedJoinTokens.Clear(); // 清空已颁发令牌

            foreach (PendingFullSyncRequest pending in _pendingFullSyncRequests.Values) // 遍历待处理同步请求
            {
                pending.ErrorMessage = "连接已断开"; // 设置错误消息
                pending.WaitHandle.Set(); // 唤醒等待线程
            }

            _pendingFullSyncRequests.Clear(); // 清空待处理同步请求字典
        }
    }
    #endregion

    #region 私有方法 — 网络事件处理

    /// <summary>
    /// 游戏事件接收入口。根据事件类型分发给对应的处理方法。
    /// </summary>
    private void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root)) // 无法解析为 JSON，尝试记录头部信息后忽略
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
    /// 从 PlayerListUpdate 事件中提取房主的玩家 ID。
    /// </summary>
    private void TryUpdateHostFromPlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array) // 玩家列表属性不存在或类型不是数组
        {
            return; // 无法解析玩家列表
        }

        foreach (JsonElement p in playersElem.EnumerateArray()) // 遍历玩家列表
        {
            if (p.ValueKind != JsonValueKind.Object) // 元素不是对象类型
            {
                continue; // 跳过无效项
            }

            if (!p.TryGetProperty("IsHost", out JsonElement isHostElem) || isHostElem.ValueKind != JsonValueKind.True) // 不是房主
            {
                continue; // 跳过非房主玩家
            }

            if (!p.TryGetProperty("PlayerId", out JsonElement idElem) || idElem.ValueKind != JsonValueKind.String) // 玩家 ID 属性不存在或不是字符串
            {
                continue; // 无法获取玩家 ID
            }

            string? hostId = idElem.GetString(); // 读取房主玩家 ID
            if (!string.IsNullOrWhiteSpace(hostId)) // 获取到有效 ID
            {
                lock (_lock) // 线程安全写入房主 ID 缓存
                {
                    _lastKnownHostPlayerId = hostId; // 更新缓存的房主 ID
                }
            }

            return; // 找到房主后退出
        }
    }

    /// <summary>
    /// 处理中途加入请求（房主侧）。
    /// 校验必填字段，检查配置是否允许，检查瓶颈房间请求数，自动批准或拒绝。
    /// </summary>
    private void HandleMidGameJoinRequest(JsonElement root)
    {
        if (!NetworkIdentityTracker.GetSelfIsHost()) // 仅房主处理请求
        {
            return; // 非房主直接忽略
        }

        string? requestId = TryGetString(root, "RequestId"); // 从 JSON 提取请求 ID
        string? roomId = TryGetString(root, "RoomId"); // 从 JSON 提取房间 ID
        string? playerName = TryGetString(root, "PlayerName"); // 从 JSON 提取玩家名
        string? clientPlayerId = TryGetString(root, "ClientPlayerId"); // 从 JSON 提取请求者 ID

        if (string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(roomId) ||
            string.IsNullOrWhiteSpace(playerName) ||
            string.IsNullOrWhiteSpace(clientPlayerId)) // 必填字段不全
        {
            _logger.LogWarning("[MidGameJoinManager] Ignore invalid MidGameJoinRequest"); // 记录无效请求警告
            return; // 忽略无效请求
        }

        if (!_config.AllowMidGameJoin) // 配置禁止中途加入
        {
            SendDirectMessage(clientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new // 发送拒绝响应
            {
                RequestId = requestId, // 原始请求 ID
                Approved = false, // 拒绝标记
                Reason = "Mid-game joining is disabled" // 拒绝原因
            });
            return; // 退出处理
        }

        lock (_lock) // 线程安全操作请求队列
        {
            CleanupExpired_NoLock(); // 先清理过期请求

            int activeCount = _pendingRequests.Count(r => string.Equals(r.RoomId, roomId, StringComparison.Ordinal)); // 统计该房间待处理请求数
            if (activeCount >= _config.MaxJoinRequestsPerRoom) // 超过房间最大请求数限制
            {
                SendDirectMessage(clientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new // 发送拒绝响应
                {
                    RequestId = requestId, // 原始请求 ID
                    Approved = false, // 拒绝标记
                    Reason = "Too many pending join requests" // 拒绝原因：请求过多
                });
                return; // 退出处理
            }

            _pendingRequests.Add(new GameJoinRequest // 将请求加入待处理列表
            {
                RequestId = requestId, // 请求 ID
                RoomId = roomId, // 房间 ID
                PlayerName = playerName, // 请求者玩家名
                ClientPlayerId = clientPlayerId, // 请求者玩家 ID
                RequestTime = DateTime.UtcNow.Ticks, // 请求时间
                Status = JoinRequestStatus.Pending // 初始状态为待处理
            });
        }

        // 最小可用实现：自动批准（后续可改为 UI/投票）。
        string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取房主自身 ID
        if (string.IsNullOrWhiteSpace(selfId)) // 身份未就绪
        {
            return; // 无法自动批准
        }

        ApproveJoinResult approved = ApproveJoin(requestId, selfId); // 自动批准请求
        if (string.IsNullOrWhiteSpace(approved.JoinToken)) // 批准失败（返回空令牌）
        {
            SendDirectMessage(clientPlayerId, NetworkMessageTypes.MidGameJoinResponse, new // 发送拒绝响应
            {
                RequestId = requestId, // 原始请求 ID
                Approved = false, // 拒绝标记
                Reason = approved.ErrorMessage ?? "Join request denied" // 使用错误消息或默认原因
            });
        }
    }

    /// <summary>
    /// 处理中途加入响应（客户端侧）。校验字段后将批准信息存入 _approvedJoins 和 _approvedByRequestId。
    /// </summary>
    private void HandleMidGameJoinResponse(JsonElement root)
    {
        string? requestId = TryGetString(root, "RequestId"); // 提取请求 ID
        bool? approved = TryGetBool(root, "Approved"); // 提取批准标记

        if (string.IsNullOrWhiteSpace(requestId) || approved != true) // 请求无 ID 或未被批准
        {
            string? reason = TryGetString(root, "Reason"); // 提取拒绝原因
            _logger.LogWarning($"[MidGameJoinManager] Join denied: requestId={requestId}, reason={reason}"); // 记录拒绝日志
            return; // 退出处理
        }

        string? joinToken = TryGetString(root, "JoinToken"); // 提取一次性加入令牌
        string? hostPlayerId = TryGetString(root, "HostPlayerId"); // 提取房主玩家 ID
        string? roomId = TryGetString(root, "RoomId"); // 提取房间 ID
        long expiresAtUtcTicks = TryGetLong(root, "ExpiresAtUtcTicks") ?? 0; // 提取过期时间

        if (string.IsNullOrWhiteSpace(joinToken) ||
            string.IsNullOrWhiteSpace(hostPlayerId) ||
            string.IsNullOrWhiteSpace(roomId)) // 必填字段缺失
        {
            _logger.LogWarning($"[MidGameJoinManager] Invalid MidGameJoinResponse: requestId={requestId}"); // 记录无效响应警告
            return; // 退出处理
        }

        if (!root.TryGetProperty("BootstrappedState", out JsonElement bsElem) || bsElem.ValueKind != JsonValueKind.Object) // 引导状态字段缺失
        {
            _logger.LogWarning($"[MidGameJoinManager] Missing BootstrappedState: requestId={requestId}"); // 记录缺失警告
            return; // 退出处理
        }

        PlayerBootstrappedState? bootstrapped = TryDeserialize<PlayerBootstrappedState>(bsElem); // 反序列化引导状态
        if (bootstrapped == null) // 反序列化失败
        {
            _logger.LogWarning($"[MidGameJoinManager] Invalid BootstrappedState: requestId={requestId}"); // 记录无效警告
            return; // 退出处理
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家 ID
        if (string.IsNullOrWhiteSpace(selfId)) // 身份未就绪
        {
            return; // 退出处理
        }

        ApprovedJoin approvedJoin = new() // 创建批准加入信息
        {
            RequestId = requestId, // 请求 ID
            RoomId = roomId, // 房间 ID
            PlayerName = TryGetString(root, "PlayerName") ?? string.Empty, // 玩家名
            HostPlayerId = hostPlayerId, // 房主 ID
            ClientPlayerId = selfId, // 自身玩家 ID
            JoinToken = joinToken, // 加入令牌
            ApprovedAt = DateTime.UtcNow.Ticks, // 批准时间
            ExpiresAt = expiresAtUtcTicks > 0 ? expiresAtUtcTicks : DateTime.UtcNow.AddMinutes(_config.JoinRequestTimeoutMinutes).Ticks, // 过期时间
            BootstrappedState = bootstrapped, // 引导状态
        };

        lock (_lock) // 线程安全写入批准信息
        {
            _approvedJoins[joinToken] = approvedJoin; // 按令牌索引存储

            // 同时按 requestId 索引，方便 UI/协程不用持有令牌就能找到批准信息。
            if (!string.IsNullOrWhiteSpace(requestId)) // requestId 有效
            {
                _approvedByRequestId[requestId] = joinToken; // 建立 requestId 到令牌的映射
            }
        }

        _logger.LogInfo($"[MidGameJoinManager] Join approved: joinToken=<已脱敏>, host={hostPlayerId}"); // 记录批准成功日志
    }

    /// <summary>
    /// 处理完整状态同步请求（房主侧）。验证令牌后通过重连管理器生成快照和错过事件列表，发送响应。
    /// </summary>
    private void HandleFullStateSyncRequest(JsonElement root)
    {
        if (!NetworkIdentityTracker.GetSelfIsHost()) // 仅房主能处理
        {
            return; // 非房主忽略
        }

        string? requestId = TryGetString(root, "RequestId"); // 从 JSON 提取请求 ID
        string? roomId = TryGetString(root, "RoomId"); // 从 JSON 提取房间 ID
        string? targetPlayerId = TryGetString(root, "TargetPlayerId"); // 从 JSON 提取目标玩家 ID
        long lastKnownEventIndex = TryGetLong(root, "LastKnownEventIndex") ?? 0; // 从 JSON 提取最后已知事件索引
        string? joinToken = TryGetString(root, "JoinToken"); // 从 JSON 提取加入令牌

        if (string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(targetPlayerId) ||
            string.IsNullOrWhiteSpace(joinToken)) // 必填字段缺失
        {
            return; // 无法处理
        }

        if (!TryConsumeIssuedJoinToken(joinToken, targetPlayerId, roomId ?? string.Empty, out string? denial)) // 令牌无效或已过期
        {
            SendDirectMessage(targetPlayerId, NetworkMessageTypes.FullStateSyncResponse, new // 发送拒绝响应
            {
                RequestId = requestId, // 原始请求 ID
                TargetPlayerId = targetPlayerId, // 目标玩家 ID
                ErrorMessage = denial ?? "Denied", // 错误消息
                ServerTimeUtcTicks = DateTime.UtcNow.Ticks // 服务器时间
            });
            return; // 退出处理
        }

        ReconnectionManager? reconnection = _reconnectionManager; // 获取重连管理器
        FullStateSnapshot snapshot = reconnection?.CreateFullSnapshot() ?? TryCreateFullSnapshot(); // 生成快照
        List<GameEvent> missed = reconnection?.GetMissedEvents(lastKnownEventIndex) ?? []; // 获取最后已知事件索引之后的事件

        SendDirectMessage(targetPlayerId, NetworkMessageTypes.FullStateSyncResponse, new // 发送完整状态响应
        {
            RequestId = requestId, // 原始请求 ID
            TargetPlayerId = targetPlayerId, // 目标玩家 ID
            FullSnapshot = snapshot, // 完整快照
            MissedEvents = missed, // 错过的事件列表
            ServerTimeUtcTicks = DateTime.UtcNow.Ticks // 服务器时间
        });
    }

    /// <summary>
    /// 处理完整状态同步响应（客户端侧）。将快照和错过事件写入 PendingFullSyncRequest 并唤醒等待线程。
    /// </summary>
    private void HandleFullStateSyncResponse(JsonElement root)
    {
        string? requestId = TryGetString(root, "RequestId"); // 从 JSON 提取请求 ID
        string? targetPlayerId = TryGetString(root, "TargetPlayerId"); // 从 JSON 提取目标玩家 ID
        if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(targetPlayerId)) // 必填字段缺失
        {
            return; // 无法处理
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId(); // 获取自身玩家 ID
        if (string.IsNullOrWhiteSpace(selfId) || !string.Equals(selfId, targetPlayerId, StringComparison.Ordinal)) // 不是发给自己的响应
        {
            return; // 忽略其他玩家的响应
        }

        PendingFullSyncRequest? pending; // 临时变量：待处理请求
        lock (_lock) // 线程安全查找待处理请求
        {
            _pendingFullSyncRequests.TryGetValue(requestId, out pending); // 按 requestId 查找
        }

        if (pending == null) // 请求不存在或已超时
        {
            return; // 忽略已超时的响应
        }

        pending.ErrorMessage = TryGetString(root, "ErrorMessage"); // 设置错误消息

        if (root.TryGetProperty("FullSnapshot", out JsonElement snapElem) && snapElem.ValueKind == JsonValueKind.Object) // 提取完整快照
        {
            pending.FullSnapshot = TryDeserialize<FullStateSnapshot>(snapElem); // 反序列化快照
        }

        if (root.TryGetProperty("MissedEvents", out JsonElement eventsElem) && eventsElem.ValueKind == JsonValueKind.Array) // 提取错过的事件
        {
            pending.MissedEvents = TryDeserialize<List<GameEvent>>(eventsElem) ?? []; // 反序列化事件列表
        }

        pending.WaitHandle.Set(); // 唤醒等待线程
    }

    /// <summary>
    /// 将事件负载转换为 JsonElement。支持已有 JsonElement、string 和其他类型。
    /// </summary>
    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je) // 已经是 JsonElement 类型
            {
                root = je; // 直接返回
                return true; // 转换成功
            }

            if (payload is string s) // 字符串类型负载
            {
                root = JsonSerializer.Deserialize<JsonElement>(s); // 按 JSON 解析字符串
                return true; // 转换成功
            }

            root = JsonCompat.ToJsonElement(payload); // 其他类型：序列化后转成 JsonElement
            return true; // 转换成功
        }
        catch
        {
            root = default; // 转换失败返回默认值
            return false; // 返回失败
        }
    }

    /// <summary>
    /// 从 JsonElement 读取字符串属性。属性不存在或类型不匹配时返回 null。
    /// </summary>
    private static string? TryGetString(JsonElement root, string property)
    {
        if (root.ValueKind == JsonValueKind.Object && // 根元素必须是对象
            root.TryGetProperty(property, out JsonElement p) && // 属性存在
            p.ValueKind == JsonValueKind.String) // 属性值是字符串
        {
            return p.GetString(); // 返回字符串值
        }

        return null; // 属性不存在或类型不匹配
    }

    /// <summary>
    /// 从 JsonElement 读取布尔属性。支持 true/false 值和字符串解析。
    /// </summary>
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

    /// <summary>
    /// 从 JsonElement 读取长整型属性。支持数字和字符串解析。
    /// </summary>
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

    /// <summary>
    /// 将 JsonElement 反序列化为指定类型。失败时返回 null。
    /// </summary>
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

    /// <summary>
    /// 判断事件类型是否应被回放。系统/握手/管理类消息（如 FullStateSyncRequest、Welcome、PlayerListUpdate 等）不回放。
    /// 以 On/Mana/Gap 开头的事件以及战斗类事件（伤害/治疗/状态/敌人相关）需要回放。
    /// </summary>
    private static bool ShouldReplayEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))

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

        if (eventType.StartsWith("On", StringComparison.Ordinal) || // 统一的游戏状态事件
            eventType.StartsWith("Mana", StringComparison.Ordinal) || // 法力相关事件
            eventType.StartsWith("Gap", StringComparison.Ordinal)) // 间隙相关事件
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

/// <summary>
/// 中途加入请求状态
/// </summary>
public enum JoinRequestStatus
{
    /// <summary>待处理</summary>
    Pending,
    /// <summary>已批准</summary>
    Approved,
    /// <summary>已拒绝</summary>
    Denied,
    /// <summary>已过期</summary>
    Expired
}
