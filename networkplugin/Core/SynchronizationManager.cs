using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Events;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core
{
    /// <summary>
    /// 负责协调所有游戏状态同步的核心管理器
    /// 整合所有Patch点和网络通信，基于LiteNetLib网络框架实现
    /// </summary>
    public class SynchronizationManager
    {
        private static SynchronizationManager _instance; // 单例模式的静态实例
        private readonly IServiceProvider _serviceProvider; // 依赖注入服务提供者
        private INetworkClient _networkClient; // 网络客户端接口

        private readonly Queue<GameEvent> _eventQueue = new(); // 网络不可用时的事件队列
        private readonly Dictionary<string, object> _stateCache = []; // 本地状态缓存字典
        private readonly SyncConfiguration _config = new(); // 同步配置对象

        private bool _isNetworkAvailable = false; // 网络连接状态标志
        private DateTime _lastConnectionTime = DateTime.MinValue; // 最后连接时间
        private DateTime _lastSyncTime = DateTime.MinValue; // 最后同步时间

        /// <summary>
        /// 获取全局唯一的同步管理器实例
        /// </summary>
        public static SynchronizationManager Instance
        {
            get
            {
                _instance ??= new SynchronizationManager(); // 使用空合并运算符确保单例
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数，初始化网络客户端
        /// </summary>
        private SynchronizationManager()
        {
            _serviceProvider = ModService.ServiceProvider;
            InitializeNetworkClient();
            Plugin.Logger?.LogInfo("[SyncManager] Synchronization Manager initialized");
        }

        /// <summary>
        /// 从依赖注入容器获取并初始化网络客户端
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
        /// 处理游戏事件的主要入口点
        /// </summary>
        /// <param name="gameEvent">需要处理的游戏事件对象</param>
        public void ProcessGameEvent(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                Plugin.Logger?.LogWarning("[SyncManager] Attempted to process null game event");
                return;
            }

            try
            {
                if (!IsNetworkAvailable())
                {
                    Plugin.Logger?.LogDebug("[SyncManager] Network not available, queuing event");
                    _eventQueue.Enqueue(gameEvent);
                    return;
                }

                if (!ShouldSyncEvent(gameEvent))
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Event {gameEvent.EventType} filtered out by sync rules");
                    return;
                }

                SendEventToNetwork(gameEvent);
                UpdateLocalState(gameEvent);

                Plugin.Logger?.LogDebug($"[SyncManager] Processed event: {gameEvent.EventType} from {gameEvent.SourcePlayerId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error processing game event {gameEvent.EventType}: {ex.Message}");
            }
        }

        /// <summary>
        /// 接收、解析并应用来自网络的事件到本地游戏状态
        /// </summary>
        /// <param name="eventData">来自网络的原始事件数据</param>
        public void ProcessNetworkEvent(object eventData)
        {
            if (eventData == null)
            {
                Plugin.Logger?.LogWarning("[SyncManager] Received null network event data");
                return;
            }

            try
            {
                var eventDict = eventData as Dictionary<string, object>;
                if (eventDict == null || !eventDict.ContainsKey("EventType"))
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Invalid network event data format");
                    return;
                }

                var eventType = eventDict["EventType"].ToString();
                var payload = eventDict.ContainsKey("Payload") ? eventDict["Payload"] : null;
                var timestamp = eventDict.ContainsKey("Timestamp") ? eventDict["Timestamp"] : DateTime.Now.Ticks;

                var gameEvent = CreateGameEventFromNetworkData(eventType, payload);
                if (gameEvent == null)
                {
                    Plugin.Logger?.LogWarning($"[SyncManager] Failed to create game event from network data: {eventType}");
                    return;
                }

                ApplyRemoteEvent(gameEvent);
                Plugin.Logger?.LogDebug($"[SyncManager] Applied remote event: {gameEvent.EventType} from {gameEvent.SourcePlayerId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error processing network event: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送卡牌使用事件，同步卡牌信息、法力消耗和目标选择
        /// </summary>
        /// <param name="cardId">使用的卡牌ID</param>
        /// <param name="cardName">卡牌名称</param>
        /// <param name="cardType">卡牌类型（攻击/技能/能力牌）</param>
        /// <param name="manaCost">法力消耗数组[红,蓝,绿,白]</param>
        /// <param name="targetSelector">目标选择器字符串</param>
        /// <param name="playerState">使用卡牌时的玩家状态快照</param>
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
        /// 发送法力消耗事件，同步法力变化给远程玩家
        /// </summary>
        /// <param name="manaBefore">消耗前的法力值数组[红,蓝,绿,白]</param>
        /// <param name="manaConsumed">消耗的法力值数组[红,蓝,绿,白]</param>
        /// <param name="source">法力消耗的来源（如使用卡牌、技能等）</param>
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
        /// 发送篝火选项事件，同步选择信息给远程玩家
        /// </summary>
        /// <param name="eventType">篝火事件类型（如休息、强化、升级等）</param>
        /// <param name="optionData">选项的具体数据</param>
        /// <param name="playerState">选择时的玩家状态</param>
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
        /// 请求完整状态同步，用于新玩家加入或断线重连
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
        /// 处理连接恢复事件，处理队列事件并请求完整状态同步
        /// </summary>
        public void OnConnectionRestored()
        {
            try
            {
                _isNetworkAvailable = true;
                _lastConnectionTime = DateTime.Now;

                Plugin.Logger?.LogInfo("[SyncManager] Connection restored, processing queued events");

                while (_eventQueue.Count > 0 && IsNetworkAvailable())
                {
                    var gameEvent = _eventQueue.Dequeue();
                    ProcessGameEvent(gameEvent);
                }

                RequestFullSync();

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
        /// 处理连接丢失事件，切换到离线模式并通知其他玩家
        /// </summary>
        public void OnConnectionLost()
        {
            try
            {
                _isNetworkAvailable = false;

                Plugin.Logger?.LogWarning("[SyncManager] Connection lost, switching to offline mode");

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
        /// 获取同步统计信息，用于调试和监控
        /// </summary>
        /// <returns>包含同步统计数据的对象</returns>
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

        #region 私有方法


        /// <summary>
        /// 验证网络客户端的连接状态，必要时尝试重新初始化
        /// </summary>
        /// <returns>如果网络可用返回true，否则返回false</returns>
        private bool IsNetworkAvailable()
        {
            try
            {
                if (_networkClient == null)
                {
                    InitializeNetworkClient();
                }

                _isNetworkAvailable = _networkClient?.IsConnected ?? false;
                return _isNetworkAvailable;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error checking network availability: {ex.Message}");
                _isNetworkAvailable = false;
                return false;
            }
        }

        /// <summary>
        /// 根据事件类型、玩家权限和游戏阶段判断是否需要同步事件
        /// </summary>
        /// <param name="gameEvent">需要判断的游戏事件</param>
        /// <returns>如果事件需要同步返回true，否则返回false</returns>
        private bool ShouldSyncEvent(GameEvent gameEvent)
        {
            //TODO: 添加不同事件是否需要同步
            return true;
        }

        /// <summary>
        /// 将游戏事件转换为网络数据格式并发送
        /// </summary>
        /// <param name="gameEvent">要发送的游戏事件</param>
        private void SendEventToNetwork(GameEvent gameEvent)
        {
            if (!IsNetworkAvailable())
            {
                return;
            }

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

        /// <summary>
        /// 将事件数据存储到本地缓存中，避免重复同步
        /// </summary>
        /// <param name="gameEvent">需要缓存的游戏事件</param>
        private void UpdateLocalState(GameEvent gameEvent)
        {
            try
            {
                var stateKey = $"{gameEvent.EventType}_{gameEvent.SourcePlayerId}";
                _stateCache[stateKey] = gameEvent.Data;
                CleanupOldStates();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error updating local state: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据事件类型分发到相应的具体事件创建方法
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <param name="payload">事件负载数据</param>
        /// <returns>创建的游戏事件对象，失败时返回null</returns>
        private GameEvent CreateGameEventFromNetworkData(string eventType, object payload)
        {
            try
            {
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

        /// <summary>
        /// 从网络数据中解析卡牌信息并创建卡牌事件
        /// </summary>
        /// <param name="payload">包含卡牌信息的网络数据</param>
        /// <returns>创建的卡牌事件对象，失败时返回null</returns>
        private CardPlayEvent CreateCardPlayEvent(object payload)
        {
            try
            {
                var dict = payload as Dictionary<string, object>;
                if (dict == null)
                {
                    return null;
                }

                var cardId = dict.TryGetValue("CardId", out var id) ? id?.ToString() : "";
                var cardName = dict.TryGetValue("CardName", out var name) ? name?.ToString() : "";
                var cardType = dict.TryGetValue("CardType", out var type) ? type?.ToString() : "";

                int[] manaCost = [0, 0, 0, 0];
                string targetSelector = "Nobody";

                return GameEventFactory.CreateCardPlayEvent("remote_player", cardId, cardName, cardType, manaCost, targetSelector);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating card play event: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从网络数据中解析法力信息并创建法力消耗事件
        /// </summary>
        /// <param name="payload">包含法力信息的网络数据</param>
        /// <returns>创建的法力消耗事件对象，失败时返回null</returns>
        private ManaConsumeEvent CreateManaConsumeEvent(object payload)
        {
            try
            {
                var dict = payload as Dictionary<string, object>;
                if (dict == null)
                {
                    return null;
                }

                int[] manaBefore = [0, 0, 0, 0];
                int[] manaConsumed = [0, 0, 0, 0];
                string source = dict.TryGetValue("Source", out var src) ? src?.ToString() : "Unknown";

                return GameEventFactory.CreateManaConsumeEvent("remote_player", manaBefore, manaConsumed, source);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating mana consume event: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从网络数据中解析伤害信息并创建伤害事件
        /// </summary>
        /// <param name="payload">包含伤害信息的网络数据</param>
        /// <returns>创建的伤害事件对象，失败时返回null</returns>
        private DamageEvent CreateDamageEvent(object payload)
        {
            try
            {
                var dict = payload as Dictionary<string, object>;
                if (dict == null)
                {
                    return null;
                }

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

        /// <summary>
        /// 将从网络接收的远程事件应用到本地游戏状态中
        /// </summary>
        /// <param name="gameEvent">需要应用的远程游戏事件</param>
        private void ApplyRemoteEvent(GameEvent gameEvent)
        {
            try
            {
                Plugin.Logger?.LogDebug($"[SyncManager] Applied remote event: {gameEvent.EventType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error applying remote event: {ex.Message}");
            }
        }

        /// <summary>
        /// 底层的网络发送方法，负责实际的事件传输
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <param name="eventData">事件数据</param>
        private void SendGameEvent(string eventType, object eventData)
        {
            try
            {
                if (!IsNetworkAvailable())
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Network not available for {eventType}");
                    return;
                }

                if (_networkClient is INetworkClient liteNetClient)
                {
                    _networkClient.SendGameEvent(eventType, eventData);
                }
                else
                {
                    _networkClient.SendRequest(eventType, eventData);
                }

                _lastSyncTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error sending game event {eventType}: {ex.Message}");
            }
        }

        /// <summary>
        /// 将法力数组转换为包含各颜色法力和总量的结构化对象
        /// </summary>
        /// <param name="manaArray">法力数组[红,蓝,绿,白]</param>
        /// <returns>结构化的法力对象</returns>
        private object ConvertManaArray(int[] manaArray)
        {
            if (manaArray == null || manaArray.Length < 4)
            {
                return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
            }

            return new
            {
                Red = manaArray[0],
                Blue = manaArray[1],
                Green = manaArray[2],
                White = manaArray[3],
                Total = manaArray[0] + manaArray[1] + manaArray[2] + manaArray[3]
            };
        }

        /// <summary>
        /// 移除过期的状态缓存条目，防止内存泄漏和性能问题
        /// </summary>
        private void CleanupOldStates()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow - _config.StateCacheExpiry;
                var keysToRemove = new List<string>();

                foreach (var kvp in _stateCache)
                {
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

        #endregion
    }

    /// <summary>
    /// 通用游戏事件类，用于处理未明确定义的事件类型
    /// </summary>
    /// <remarks>
    /// 构造函数，创建一个通用游戏事件
    /// </remarks>
    /// <param name="eventType">事件类型字符串</param>
    /// <param name="data">事件数据</param>
    public class GenericGameEvent(string eventType, object data) : GameEvent(ParseEventType(eventType), "unknown_player", data)
    {

        /// <summary>
        /// 解析事件类型字符串为枚举值，失败时返回Error类型
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <returns>对应的游戏事件类型枚举</returns>
        private static GameEventType ParseEventType(string eventType)
        {
            return Enum.TryParse<GameEventType>(eventType, out var result) ? result : GameEventType.Error;
        }

        /// <summary>
        /// 重写基类方法，提供通用的事件序列化
        /// </summary>
        /// <returns>网络传输格式的事件数据</returns>
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

        /// <summary>
        /// 对于通用事件，直接返回当前实例
        /// </summary>
        /// <param name="data">网络数据</param>
        /// <returns>恢复的事件实例</returns>
        public override GameEvent FromNetworkData(object data)
        {
            return this;
        }
    }

    /// <summary>
    /// 包含控制同步行为的各种配置选项和参数
    /// </summary>
    public class SyncConfiguration
    {
        /// <summary>
        /// 控制卡牌使用、抽取等行为的同步
        /// </summary>
        public bool EnableCardSync { get; set; } = true;

        /// <summary>
        /// 控制法力消耗、恢复等行为的同步
        /// </summary>
        public bool EnableManaSync { get; set; } = true;

        /// <summary>
        /// 控制伤害、状态效果等战斗行为的同步
        /// </summary>
        public bool EnableBattleSync { get; set; } = true;

        /// <summary>
        /// 控制地图探索、节点状态等地图行为的同步
        /// </summary>
        public bool EnableMapSync { get; set; } = true;

        /// <summary>
        /// 网络不可用时，事件队列的最大容量
        /// </summary>
        public int MaxQueueSize { get; set; } = 100;

        /// <summary>
        /// 状态缓存的存活时间，超过此时间的缓存将被清理
        /// </summary>
        public TimeSpan StateCacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
    }
}