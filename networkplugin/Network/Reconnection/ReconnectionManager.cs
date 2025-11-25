using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetworkPlugin.Network.Reconnection;

/// <summary>
/// 断线重连管理器 - 处理玩家断线检测、状态保存、重连恢复
/// 重要性: ⭐⭐⭐ (高优先级体验优化)
/// 依赖: 主机权威系统 + 中继服务器
/// </summary>
public class ReconnectionManager
{
    private readonly ILogger<ReconnectionManager> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 重连配置
    /// </summary>
    private readonly ReconnectionConfig _config;

    /// <summary>
    /// 玩家状态快照字典 - 用于断线后恢复
    /// Key: 玩家ID
    /// Value: 状态快照
    /// </summary>
    private readonly Dictionary<string, PlayerStateSnapshot> _playerSnapshots;

    /// <summary>
    /// 最近游戏事件历史（用于断线追赶）
 /// Key: 时间戳
    /// Value: 游戏事件
    /// </summary>
    private readonly SortedList<long, GameEvent> _eventHistory;

    /// <summary>
    /// 心跳检测定时器
    /// </summary>
    private readonly Timer _heartbeatTimer;

    /// <summary>
    /// 快照保存定时器
    /// </summary>
    private readonly Timer _snapshotTimer;

    /// <summary>
    /// 当前连接状态
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// 正在重连中
    /// </summary>
    public bool IsReconnecting { get; private set; }

    /// <summary>
    /// 实例化断线重连管理器
    /// </summary>
    public ReconnectionManager(ReconnectionConfig config, ILogger<ReconnectionManager> logger, IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _playerSnapshots = new Dictionary<string, PlayerStateSnapshot>();
        _eventHistory = new SortedList<long, GameEvent>();
        IsConnected = false;
        IsReconnecting = false;

        // 初始化定时器
        _heartbeatTimer = new Timer(CheckHeartbeats, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        _snapshotTimer = new Timer(SavePeriodicSnapshot, null, TimeSpan.FromSeconds(config.SnapshotIntervalSeconds), TimeSpan.FromSeconds(config.SnapshotIntervalSeconds));
    }

    /// <summary>
    /// 初始化重连管理器
    /// TODO: 连接到网络客户端和事件系统
    /// </summary>
    public void Initialize()
    {
        // TODO: 订阅网络连接事件（连接、断开、心跳超时）
        // TODO: 订阅游戏状态变更事件（用于生成快照）
        // TODO: 绑定到主机权威系统的事件广播

        _logger.LogInformation("[ReconnectionManager] Initialized");
    }

    /// <summary>
    /// 保存完整状态快照（定期执行）
    /// </summary>
    private void SavePeriodicSnapshot(object? state)
    {
        if (!IsConnected)
            return;

        try
        {
            var networkClient = _serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // 创建完整快照
            var snapshot = CreateFullSnapshot();
            SaveSnapshot(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ReconnectionManager] Error saving periodic snapshot: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建完整游戏状态快照
    /// TODO: 包含所有需要恢复的游戏状态
    /// </summary>
    public FullStateSnapshot CreateFullSnapshot()
    {
        var snapshot = new FullStateSnapshot
        {
            Timestamp = DateTime.Now.Ticks,
            GameState = new GameStateSnapshot(),
            PlayerStates = new List<PlayerStateSnapshot>(),
            EventIndex = _eventHistory.Count > 0 ? _eventHistory.Keys[_eventHistory.Count - 1] : 0,
            BattleState = new BattleStateSnapshot(),
            MapState = new MapStateSnapshot()
        };

        // TODO: 从GameRunController获取当前游戏状态
        // TODO: 从BattleController获取战斗状态（如果在战斗中）
        // TODO: 从所有玩家获取玩家状态
        // TODO: 从GameMap获取地图状态

        _logger.LogDebug($"[ReconnectionManager] Full snapshot created at {snapshot.Timestamp}");
        return snapshot;
    }

    /// <summary>
    /// 保存快照到内存
    /// </summary>
    private void SaveSnapshot(FullStateSnapshot snapshot)
    {
        // 保存快照（限制内存使用）
        lock (_playerSnapshots)
        {
            // TODO: 实现快照存储限制（最多保存N个快照）
            // TODO: 若超过限制，删除最旧的快照
        }
    }

    /// <summary>
    /// 心跳检测
    /// </summary>
    private void CheckHeartbeats(object? state)
    {
        // TODO: 检查所有玩家的最后心跳时间
        // TODO: 如果超时标记为断开连接
        // TODO: 触发断线事件
    }

    /// <summary>
    /// 检测到玩家断线
    /// TODO: 在心跳超时或连接断开时调用
    /// </summary>
    public void OnPlayerDisconnected(string playerId, DisconnectReason reason)
    {
        _logger.LogWarning($"[ReconnectionManager] Player {playerId} disconnected, reason: {reason}");

        // 保存玩家断线前的状态
        SavePlayerSnapshotBeforeDisconnect(playerId);

        // 触发断线事件
        PlayerDisconnected?.Invoke(playerId, reason);

        // 启动重连等待计时器
        Task.Delay(TimeSpan.FromMinutes(_config.MaxReconnectionMinutes))
            .ContinueWith(_ => CheckReconnectionTimeout(playerId));
    }

    /// <summary>
    /// 保存玩家断线前的快照
    /// </summary>
    private void SavePlayerSnapshotBeforeDisconnect(string playerId)
    {
        try
        {
            var snapshot = CreatePlayerStateSnapshot(playerId);
            snapshot.ReconnectToken = GenerateReconnectToken(); // 生成重连令牌
            snapshot.DisconnectTime = DateTime.Now.Ticks;

            lock (_playerSnapshots)
            {
                _playerSnapshots[playerId] = snapshot;
            }

            _logger.LogInformation($"[ReconnectionManager] Saved snapshot for player {playerId} before disconnect");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ReconnectionManager] Error saving player {playerId} snapshot: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建玩家状态快照
    /// TODO: 提取玩家的完整状态
    /// </summary>
    private PlayerStateSnapshot CreatePlayerStateSnapshot(string playerId)
    {
        var snapshot = new PlayerStateSnapshot
        {
            PlayerId = playerId,
            Timestamp = DateTime.Now.Ticks,
            Health = 0, // TODO: 从PlayerUnit获取
            MaxHealth = 0, // TODO: 从PlayerUnit获取
            Block = 0, // TODO: 从PlayerUnit获取
            Shield = 0, // TODO: 从PlayerUnit获取
            ManaGroup = new[] { 0, 0, 0, 0 }, // TODO: 获取当前法力
            Gold = 0, // TODO: 从GameRun获取
            Cards = new List<CardStateSnapshot>(), // TODO: 获取手牌、牌库、弃牌堆
            Exhibits = new List<ExhibitStateSnapshot>(), // TODO: 获取宝物
            Potions = new Dictionary<string, int>(), // TODO: 获取药水
            StatusEffects = new List<StatusEffectStateSnapshot>(), // TODO: 获取状态效果
            GameLocation = new LocationSnapshot(),
            IsInBattle = false, // TODO: 检查是否在战斗
            ReconnectToken = string.Empty
        };

        return snapshot;
    }

    /// <summary>
    /// 玩家请求重连
    /// </summary>
    public ReconnectionResult RequestReconnection(string playerId, string reconnectToken)
    {
        if (!IsReconnecting)
        {
            IsReconnecting = true;
        }

        try
        {
            // 检查重连令牌
            if (!_playerSnapshots.TryGetValue(playerId, out var snapshot))
            {
                return ReconnectionResult.Failed("No saved state for player");
            }

            if (snapshot.ReconnectToken != reconnectToken)
            {
                return ReconnectionResult.Failed("Invalid reconnect token");
            }

            // 检查重连超时
            var timeSinceDisconnect = DateTime.Now.Ticks - snapshot.DisconnectTime;
            if (timeSinceDisconnect > TimeSpan.FromMinutes(_config.MaxReconnectionMinutes).Ticks)
            {
                // 清除快照
                RemovePlayerSnapshot(playerId);
                return ReconnectionResult.Failed("Reconnection timeout");
            }

            // 恢复玩家状态
            _logger.LogInformation($"[ReconnectionManager] Reconnection approved for player {playerId}");

            // 发送快照给玩家
            SendReconnectionSnapshot(playerId, snapshot);

            // 广播给其他玩家
            NotifyPlayerReconnected(playerId);

            return ReconnectionResult.Success(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ReconnectionManager] Error in reconnection request: {ex.Message}");
            return ReconnectionResult.Failed($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送重连快照给玩家（快速同步）
    /// </summary>
    private void SendReconnectionSnapshot(string playerId, PlayerStateSnapshot snapshot)
    {
        // TODO: 通过NetworkClient发送完整状态给重连的玩家
        // 1. 发送完整游戏状态
        // 2. 发送最近的事件历史（用于追赶）
        // 3. 发送房间信息和其他玩家状态

        _logger.LogInformation($"[ReconnectionManager] Sent reconnection snapshot to player {playerId}");
    }

    /// <summary>
    /// 通知其他玩家该玩家已重连
    /// </summary>
    private void NotifyPlayerReconnected(string playerId)
    {
        // TODO: 通知其他玩家playerId已重连
        PlayerReconnected?.Invoke(playerId);
    }

    /// <summary>
    /// 检查重连超时（如果玩家在规定时间内未重连）
    /// </summary>
    private void CheckReconnectionTimeout(string playerId)
    {
        if (_playerSnapshots.ContainsKey(playerId))
        {
            RemovePlayerSnapshot(playerId);
            ReconnectionTimeout?.Invoke(playerId);
            _logger.LogInformation($"[ReconnectionManager] Reconnection timeout for player {playerId}");
        }
    }

    /// <summary>
    /// 生成重连令牌（安全随机字符串）
    /// </summary>
    private string GenerateReconnectToken()
    {
        // TODO: 使用更安全的随机令牌生成
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 移除玩家快照
    /// </summary>
    private void RemovePlayerSnapshot(string playerId)
    {
        lock (_playerSnapshots)
        {
            _playerSnapshots.Remove(playerId);
        }
    }

    /// <summary>
    /// 保存历史事件（用于断线追赶）
    /// </summary>
    public void RecordGameEvent(GameEvent gameEvent)
    {
        lock (_eventHistory)
        {
            _eventHistory[gameEvent.Timestamp] = gameEvent;

            // 限制历史记录大小
            if (_eventHistory.Count > _config.MaxHistoryEvents)
            {
                var removeCount = _eventHistory.Count - _config.MaxHistoryEvents;
                for (int i = 0; i < removeCount; i++)
                {
                    _eventHistory.RemoveAt(0);
                }
            }
        }
    }

    /// <summary>
    /// 获取玩家断线期间的历史事件（用于追赶）
    /// </summary>
    public List<GameEvent> GetMissedEvents(long lastKnownEventIndex)
    {
        lock (_eventHistory)
        {
            return _eventHistory
                .Where(e => e.Key > lastKnownEventIndex)
                .Select(e => e.Value)
                .ToList();
        }
    }

    /// <summary>
    /// 获取服务器状态
    /// </summary>
    public ReconnectionManagerStats GetStats()
    {
        lock (_playerSnapshots)
        lock (_eventHistory)
        {
            return new ReconnectionManagerStats
            {
                ActiveSnapshots = _playerSnapshots.Count,
                TotalEvents = _eventHistory.Count,
                IsConnected = IsConnected,
                IsReconnecting = IsReconnecting,
                MaxReconnectionMinutes = _config.MaxReconnectionMinutes,
                SnapshotIntervalSeconds = _config.SnapshotIntervalSeconds
            };
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        _heartbeatTimer?.Dispose();
        _snapshotTimer?.Dispose();
    }

    /// <summary>
    /// 事件
    /// </summary>
    public event Action<string, DisconnectReason>? PlayerDisconnected;
    public event Action<string>? PlayerReconnected;
    public event Action<string>? ReconnectionTimeout;
}

/// <summary>
/// 断连原因
/// </summary>
public enum DisconnectReason
{
    Timeout,
    Manual,
    NetworkError,
    Kicked,
    ServerShutdown
}

/// <summary>
/// 重连结果
/// </summary>
public class ReconnectionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PlayerStateSnapshot? Snapshot { get; set; }

    public static ReconnectionResult Success(PlayerStateSnapshot snapshot)
    {
        return new ReconnectionResult
        {
            Success = true,
            Snapshot = snapshot
        };
    }

    public static ReconnectionResult Failed(string errorMessage)
    {
        return new ReconnectionResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// 重连配置
/// </summary>
public class ReconnectionConfig
{
    public int MaxReconnectionMinutes { get; set; } = 5;
    public int SnapshotIntervalSeconds { get; set; } = 30;
    public int HeartbeatTimeoutSeconds { get; set; } = 30;
    public int MaxHistoryEvents { get; set; } = 1000;
}

/// <summary>
/// 统计信息
/// </summary>
public class ReconnectionManagerStats
{
    public int ActiveSnapshots { get; set; }
    public int TotalEvents { get; set; }
    public bool IsConnected { get; set; }
    public bool IsReconnecting { get; set; }
    public int MaxReconnectionMinutes { get; set; }
    public int SnapshotIntervalSeconds { get; set; }
}
