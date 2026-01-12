using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.MidGameJoin.Result;
using NetworkPlugin.Network.Room;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 中途加入管理器 - 允许玩家在开始后加入游戏
/// 依赖: 断线重连系统
/// </summary>
public class MidGameJoinManager
{
    private readonly ILogger<MidGameJoinManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MidGameJoinConfig _config;

    /// <summary>
    /// 待处理的中途加入请求
    /// </summary>
    private readonly List<GameJoinRequest> _pendingRequests;

    /// <summary>
    /// 已批准但尚未加入的玩家
    /// </summary>
    private readonly Dictionary<string, ApprovedJoin> _approvedJoins;

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

    public MidGameJoinManager(MidGameJoinConfig config, ILogger<MidGameJoinManager> logger, IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _pendingRequests = [];
        _approvedJoins = [];
        _fastSyncService = new FastSyncService(_logger);
        AIController = new AIPlayerController(logger);
    }

    /// <summary>
    /// 初始化中途加入管理器
    /// TODO: 订阅房间和游戏开始事件
    /// </summary>
    public void Initialize()
    {
        _logger.LogInformation("[MidGameJoinManager] Initialized");
        // TODO: 订阅GameStarted事件
        // TODO: 订阅PlayerJoined事件
    }

    /// <summary>
    /// 请求中途加入游戏
    /// TODO: 实现加入验证逻辑,待重做
    /// </summary>
    public JoinRequestResult RequestJoin(string roomId, string playerName)
    {
        try
        {
            lock (_lock)
            {
                // 检查是否允许中途加入
                if (!_config.AllowMidGameJoin)
                {
                    return JoinRequestResult.Denied("Mid-game joining is disabled");
                }

                // 检查房间是否存在
                // TODO: 从RelayServer获取房间信息
                RoomStatus? roomStatus = GetRoomInfo(roomId);
                if (roomStatus == null)
                {
                    return JoinRequestResult.Denied("Room not found");
                }

                // 检查房间是否已满
                if (roomStatus.PlayerCount >= roomStatus.MaxPlayers)
                {
                    return JoinRequestResult.Denied("Room is full");
                }

                // 检查房间是否已开始游戏
                if (!roomStatus.IsInGame)
                {
                    // 如果游戏未开始，直接加入
                    return JoinRequestResult.Approved(new PlayerBootstrappedState());
                }

                // 如果房间已在游戏中，进入请求队列
                GameJoinRequest request = new()
                {
                    RequestId = GenerateRequestId(),
                    RoomId = roomId,
                    PlayerName = playerName,
                    RequestTime = DateTime.Now.Ticks,
                    Status = JoinRequestStatus.Pending
                };

                _pendingRequests.Add(request);

                // 通知房间主持玩家有中途加入请求
                NotifyHostOfJoinRequest(roomStatus.HostPlayerId, request);

                return JoinRequestResult.Pending(request.RequestId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error processing join request: {ex.Message}");
            return JoinRequestResult.Denied($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 批准中途加入请求（由房主调用）
    /// TODO: 实现批准逻辑
    /// </summary>
    public ApproveJoinResult ApproveJoin(string requestId, string approvedByPlayerId)
    {
        lock (_lock)
        {
            var request = _pendingRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
            {
                return ApproveJoinResult.Failed("Request not found");
            }

            // 检查批准权限（必须是房主）
            var roomInfo = GetRoomInfo(request.RoomId);
            if (roomInfo?.HostPlayerId != approvedByPlayerId)
            {
                return ApproveJoinResult.Failed("Only host can approve join requests");
            }

            // 生成加入令牌
            string joinToken = GenerateJoinToken();

            // 创建批准加入信息
            ApprovedJoin approvedJoin = new()
            {
                RequestId = requestId,
                RoomId = request.RoomId,
                PlayerName = request.PlayerName,
                JoinToken = joinToken,
                ApprovedAt = DateTime.Now.Ticks,
                ExpiresAt = DateTime.Now.AddMinutes(_config.JoinRequestTimeoutMinutes).Ticks,
                BootstrappedState = new PlayerBootstrappedState()
            };

            _approvedJoins[joinToken] = approvedJoin;

            // 从请求列表中移除
            _pendingRequests.Remove(request);

            _logger.LogInformation($"[MidGameJoinManager] Join request approved for {request.PlayerName}");

            return ApproveJoinResult.Success(joinToken, approvedJoin.BootstrappedState);
        }
    }

    /// <summary>
    /// 执行中途加入（玩家调用）
    /// TODO: 实现完整加入流程
    /// </summary>
    public JoinExecutionResult ExecuteJoin(string joinToken)
    {
        lock (_lock)
        {
            if (!_approvedJoins.TryGetValue(joinToken, out var approvedJoin))
            {
                return JoinExecutionResult.Failed("Invalid or expired join token");
            }

            // 检查令牌是否过期
            if (DateTime.Now.Ticks > approvedJoin.ExpiresAt)
            {
                _approvedJoins.Remove(joinToken);
                return JoinExecutionResult.Failed("Join token expired");
            }

            // 生成新玩家ID
            string playerId = GeneratePlayerId();

            try
            {
                // 1. 快速同步（从快照恢复基础状态）
                _fastSyncService.SyncPlayerState(playerId, approvedJoin.BootstrappedState);

                // 2. 完全同步（从主机获取最新状态）
                BaseResult fullState = RequestFullStateSync(approvedJoin.RoomId, playerId);
                if (!fullState.IsSuccess)
                {
                    return JoinExecutionResult.Failed("Failed to sync full state: " + fullState.ErrorMessage);
                }

                // 3. 加入房间
                var joinResult = JoinRoom(approvedJoin.RoomId, playerId);
                if (!joinResult.IsSuccess)
                {
                    return JoinExecutionResult.Failed("Failed to join room: " + joinResult.ErrorMessage);
                }

                // 4. 通知房间其他玩家
                NotifyPlayersOfNewPlayer(approvedJoin.RoomId, playerId, approvedJoin.PlayerName);

                // 5. 应用追赶机制（回放断线期间的事件）
                var catchUpResult = ApplyCatchUpEvents(playerId, approvedJoin.BootstrappedState.LastEventIndex);
                if (!catchUpResult.IsSuccess)
                {
                    _logger.LogWarning($"[MidGameJoinManager] Failed to apply catch-up events for player {playerId}");
                }

                // 清理令牌
                _approvedJoins.Remove(joinToken);

                _logger.LogInformation($"[MidGameJoinManager] Player {approvedJoin.PlayerName} successfully joined game at {approvedJoin.BootstrappedState.GameProgress}% progress");

                return JoinExecutionResult.Success(playerId, approvedJoin.BootstrappedState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[MidGameJoinManager] Error executing join: {ex.Message}");
                return JoinExecutionResult.Failed($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 计算游戏进度（百分比）
    /// TODO: 实现进度计算逻辑
    /// </summary>
    private int CalculateGameProgress(object roomState)
    {
        // TODO: 根据楼层、击败的Boss、地图探索等计算进度
        return 50; // 默认值
    }

    /// <summary>
    /// 生成初始卡牌
    /// TODO: 根据游戏进度生成合适数量和稀有度的卡牌
    /// </summary>
    private List<string> GenerateStartingCards(int progress)
    {
        List<string> cards =
        [
            // 基础卡牌
            "Strike",
            "Defend"
        ];

        // 根据进度添加额外卡牌
        if (progress > 20)
        {
            cards.Add("UpgradedStrike");
        }

        if (progress > 40)
        {
            cards.Add("AdvancedCard");
        }

        return cards;
    }

    /// <summary>
    /// 生成初始宝物
    /// TODO: 根据进度生成合适的宝物
    /// </summary>
    private List<string> GenerateStartingExhibits(int progress)
    {
        List<string> exhibits = [];

        if (progress > 10)
        {
            exhibits.Add("BasicRelic");
        }

        if (progress > 30)
        {
            exhibits.Add("CommonRelic");
        }

        return exhibits;
    }

    /// <summary>
    /// 生成初始药水
    /// TODO: 根据进度生成合适的药水
    /// </summary>
    private Dictionary<string, int> GenerateStartingPotions(int progress)
    {
        Dictionary<string, int> potions = [];

        if (progress > 25)
        {
            potions.Add("HealingPotion", 1);
        }

        return potions;
    }

    /// <summary>
    /// 应用追赶事件（回放其他玩家在断线期间的事件）
    /// TODO: 实现事件回放
    /// </summary>
    private CatchUpResult ApplyCatchUpEvents(string playerId, long lastEventIndex)
    {
        try
        {
            var missedEvents = GetMissedEvents(lastEventIndex);

            foreach (var gameEvent in missedEvents)
            {
                // 应用每个事件
                bool applied = ReplayEvent(playerId, gameEvent);
                if (!applied)
                {
                    _logger.LogWarning($"[MidGameJoinManager] Failed to apply event: {gameEvent.EventType}");
                }
            }

            return CatchUpResult.Success(missedEvents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MidGameJoinManager] Error applying catch-up events: {ex.Message}");
            return CatchUpResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// 开始AI托管（代管掉线玩家）
    /// </summary>
    public void StartAIControl(string playerId)
    {
        AIController.StartControlling(playerId);
        _logger.LogInformation($"[MidGameJoinManager] AI started controlling player {playerId}");
    }

    /// <summary>
    /// 停止AI托管（玩家重新连接后）
    /// </summary>
    public void StopAIControl(string playerId)
    {
        AIController.StopControlling(playerId);
        _logger.LogInformation($"[MidGameJoinManager] AI stopped controlling player {playerId}");
    }

    // TODO: Helper methods

    private string GenerateRequestId() => Guid.NewGuid().ToString("N");
    private string GeneratePlayerId() => Guid.NewGuid().ToString("N");
    private string GenerateJoinToken() => Guid.NewGuid().ToString("N");

    // Stub methods - TODO: Implement
    private RoomStatus? GetRoomInfo(string roomId) => null;
    private void NotifyHostOfJoinRequest(string hostPlayerId, GameJoinRequest request) { }
    private BaseResult RequestFullStateSync(string roomId, string playerId) => new() { IsSuccess = true };
    private BaseResult JoinRoom(string roomId, string playerId) => new() { IsSuccess = true };
    private void NotifyPlayersOfNewPlayer(string roomId, string playerId, string playerName) { }
    private RoomStatus GetRoomGameState(string roomId) => new();
    private int CalculateAppropriateLevel(int progress) => 1;
    private int CalculateAppropriateHealth(int progress) => 80;
    private int CalculateAppropriateGold(int progress) => 100;
    private List<GameEvent> GetMissedEvents(long lastEventIndex) => [];
    private bool ReplayEvent(string playerId, GameEvent gameEvent) => true;
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
