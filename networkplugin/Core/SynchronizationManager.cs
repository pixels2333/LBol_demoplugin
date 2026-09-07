using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

public class SynchronizationManager : ISynchronizationManager
{
    #region 依赖注入和服务

    private readonly INetworkClient _networkClient;
    private readonly ManualLogSource _logger;
    private readonly ConfigManager _configManager;

    private readonly NetworkEventBufferManager _eventBufferManager;
    private readonly StateCacheManager _stateCacheManager;
    private readonly NetworkAvailabilityTracker _netAvailTracker;

        private readonly ConcurrentQueue<GameEvent> _eventQueue = new();

        private readonly SyncConfiguration _config;

    #endregion

        public SynchronizationManager(
        INetworkClient networkClient,
        NetworkAvailabilityTracker netAvailTracker,
        ManualLogSource logger,
        ConfigManager configManager)
    {
        _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        _config = _configManager.GetSyncConfiguration();
        _eventBufferManager = new NetworkEventBufferManager();
        _stateCacheManager = new StateCacheManager(_config);
        _netAvailTracker = netAvailTracker ?? throw new ArgumentNullException(nameof(netAvailTracker));
        _logger?.LogInfo("[SyncManager] 同步管理器初始化完成（委托模式）");
    }

    #region 网络可用性检查

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
                if (_eventQueue.Count < _config.MaxQueueSize)
                {
                    _logger?.LogDebug("[SyncManager] 网络不可用，事件加入队列");
                    _eventQueue.Enqueue(gameEvent);
                }
                else
                {
                    _logger?.LogWarning($"[SyncManager] 事件队列已满 (MaxQueueSize={_config.MaxQueueSize})，丢弃事件: {gameEvent.EventType}");
                }
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

        public void OnConnectionRestored()
    {
        try
        {
            _netAvailTracker.SetAvailable();
            _logger?.LogInfo("[SyncManager] 网络连接已恢复，开始处理队列事件");

            while (IsNetworkAvailable() && _eventQueue.TryDequeue(out var gameEvent))
            {
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

        public void OnConnectionLost()
    {
        try
        {
            _netAvailTracker.SetUnavailable();
            _logger?.LogWarning("[SyncManager] 网络连接丢失，切换到离线模式");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[SyncManager] 连接丢失处理异常: {ex.Message}");
        }
    }

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

        public object GetEventBufferStatistics() => _eventBufferManager.GetStatistics();

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
                liteNetClient.SendGameEventData(gameEvent.EventType.ToString(), gameEvent.Data);
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
