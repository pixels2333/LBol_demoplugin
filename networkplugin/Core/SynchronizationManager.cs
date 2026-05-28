using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

/// <summary>
/// 同步管理器类
/// LBoL联机MOD的核心组件，负责协调所有游戏状态的同步功能
/// 整合所有Harmony补丁点和网络通信，基于LiteNetLib网络框架实现多人游戏状态同步
///
/// 重构说明：已将缓冲区管理、状态缓存、网络可用性跟踪分别委托给
/// <see cref="NetworkEventBufferManager"/>、<see cref="StateCacheManager"/>、<see cref="NetworkAvailabilityTracker"/>。
/// </summary>
public class SynchronizationManager : ISynchronizationManager
{
    #region 依赖注入和服务

    private readonly INetworkClient _networkClient;
    private readonly ManualLogSource _logger;
    private readonly ConfigManager _configManager;

    private readonly NetworkEventBufferManager _eventBufferManager;
    private readonly StateCacheManager _stateCacheManager;
    private readonly NetworkAvailabilityTracker _netAvailTracker;

    /// <summary>
    /// 网络不可用时的事件队列
    /// </summary>
    private readonly Queue<GameEvent> _eventQueue = new();

    /// <summary>
    /// 同步配置对象
    /// </summary>
    private readonly SyncConfiguration _config = new();

    #endregion

    /// <summary>
    /// 初始化同步管理器
    /// </summary>
    /// <param name="networkClient">网络客户端实例</param>
    /// <param name="netAvailTracker">网络可用性跟踪器</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="configManager">配置管理器</param>
    public SynchronizationManager(
        INetworkClient networkClient,
        NetworkAvailabilityTracker netAvailTracker,
        ManualLogSource logger,
        ConfigManager configManager)
    {
        _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        _eventBufferManager = new NetworkEventBufferManager();
        _stateCacheManager = new StateCacheManager(_config);
        _netAvailTracker = netAvailTracker ?? throw new ArgumentNullException(nameof(netAvailTracker));
        _logger?.LogInfo("[SyncManager] 同步管理器初始化完成（委托模式）");
    }

    #region 网络可用性检查

    /// <summary>
    /// 检查网络客户端是否可用
    /// </summary>
    /// <returns>网络可用时返回 true，否则 false</returns>
    private bool IsNetworkAvailable()
    {
        try
        {
            _netAvailTracker.SetAvailable();
            bool available = _networkClient?.IsConnected ?? false;
            if (!available) _netAvailTracker.SetUnavailable();
            return available;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 网络可用性检查异常: {ex.Message}");
            _netAvailTracker.SetUnavailable();
            return false;
        }
    }

    #endregion

    #region ISynchronizationManager 实现

    /// <summary>
    /// 处理游戏事件的主要入口点，将本地游戏事件同步到网络
    /// </summary>
    /// <param name="gameEvent">需要处理的游戏事件对象</param>
    public void SyncGameEventToNetwork(GameEvent gameEvent)
    {
        if (gameEvent == null)
        {
            _logger?.LogWarning("[SyncManager] 尝试处理空的游戏事件");
            return;
        }

        try
        {
            if (!IsNetworkAvailable())
            {
                _logger?.LogDebug("[SyncManager] 网络不可用，事件加入队列");
                _eventQueue.Enqueue(gameEvent);
                return;
            }

            if (!ShouldSyncEvent(gameEvent))
            {
                _logger?.LogDebug($"[SyncManager] 事件 {gameEvent.EventType} 被同步规则过滤");
                return;
            }

            SendGameEvent(gameEvent);

            _logger?.LogDebug($"[SyncManager] 事件处理完成: {gameEvent.EventType} 来自 {gameEvent.UserName}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 游戏事件处理异常 - 事件类型: {gameEvent.EventType}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 接收并处理来自网络的远程事件，将网络传输的事件数据应用到本地游戏状态
    /// </summary>
    /// <param name="eventData">来自网络的原始事件数据</param>
    public void ProcessEventFromNetwork(object eventData)
    {
        if (eventData == null)
        {
            _logger?.LogWarning("[SyncManager] 接收到空的网络事件数据");
            return;
        }

        try
        {
            _eventBufferManager.EnqueueEvent(eventData);
            _eventBufferManager.ProcessBufferedEvents(gameEvent =>
            {
                _stateCacheManager.ApplyRemoteEvent(gameEvent);
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 网络事件处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送卡牌使用事件到网络
    /// </summary>
    /// <param name="cardId">卡牌唯一标识符</param>
    /// <param name="cardName">卡牌显示名称</param>
    /// <param name="cardType">卡牌类型</param>
    /// <param name="manaCost">法力消耗数组</param>
    /// <param name="targetSelector">目标选择器</param>
    /// <param name="playerState">玩家状态</param>
    public void SendCardPlayEvent(string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object playerState)
    {
        string playerId = GameStateUtils.GetCurrentPlayerId();
        var eventData = new Dictionary<string, object>
        {
            ["CardId"] = cardId,
            ["CardName"] = cardName,
            ["CardType"] = cardType,
            ["ManaCost"] = manaCost,
            ["TargetSelector"] = targetSelector,
            ["PlayerState"] = playerState ?? ""
        };
        SendGameEvent(new GameEvent("CardPlayed", playerId, eventData));
    }

    /// <summary>
    /// 发送法力消耗事件到网络
    /// </summary>
    /// <param name="manaBefore">消耗前的法力值数组</param>
    /// <param name="manaConsumed">消耗的法力值数组</param>
    /// <param name="source">消耗来源</param>
    public void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source)
    {
        string playerId = GameStateUtils.GetCurrentPlayerId();
        var eventData = new Dictionary<string, object>
        {
            ["ManaBefore"] = ConvertManaArray(manaBefore),
            ["ManaConsumed"] = ConvertManaArray(manaConsumed),
            ["Source"] = source
        };
        SendGameEvent(new GameEvent("ManaConsumeStarted", playerId, eventData));
    }

    /// <summary>
    /// 发送 GapStation 选项事件到网络
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="optionData">选项数据</param>
    /// <param name="playerState">玩家状态</param>
    public void SendGapStationEvent(string eventType, object optionData, object playerState)
    {
        string playerId = GameStateUtils.GetCurrentPlayerId();
        var eventData = new Dictionary<string, object>
        {
            ["OptionData"] = optionData ?? "",
            ["PlayerState"] = playerState ?? ""
        };
        SendGameEvent(new GameEvent(eventType, playerId, eventData));
    }

    /// <summary>
    /// 请求完整状态同步，用于新玩家加入游戏或断线重连时获取完整的游戏状态
    /// </summary>
    public void RequestFullSync()
    {
        try
        {
            if (!IsNetworkAvailable())
            {
                _logger?.LogWarning("[SyncManager] 无法请求完整状态同步 - 网络连接不可用");
                return;
            }

            if (!_netAvailTracker.CanRequestFullSync())
            {
                _logger?.LogDebug("[SyncManager] FullStateSyncRequest 节流：距离上次请求过近，已跳过");
                return;
            }

            var syncRequestData = new Dictionary<string, object>
            {
                ["RequestType"] = "FullSync",
                ["RequestReason"] = "ManualRequest",
                ["RequestId"] = DateTime.UtcNow.Ticks
            };
            string playerId = GameStateUtils.GetCurrentPlayerId();
            var syncEvent = new GameEvent(NetworkMessageTypes.FullStateSyncRequest.ToString(), playerId, syncRequestData);
            SendGameEvent(syncEvent);

            _logger?.LogInfo($"[SyncManager] 发起完整状态同步请求: playerId={playerId}, requestId={DateTime.UtcNow.Ticks}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 完整状态同步请求异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理网络连接恢复事件
    /// </summary>
    public void OnConnectionRestored()
    {
        try
        {
            _netAvailTracker.SetAvailable();
            _logger?.LogInfo("[SyncManager] 网络连接已恢复，开始处理队列事件");

            while (_eventQueue.Count > 0 && IsNetworkAvailable())
            {
                var gameEvent = _eventQueue.Dequeue();
                SyncGameEventToNetwork(gameEvent);
            }

            RequestFullSync();

            var connectionData = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["PlayerId"] = GameStateUtils.GetCurrentPlayerId()
            };
            SendGameEvent(new GameEvent(NetworkMessageTypes.OnConnectionEstablished.ToString(), GameStateUtils.GetCurrentPlayerId(), connectionData));
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 连接恢复处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理网络连接丢失事件
    /// </summary>
    public void OnConnectionLost()
    {
        try
        {
            _netAvailTracker.SetUnavailable();
            _logger?.LogWarning("[SyncManager] 网络连接丢失，切换到离线模式");

            string playerId = GameStateUtils.GetCurrentPlayerId();
            var eventData = new Dictionary<string, object> { ["QueuedEvents"] = _eventQueue.Count };
            SendGameEvent(new GameEvent("ConnectionLost", playerId, eventData));
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 连接丢失处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取同步统计信息
    /// </summary>
    /// <returns>同步统计数据对象</returns>
    public object GetSyncStatistics()
    {
        return new
        {
            QueuedEvents = _eventQueue.Count,
            MaxQueueSize = _config.MaxQueueSize,
            EventBuffer = _eventBufferManager.GetStatistics(),
            CachedStates = _stateCacheManager.CachedStateCount,
            CacheExpiry = _config.StateCacheExpiry,
            IsNetworkAvailable = _netAvailTracker.IsAvailable,
            LastSyncTime = _netAvailTracker.LastSyncTime,
            LastConnectionTime = _netAvailTracker.LastConnectionTime,
            Configuration = _config
        };
    }

    /// <summary>
    /// 获取远程事件缓冲区统计信息
    /// </summary>
    /// <returns>缓冲区统计数据对象</returns>
    public object GetEventBufferStatistics() => _eventBufferManager.GetStatistics();

    /// <summary>
    /// 底层的网络发送方法，负责实际的事件数据传输和网络通信
    /// </summary>
    /// <param name="gameEvent">要发送的游戏事件</param>
    public void SendGameEvent(GameEvent gameEvent)
    {
        try
        {
            if (!IsNetworkAvailable())
            {
                _logger?.LogDebug($"[SyncManager] 网络不可用，跳过事件发送: {gameEvent.EventType}");
                return;
            }

            if (_networkClient is NetworkClient liteNetClient)
                _networkClient.SendGameEventData(gameEvent.EventType.ToString(), gameEvent.Data);
            else
                _networkClient.SendRequest(gameEvent.EventType.ToString(), gameEvent.Data);

            _netAvailTracker.MarkSyncCompleted();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 游戏事件发送异常 - 类型: {gameEvent.EventType}, 错误: {ex.Message}");
        }
    }

    #endregion

    #region 事件过滤

    /// <summary>
    /// 根据配置判断事件是否需要同步到网络
    /// </summary>
    /// <param name="gameEvent">待判断的游戏事件</param>
    /// <returns>需要同步则返回 true</returns>
    private bool ShouldSyncEvent(GameEvent gameEvent)
    {
        if (gameEvent == null || string.IsNullOrWhiteSpace(gameEvent.EventType))
            return false;

        try
        {
            if (_configManager == null) return true;

            string t = gameEvent.EventType;

            bool isCard =
                string.Equals(t, NetworkMessageTypes.OnCardPlayStart, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnCardPlayComplete, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnCardDraw, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnCardDiscard, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnCardExile, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnCardUpgrade, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnCardRemove, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnRemoteCardUse, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnRemoteCardResolved, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.HandSyncRequest, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.HandSyncResponse, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.DeckSyncRequest, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.DeckSyncResponse, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.DiscardSyncRequest, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.DiscardSyncResponse, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.DeckOperation, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.CardStateChanged, StringComparison.Ordinal);

            bool isMana =
                string.Equals(t, NetworkMessageTypes.ManaConsumeStarted, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.ManaConsumeCompleted, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.ManaRegain, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.TurnManaCalculated, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.MaxManaChange, StringComparison.Ordinal);

            bool isBattle =
                string.Equals(t, NetworkMessageTypes.OnBattleStart, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnBattleEnd, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnTurnStart, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnTurnEnd, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnDamageDealt, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnDamageReceived, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnBlockGained, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnShieldGained, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnHealingReceived, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnStatusEffectApplied, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnStatusEffectRemoved, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnMoodEffectLoopStarted, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnMoodEffectLoopEnded, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnMoodEffectStateSync, StringComparison.Ordinal);

            bool isMap =
                string.Equals(t, NetworkMessageTypes.OnShopEnter, StringComparison.Ordinal) ||
                string.Equals(t, NetworkMessageTypes.OnShopExit, StringComparison.Ordinal);

            if (isCard && _configManager.EnableCardSync != null)
                return _configManager.EnableCardSync.Value;
            if (isMana && _configManager.EnableManaSync != null)
                return _configManager.EnableManaSync.Value;
            if (isBattle && _configManager.EnableBattleSync != null)
                return _configManager.EnableBattleSync.Value;
            if (isMap && _configManager.EnableMapSync != null)
                return _configManager.EnableMapSync.Value;

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"[SyncManager] Event filter failed, allow by default: {ex.Message}");
            return true;
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 将法力值数组转换为包含各色法力和总量的匿名对象
    /// </summary>
    /// <param name="manaArray">法力值数组 [红, 蓝, 绿, 白]</param>
    /// <returns>包含 Red、Blue、Green、White、Total 的匿名对象</returns>
    private object ConvertManaArray(int[] manaArray)
    {
        if (manaArray == null || manaArray.Length < 4)
            return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };

        return new
        {
            Red = manaArray[0],
            Blue = manaArray[1],
            Green = manaArray[2],
            White = manaArray[3],
            Total = manaArray[0] + manaArray[1] + manaArray[2] + manaArray[3]
        };
    }

    #endregion
}
