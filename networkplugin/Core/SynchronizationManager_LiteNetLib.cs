using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Events;
using NetworkPlugin.Network.Messages;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin.Core
{
    /// <summary>
    /// 核心同步管理器 - 负责协调所有游戏状态的同步
    /// 这是联机MOD的大脑，整合了所有Patch点和网络通信
    * 参考杀戮尖塔联机Mod的多人游戏状态管理系统
    /// 基于LiteNetLib网络框架实现
    /// </summary>
    public class SynchronizationManager
    {
        private static SynchronizationManager _instance;
        private readonly IServiceProvider _serviceProvider;
        private INetworkClient _networkClient;

        // 事件队列
        private readonly Queue<GameEvent> _eventQueue = new Queue<GameEvent>();

        // 状态缓存
        private readonly Dictionary<string, object> _stateCache = new Dictionary<string, object>();

        // 同步配置
        private readonly SyncConfiguration _config = new SyncConfiguration();

        // 网络状态
        private bool _isNetworkAvailable = false;
        private DateTime _lastConnectionTime = DateTime.MinValue;
        private DateTime _lastSyncTime = DateTime.MinValue;

        public static SynchronizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SynchronizationManager();
                }
                return _instance;
            }
        }

        private SynchronizationManager()
        {
            _serviceProvider = ModService.ServiceProvider;
            InitializeNetworkClient();

            Plugin.Logger?.LogInfo("[SyncManager] Synchronization Manager initialized with LiteNetLib support");
        }

        /// <summary>
        /// 初始化网络客户端
        /// </summary>
        private void InitializeNetworkClient()
        {
            try
            {
                _networkClient = _serviceProvider?.GetService<INetworkClient>();
                if (_networkClient != null)
                {
                    Plugin.Logger?.LogInfo("[SyncManager] Network client initialized successfully");
                }
                else
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Network client not available - running in offline mode");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error initializing network client: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理游戏事件 - 主要的事件入口点
        /// </summary>
        /// <param name="gameEvent">游戏事件</param>
        public void ProcessGameEvent(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                Plugin.Logger?.LogWarning("[SyncManager] Attempted to process null game event");
                return;
            }

            try
            {
                // 验证网络连接状态
                if (!IsNetworkAvailable())
                {
                    Plugin.Logger?.LogDebug("[SyncManager] Network not available, queuing event");
                    _eventQueue.Enqueue(gameEvent);
                    return;
                }

                // 验证同步权限
                if (!ShouldSyncEvent(gameEvent))
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Event {gameEvent.EventType} filtered out by sync rules");
                    return;
                }

                // 发送事件到网络
                SendEventToNetwork(gameEvent);

                // 更新本地状态缓存
                UpdateLocalState(gameEvent);

                Plugin.Logger?.LogDebug($"[SyncManager] Processed event: {gameEvent.EventType} from {gameEvent.SourcePlayerId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error processing game event {gameEvent.EventType}: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理来自网络的事件
        /// </summary>
        /// <param name="eventData">网络事件数据</param>
        public void ProcessNetworkEvent(object eventData)
        {
            if (eventData == null)
            {
                Plugin.Logger?.LogWarning("[SyncManager] Received null network event data");
                return;
            }

            try
            {
                // 解析网络事件数据
                var eventDict = eventData as Dictionary<string, object>;
                if (eventDict == null || !eventDict.ContainsKey("EventType"))
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Invalid network event data format");
                    return;
                }

                var eventType = eventDict["EventType"].ToString();
                var payload = eventDict.ContainsKey("Payload") ? eventDict["Payload"] : null;
                var timestamp = eventDict.ContainsKey("Timestamp") ? eventDict["Timestamp"] : DateTime.Now.Ticks;

                // 创建游戏事件
                var gameEvent = CreateGameEventFromNetworkData(eventType, payload);
                if (gameEvent == null)
                {
                    Plugin.Logger?.LogWarning($"[SyncManager] Failed to create game event from network data: {eventType}");
                    return;
                }

                // 应用远程事件到本地游戏状态
                ApplyRemoteEvent(gameEvent);

                Plugin.Logger?.LogDebug($"[SyncManager] Applied remote event: {gameEvent.EventType} from {gameEvent.SourcePlayerId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error processing network event: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送卡牌使用事件
        /// </summary>
        public void SendCardPlayEvent(string cardId, string cardName, string cardType,
            int[] manaCost, string targetSelector, object playerState)
        {
            var playerId = GameStateUtils.GetCurrentPlayerId();
            var eventData = new
            {
                Timestamp = DateTime.Now.Ticks,
                CardId = cardId,
                CardName = cardName,
                CardType = cardType,
                ManaCost = ConvertManaArray(manaCost),
                TargetSelector = targetSelector,
                PlayerState = playerState
            };

            SendGameEvent(NetworkMessageTypes.OnCardPlayStart, eventData);
        }

        /// <summary>
        /// 发送法力消耗事件
        /// </summary>
        public void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source)
        {
            var playerId = GameStateUtils.GetCurrentPlayerId();
            var eventData = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = playerId,
                ManaBefore = ConvertManaArray(manaBefore),
                ManaConsumed = ConvertManaArray(manaConsumed),
                Source = source
            };

            SendGameEvent(NetworkMessageTypes.ManaConsumeStarted, eventData);
        }

        /// <summary>
        /// 发送篝火选项事件
        /// </summary>
        public void SendGapStationEvent(string eventType, object optionData, object playerState)
        {
            var playerId = GameStateUtils.GetCurrentPlayerId();
            var eventData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = eventType,
                PlayerId = playerId,
                OptionData = optionData,
                PlayerState = playerState
            };

            SendGameEvent(eventType, eventData);
        }

        /// <summary>
        /// 请求完整状态同步
        /// </summary>
        public void RequestFullSync()
        {
            try
            {
                if (!IsNetworkAvailable())
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Cannot request full sync - network not available");
                    return;
                }

                var syncRequest = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    RequestType = "FullSync",
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    RequestReason = "ManualRequest"
                };

                SendGameEvent(NetworkMessageTypes.FullStateSyncRequest, syncRequest);
                _lastSyncTime = DateTime.Now;

                Plugin.Logger?.LogInfo("[SyncManager] Full state sync requested");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error requesting full sync: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理连接恢复事件
        /// </summary>
        public void OnConnectionRestored()
        {
            try
            {
                _isNetworkAvailable = true;
                _lastConnectionTime = DateTime.Now;

                Plugin.Logger?.LogInfo("[SyncManager] Connection restored, processing queued events");

                // 处理队列中的事件
                while (_eventQueue.Count > 0 && IsNetworkAvailable())
                {
                    var gameEvent = _eventQueue.Dequeue();
                    ProcessGameEvent(gameEvent);
                }

                // 请求完整状态同步
                RequestFullSync();

                // 发送连接建立通知
                SendGameEvent(NetworkMessageTypes.OnConnectionEstablished, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = GameStateUtils.GetCurrentPlayerId()
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error handling connection restoration: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理连接丢失事件
        /// </summary>
        public void OnConnectionLost()
        {
            try
            {
                _isNetworkAvailable = false;

                Plugin.Logger?.LogWarning("[SyncManager] Connection lost, switching to offline mode");

                // 发送连接丢失通知
                SendGameEvent(NetworkMessageTypes.OnConnectionLost, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    QueuedEvents = _eventQueue.Count
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error handling connection loss: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取同步统计信息
        /// </summary>
        public object GetSyncStatistics()
        {
            return new
            {
                QueuedEvents = _eventQueue.Count,
                CachedStates = _stateCache.Count,
                IsNetworkAvailable = _isNetworkAvailable,
                LastSyncTime = _lastSyncTime,
                LastConnectionTime = _lastConnectionTime,
                Configuration = _config
            };
        }

        // 私有方法

        /// <summary>
        /// 检查网络是否可用
        /// </summary>
        private bool IsNetworkAvailable()
        {
            try
            {
                if (_networkClient == null)
                {
                    // 尝试重新初始化网络客户端
                    InitializeNetworkClient();
                }

                _isNetworkAvailable = _networkClient?.IsConnected == true;
                return _isNetworkAvailable;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error checking network availability: {ex.Message}");
                _isNetworkAvailable = false;
                return false;
            }
        }

        private bool ShouldSyncEvent(GameEvent gameEvent)
        {
            // TODO: 实现事件过滤逻辑
            // 检查事件类型是否在同步范围内
            // 检查玩家是否有权限同步此事件
            // 检查是否在正确的游戏阶段

            return true;
        }

        private void SendEventToNetwork(GameEvent gameEvent)
        {
            if (!IsNetworkAvailable())
                return;

            try
            {
                var networkData = gameEvent.ToNetworkData();
                SendGameEvent(gameEvent.EventType.ToString(), networkData);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error sending event to network: {ex.Message}");
            }
        }

        private void UpdateLocalState(GameEvent gameEvent)
        {
            try
            {
                var stateKey = $"{gameEvent.EventType}_{gameEvent.SourcePlayerId}";
                _stateCache[stateKey] = gameEvent.Data;

                // 清理旧的状态缓存
                CleanupOldStates();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error updating local state: {ex.Message}");
            }
        }

        private GameEvent CreateGameEventFromNetworkData(string eventType, object payload)
        {
            try
            {
                // 根据事件类型创建相应的GameEvent实例
                return eventType switch
                {
                    NetworkMessageTypes.OnCardPlayStart => CreateCardPlayEvent(payload),
                    NetworkMessageTypes.ManaConsumeStarted => CreateManaConsumeEvent(payload),
                    NetworkMessageTypes.OnDamageDealt => CreateDamageEvent(payload),
                    _ => new GenericGameEvent(eventType, payload)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating game event from network data: {ex.Message}");
                return null;
            }
        }

        private CardPlayEvent CreateCardPlayEvent(object payload)
        {
            try
            {
                var dict = payload as Dictionary<string, object>;
                if (dict == null) return null;

                var cardId = dict.TryGetValue("CardId", out var id) ? id?.ToString() : "";
                var cardName = dict.TryGetValue("CardName", out var name) ? name?.ToString() : "";
                var cardType = dict.TryGetValue("CardType", out var type) ? type?.ToString() : "";

                // TODO: 解析manaCost和targetSelector
                int[] manaCost = new int[4] { 0, 0, 0, 0 };
                string targetSelector = "Nobody";

                return GameEventFactory.CreateCardPlayEvent("remote_player", cardId, cardName, cardType, manaCost, targetSelector);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating card play event: {ex.Message}");
                return null;
            }
        }

        private ManaConsumeEvent CreateManaConsumeEvent(object payload)
        {
            try
            {
                var dict = payload as Dictionary<string, object>;
                if (dict == null) return null;

                // TODO: 解析法力数组
                int[] manaBefore = new int[4] { 0, 0, 0, 0 };
                int[] manaConsumed = new int[4] { 0, 0, 0, 0 };
                string source = dict.TryGetValue("Source", out var src) ? src?.ToString() : "Unknown";

                return GameEventFactory.CreateManaConsumeEvent("remote_player", manaBefore, manaConsumed, source);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating mana consume event: {ex.Message}");
                return null;
            }
        }

        private DamageEvent CreateDamageEvent(object payload)
        {
            try
            {
                var dict = payload as Dictionary<string, object>;
                if (dict == null) return null;

                string sourceId = dict.TryGetValue("SourceId", out var src) ? src?.ToString() : "";
                string targetId = dict.TryGetValue("TargetId", out var tgt) ? tgt?.ToString() : "";
                int damageAmount = dict.TryGetValue("DamageAmount", out var dmg) && dmg != null ? Convert.ToInt32(dmg) : 0;
                string damageType = dict.TryGetValue("DamageType", out var dmgType) ? dmgType?.ToString() : "Unknown";

                return GameEventFactory.CreateDamageEvent("remote_player", sourceId, targetId, damageAmount, damageType);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating damage event: {ex.Message}");
                return null;
            }
        }

        private void ApplyRemoteEvent(GameEvent gameEvent)
        {
            try
            {
                // TODO: 实现远程事件应用逻辑
                // 根据事件类型修改本地游戏状态
                // 可能需要调用游戏的API或使用反射

                Plugin.Logger?.LogDebug($"[SyncManager] Applied remote event: {gameEvent.EventType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error applying remote event: {ex.Message}");
            }
        }

        private void SendGameEvent(string eventType, object eventData)
        {
            try
            {
                if (!IsNetworkAvailable())
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Network not available for {eventType}");
                    return;
                }

                // 使用LiteNetLib网络客户端发送
                if (_networkClient is NetworkClient liteNetClient)
                {
                    liteNetClient.SendGameEvent(eventType, eventData);
                }
                else
                {
                    // 备用方案：使用通用SendRequest方法
                    _networkClient.SendRequest(eventType, eventData);
                }

                _lastSyncTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error sending game event {eventType}: {ex.Message}");
            }
        }

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

        private void CleanupOldStates()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow - _config.StateCacheExpiry;
                var keysToRemove = new List<string>();

                foreach (var kvp in _stateCache)
                {
                    // 简单的清理逻辑，可以根据需要改进
                    if (kvp.Key.Contains("Old") || kvp.Key.Contains("Temp"))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _stateCache.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error cleaning up old states: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 通用游戏事件类，用于处理未明确定义的事件类型
    /// </summary>
    public class GenericGameEvent : GameEvent
    {
        public GenericGameEvent(string eventType, object data)
            : base(ParseEventType(eventType), "unknown_player", data)
        {
        }

        private static GameEventType ParseEventType(string eventType)
        {
            return Enum.TryParse<GameEventType>(eventType, out var result) ? result : GameEventType.Error;
        }

        public override object ToNetworkData()
        {
            return new
            {
                EventType = EventType.ToString(),
                Timestamp = Timestamp.Ticks,
                SourcePlayerId,
                Data
            };
        }

        public override GameEvent FromNetworkData(object data)
        {
            return this;
        }
    }

    /// <summary>
    /// 同步配置类
    /// </summary>
    public class SyncConfiguration
    {
        public bool EnableCardSync { get; set; } = true;
        public bool EnableManaSync { get; set; } = true;
        public bool EnableBattleSync { get; set; } = true;
        public bool EnableMapSync { get; set; } = true;
        public int MaxQueueSize { get; set; } = 100;
        public TimeSpan StateCacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
    }
}