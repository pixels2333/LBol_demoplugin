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
        // 单例模式的静态实例
        private static SynchronizationManager _instance;
        // 依赖注入服务提供者
        private readonly IServiceProvider _serviceProvider;
        // 网络客户端接口
        private INetworkClient _networkClient;

        // 网络不可用时的事件队列
        private readonly Queue<GameEvent> _eventQueue = new();
        // 本地状态缓存字典
        private readonly Dictionary<string, object> _stateCache = [];
        // 同步配置对象
        private readonly SyncConfiguration _config = new();

        // 网络连接状态标志
        private bool _isNetworkAvailable = false;
        // 最后连接时间
        private DateTime _lastConnectionTime = DateTime.MinValue;
        // 最后同步时间
        private DateTime _lastSyncTime = DateTime.MinValue;

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
            _serviceProvider = ModService.ServiceProvider; // 获取MOD服务提供者
            InitializeNetworkClient(); // 初始化网络客户端
            Plugin.Logger?.LogInfo("[SyncManager] Synchronization Manager initialized"); // 记录初始化日志

        }

        /// <summary>
        /// 从依赖注入容器获取并初始化网络客户端
        /// </summary>
        private void InitializeNetworkClient()
        {
            try
            {

                _networkClient = _serviceProvider?.GetService<INetworkClient>(); // 从服务容器获取网络客户端

                if (_networkClient != null) // 检查网络客户端是否成功获取
                {
                    Plugin.Logger?.LogInfo("[SyncManager] Network client initialized successfully"); // 记录成功日志
                }
                else

                {

                    Plugin.Logger?.LogWarning("[SyncManager] Network client not available - running in offline mode"); // 记录警告日志
                }

            }

            catch (Exception ex) // 捕获初始化异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error initializing network client: {ex.Message}"); // 记录错误日志
            }
        }


        /// <summary>
        /// 处理游戏事件的主要入口点
        /// </summary>
        /// <param name="gameEvent">需要处理的游戏事件对象</param>
        public void ProcessGameEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) // 检查事件是否为空
            {
                Plugin.Logger?.LogWarning("[SyncManager] Attempted to process null game event"); // 记录空事件警告
                return; // 退出方法
            }

            try
            {
                if (!IsNetworkAvailable()) // 检查网络是否可用
                {
                    Plugin.Logger?.LogDebug("[SyncManager] Network not available, queuing event"); // 记录调试信息
                    _eventQueue.Enqueue(gameEvent); // 将事件加入队列
                    return; // 退出方法
                }

                if (!ShouldSyncEvent(gameEvent)) // 检查事件是否需要同步
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Event {gameEvent.EventType} filtered out by sync rules"); // 记录过滤信息
                    return; // 退出方法
                }

                SendEventToNetwork(gameEvent); // 发送事件到网络
                UpdateLocalState(gameEvent); // 更新本地状态

                Plugin.Logger?.LogDebug($"[SyncManager] Processed event: {gameEvent.EventType} from {gameEvent.SourcePlayerId}"); // 记录处理完成信息
            }
            catch (Exception ex) // 捕获处理异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error processing game event {gameEvent.EventType}: {ex.Message}"); // 记录错误日志
            }
        }


        /// <summary>
        /// 接收、解析并应用来自网络的事件到本地游戏状态
        /// </summary>
        /// <param name="eventData">来自网络的原始事件数据</param>
        public void ProcessNetworkEvent(object eventData)
        {
            if (eventData == null) // 检查事件数据是否为空
            {
                Plugin.Logger?.LogWarning("[SyncManager] Received null network event data"); // 记录空数据警告
                return; // 退出方法
            }

            try
            {
                var eventDict = eventData as Dictionary<string, object>; // 将事件数据转换为字典
                if (eventDict == null || !eventDict.ContainsKey("EventType")) // 检查数据格式是否正确
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Invalid network event data format"); // 记录格式错误警告
                    return; // 退出方法
                }

                var eventType = eventDict["EventType"].ToString(); // 获取事件类型
                var payload = eventDict.ContainsKey("Payload") ? eventDict["Payload"] : null; // 获取事件载荷
                var timestamp = eventDict.ContainsKey("Timestamp") ? eventDict["Timestamp"] : DateTime.Now.Ticks; // 获取时间戳

                var gameEvent = CreateGameEventFromNetworkData(eventType, payload); // 根据网络数据创建游戏事件
                if (gameEvent == null) // 检查事件是否创建成功
                {
                    Plugin.Logger?.LogWarning($"[SyncManager] Failed to create game event from network data: {eventType}"); // 记录创建失败警告
                    return; // 退出方法
                }

                ApplyRemoteEvent(gameEvent); // 应用远程事件到本地状态
                Plugin.Logger?.LogDebug($"[SyncManager] Applied remote event: {gameEvent.EventType} from {gameEvent.SourcePlayerId}"); // 记录应用成功信息
            }
            catch (Exception ex) // 捕获处理异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error processing network event: {ex.Message}"); // 记录错误日志
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
            var playerId = GameStateUtils.GetCurrentPlayerId(); // 获取当前玩家ID

            var eventData = new // 创建事件数据对象
            {
                Timestamp = DateTime.Now.Ticks, // 添加时间戳
                CardId = cardId, // 卡牌ID
                CardName = cardName, // 卡牌名称
                CardType = cardType, // 卡牌类型
                ManaCost = ConvertManaArray(manaCost), // 转换法力数组格式
                TargetSelector = targetSelector, // 目标选择器
                PlayerState = playerState // 玩家状态
            };

            SendGameEvent(NetworkMessageTypes.OnCardPlayStart, eventData); // 发送卡牌使用事件
        }


        /// <summary>
        /// 发送法力消耗事件，同步法力变化给远程玩家
        /// </summary>
        /// <param name="manaBefore">消耗前的法力值数组[红,蓝,绿,白]</param>
        /// <param name="manaConsumed">消耗的法力值数组[红,蓝,绿,白]</param>
        /// <param name="source">法力消耗的来源（如使用卡牌、技能等）</param>
        public void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source)
        {
            var playerId = GameStateUtils.GetCurrentPlayerId(); // 获取当前玩家ID

            var eventData = new // 创建法力消耗事件数据
            {
                Timestamp = DateTime.Now.Ticks, // 添加时间戳
                PlayerId = playerId, // 玩家ID
                ManaBefore = ConvertManaArray(manaBefore), // 转换消耗前法力数组
                ManaConsumed = ConvertManaArray(manaConsumed), // 转换消耗法力数组
                Source = source // 法力消耗来源
            };

            SendGameEvent(NetworkMessageTypes.ManaConsumeStarted, eventData); // 发送法力消耗事件
        }

        // 发送法力消耗事件，同步法力变化信息到远程玩家

        /// <summary>
        /// 发送篝火选项事件，同步选择信息给远程玩家
        /// </summary>
        /// <param name="eventType">篝火事件类型（如休息、强化、升级等）</param>
        /// <param name="optionData">选项的具体数据</param>
        /// <param name="playerState">选择时的玩家状态</param>
        public void SendGapStationEvent(string eventType, object optionData, object playerState)
        {
            var playerId = GameStateUtils.GetCurrentPlayerId(); // 获取当前玩家ID

            var eventData = new // 创建篝火事件数据
            {
                Timestamp = DateTime.Now.Ticks, // 添加时间戳
                EventType = eventType, // 篝火事件类型
                PlayerId = playerId, // 玩家ID
                OptionData = optionData, // 选项数据
                PlayerState = playerState // 玩家状态
            };

            SendGameEvent(eventType, eventData); // 发送篝火事件
        }

        /// <summary>
        /// 请求完整状态同步，用于新玩家加入或断线重连
        /// </summary>
        public void RequestFullSync()
        {
            try
            {
                if (!IsNetworkAvailable()) // 检查网络是否可用
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Cannot request full sync - network not available"); // 记录网络不可用警告
                    return; // 退出方法
                }

                var syncRequest = new // 创建同步请求数据
                {
                    Timestamp = DateTime.Now.Ticks, // 添加时间戳
                    RequestType = "FullSync", // 请求类型
                    PlayerId = GameStateUtils.GetCurrentPlayerId(), // 获取当前玩家ID
                    RequestReason = "ManualRequest" // 请求原因
                };

                SendGameEvent(NetworkMessageTypes.FullStateSyncRequest, syncRequest); // 发送完整同步请求
                _lastSyncTime = DateTime.Now; // 更新最后同步时间

                Plugin.Logger?.LogInfo("[SyncManager] Full state sync requested"); // 记录同步请求日志
            }
            catch (Exception ex) // 捕获请求异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error requesting full sync: {ex.Message}"); // 记录错误日志
            }
        }


        /// <summary>
        /// 处理连接恢复事件，处理队列事件并请求完整状态同步
        /// </summary>
        public void OnConnectionRestored()
        {
            try
            {
                _isNetworkAvailable = true; // 设置网络状态为可用
                _lastConnectionTime = DateTime.Now; // 更新最后连接时间

                Plugin.Logger?.LogInfo("[SyncManager] Connection restored, processing queued events"); // 记录连接恢复日志

                while (_eventQueue.Count > 0 && IsNetworkAvailable()) // 处理队列中的所有事件
                {
                    var gameEvent = _eventQueue.Dequeue(); // 从队列中取出事件
                    ProcessGameEvent(gameEvent); // 处理事件
                }

                RequestFullSync(); // 请求完整状态同步

                SendGameEvent(NetworkMessageTypes.OnConnectionEstablished, new // 发送连接建立事件
                {
                    Timestamp = DateTime.Now.Ticks, // 添加时间戳
                    PlayerId = GameStateUtils.GetCurrentPlayerId() // 获取当前玩家ID
                });
            }
            catch (Exception ex) // 捕获处理异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error handling connection restoration: {ex.Message}"); // 记录错误日志
            }
        }

        /// <summary>
        /// 处理连接丢失事件，切换到离线模式并通知其他玩家
        /// </summary>
        public void OnConnectionLost()
        {
            try
            {
                _isNetworkAvailable = false; // 设置网络状态为不可用

                Plugin.Logger?.LogWarning("[SyncManager] Connection lost, switching to offline mode"); // 记录连接丢失警告

                SendGameEvent(NetworkMessageTypes.OnConnectionLost, new // 发送连接丢失事件
                {
                    Timestamp = DateTime.Now.Ticks, // 添加时间戳
                    PlayerId = GameStateUtils.GetCurrentPlayerId(), // 获取当前玩家ID
                    QueuedEvents = _eventQueue.Count // 队列中待处理的事件数量
                });
            }
            catch (Exception ex) // 捕获处理异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error handling connection loss: {ex.Message}"); // 记录错误日志
            }
        }

        /// <summary>
        /// 获取同步统计信息，用于调试和监控
        /// </summary>
        /// <returns>包含同步统计数据的对象</returns>
        public object GetSyncStatistics()
        {
            return new // 创建统计信息对象
            {
                QueuedEvents = _eventQueue.Count, // 队列中的事件数量
                CachedStates = _stateCache.Count, // 缓存的状态数量
                IsNetworkAvailable = _isNetworkAvailable, // 网络连接状态
                LastSyncTime = _lastSyncTime, // 最后同步时间
                LastConnectionTime = _lastConnectionTime, // 最后连接时间
                Configuration = _config // 同步配置对象
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
                if (_networkClient == null) // 检查网络客户端是否为空
                {
                    InitializeNetworkClient(); // 初始化网络客户端
                }

                _isNetworkAvailable = _networkClient?.IsConnected ?? false; // 检查网络连接状态
                return _isNetworkAvailable; // 返回网络可用状态
            }
            catch (Exception ex) // 捕获检查异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error checking network availability: {ex.Message}"); // 记录错误日志
                _isNetworkAvailable = false; // 设置网络状态为不可用
                return false; // 返回false
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
            return true; // 暂时返回true，表示所有事件都需要同步
        }

        /// <summary>
        /// 将游戏事件转换为网络数据格式并发送
        /// </summary>
        /// <param name="gameEvent">要发送的游戏事件</param>
        private void SendEventToNetwork(GameEvent gameEvent)
        {
            if (!IsNetworkAvailable()) // 检查网络是否可用
            {
                return; // 网络不可用时直接返回
            }

            try
            {
                var networkData = gameEvent.ToNetworkData(); // 将事件转换为网络数据格式
                SendGameEvent(gameEvent.EventType.ToString(), networkData); // 发送事件到网络
            }
            catch (Exception ex) // 捕获发送异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error sending event to network: {ex.Message}"); // 记录错误日志
            }
        }

        // 将游戏事件转换为网络数据格式并发送到网络

        /// <summary>
        /// 将事件数据存储到本地缓存中，避免重复同步
        /// </summary>
        /// <param name="gameEvent">需要缓存的游戏事件</param>
        private void UpdateLocalState(GameEvent gameEvent)
        {
            try
            {
                var stateKey = $"{gameEvent.EventType}_{gameEvent.SourcePlayerId}"; // 创建状态缓存键
                _stateCache[stateKey] = gameEvent.Data; // 将事件数据存储到缓存
                CleanupOldStates(); // 清理过期状态
            }
            catch (Exception ex) // 捕获更新异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error updating local state: {ex.Message}"); // 记录错误日志
            }
        }

        // 更新本地状态缓存，存储已同步的事件数据以避免重复处理

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
                return eventType switch // 根据事件类型创建对应的事件对象
                {
                    NetworkMessageTypes.OnCardPlayStart => CreateCardPlayEvent(payload), // 创建卡牌使用事件
                    NetworkMessageTypes.ManaConsumeStarted => CreateManaConsumeEvent(payload), // 创建法力消耗事件
                    NetworkMessageTypes.OnDamageDealt => CreateDamageEvent(payload), // 创建伤害事件
                    _ => new GenericGameEvent(eventType, payload) // 创建通用事件
                };
            }
            catch (Exception ex) // 捕获创建异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating game event from network data: {ex.Message}"); // 记录错误日志
                return null; // 返回null表示创建失败
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
                var dict = payload as Dictionary<string, object>; // 将载荷转换为字典
                if (dict == null) // 检查数据格式
                {
                    return null; // 格式不正确时返回null
                }

                var cardId = dict.TryGetValue("CardId", out var id) ? id?.ToString() : ""; // 获取卡牌ID
                var cardName = dict.TryGetValue("CardName", out var name) ? name?.ToString() : ""; // 获取卡牌名称
                var cardType = dict.TryGetValue("CardType", out var type) ? type?.ToString() : ""; // 获取卡牌类型

                int[] manaCost = [0, 0, 0, 0]; // 默认法力消耗
                string targetSelector = "Nobody"; // 默认目标选择器

                return GameEventFactory.CreateCardPlayEvent("remote_player", cardId, cardName, cardType, manaCost, targetSelector); // 创建卡牌事件
            }
            catch (Exception ex) // 捕获创建异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating card play event: {ex.Message}"); // 记录错误日志
                return null; // 返回null表示创建失败
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
                var dict = payload as Dictionary<string, object>; // 将载荷转换为字典
                if (dict == null) // 检查数据格式
                {
                    return null; // 格式不正确时返回null
                }

                int[] manaBefore = [0, 0, 0, 0]; // 默认消耗前法力值
                int[] manaConsumed = [0, 0, 0, 0]; // 默认消耗法力值
                string source = dict.TryGetValue("Source", out var src) ? src?.ToString() : "Unknown"; // 获取法力消耗来源

                return GameEventFactory.CreateManaConsumeEvent("remote_player", manaBefore, manaConsumed, source); // 创建法力消耗事件
            }
            catch (Exception ex) // 捕获创建异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating mana consume event: {ex.Message}"); // 记录错误日志
                return null; // 返回null表示创建失败
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
                var dict = payload as Dictionary<string, object>; // 将载荷转换为字典
                if (dict == null) // 检查数据格式
                {
                    return null; // 格式不正确时返回null
                }

                string sourceId = dict.TryGetValue("SourceId", out var src) ? src?.ToString() : ""; // 获取伤害源ID
                string targetId = dict.TryGetValue("TargetId", out var tgt) ? tgt?.ToString() : ""; // 获取目标ID
                int damageAmount = dict.TryGetValue("DamageAmount", out var dmg) && dmg != null ? Convert.ToInt32(dmg) : 0; // 获取伤害数值
                string damageType = dict.TryGetValue("DamageType", out var dmgType) ? dmgType?.ToString() : "Unknown"; // 获取伤害类型

                return GameEventFactory.CreateDamageEvent("remote_player", sourceId, targetId, damageAmount, damageType); // 创建伤害事件
            }
            catch (Exception ex) // 捕获创建异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error creating damage event: {ex.Message}"); // 记录错误日志
                return null; // 返回null表示创建失败
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
                Plugin.Logger?.LogDebug($"[SyncManager] Applied remote event: {gameEvent.EventType}"); // 记录应用事件调试信息
            }
            catch (Exception ex) // 捕获应用异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error applying remote event: {ex.Message}"); // 记录错误日志
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
                if (!IsNetworkAvailable()) // 检查网络是否可用
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Network not available for {eventType}"); // 记录网络不可用调试信息
                    return; // 退出方法
                }

                if (_networkClient is INetworkClient liteNetClient) // 检查网络客户端类型
                {
                    _networkClient.SendGameEvent(eventType, eventData); // 使用游戏事件发送方法
                }
                else
                {
                    _networkClient.SendRequest(eventType, eventData); // 使用通用请求发送方法
                }

                _lastSyncTime = DateTime.Now; // 更新最后同步时间
            }
            catch (Exception ex) // 捕获发送异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error sending game event {eventType}: {ex.Message}"); // 记录错误日志
            }
        }

        /// <summary>
        /// 将法力数组转换为包含各颜色法力和总量的结构化对象
        /// </summary>
        /// <param name="manaArray">法力数组[红,蓝,绿,白]</param>
        /// <returns>结构化的法力对象</returns>
        private object ConvertManaArray(int[] manaArray)
        {
            if (manaArray == null || manaArray.Length < 4) // 检查数组是否为空或长度不足
            {
                return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 }; // 返回默认法力对象
            }

            return new // 创建法力对象
            {
                Red = manaArray[0], // 红色法力
                Blue = manaArray[1], // 蓝色法力
                Green = manaArray[2], // 绿色法力
                White = manaArray[3], // 白色法力
                Total = manaArray[0] + manaArray[1] + manaArray[2] + manaArray[3] // 法力总量
            };
        }

        /// <summary>
        /// 移除过期的状态缓存条目，防止内存泄漏和性能问题
        /// </summary>
        private void CleanupOldStates()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow - _config.StateCacheExpiry; // 计算截止时间
                var keysToRemove = new List<string>(); // 创建待删除键列表

                foreach (var kvp in _stateCache) // 遍历状态缓存
                {
                    if (kvp.Key.Contains("Old") || kvp.Key.Contains("Temp")) // 检查是否为过期或临时状态
                    {
                        keysToRemove.Add(kvp.Key); // 添加到删除列表
                    }
                }

                foreach (var key in keysToRemove) // 删除过期状态
                {
                    _stateCache.Remove(key); // 从缓存中移除
                }
            }
            catch (Exception ex) // 捕获清理异常
            {
                Plugin.Logger?.LogError($"[SyncManager] Error cleaning up old states: {ex.Message}"); // 记录错误日志
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
    public class GenericGameEvent(string eventType, object data) : GameEvent(ParseEventType(eventType), "unknown_player", data) // 调用基类构造函数
    {

        /// <summary>
        /// 解析事件类型字符串为枚举值，失败时返回Error类型
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <returns>对应的游戏事件类型枚举</returns>
        private static GameEventType ParseEventType(string eventType)
        {
            return Enum.TryParse<GameEventType>(eventType, out var result) ? result : GameEventType.Error; // 尝试解析枚举，失败时返回Error类型
        }

        /// <summary>
        /// 重写基类方法，提供通用的事件序列化
        /// </summary>
        /// <returns>网络传输格式的事件数据</returns>
        public override object ToNetworkData()
        {
            return new // 创建网络数据对象
            {
                EventType = EventType.ToString(), // 事件类型字符串
                Timestamp = Timestamp.Ticks, // 时间戳
                SourcePlayerId, // 来源玩家ID
                Data // 事件数据
            };
        }

        /// <summary>
        /// 对于通用事件，直接返回当前实例
        /// </summary>
        /// <param name="data">网络数据</param>
        /// <returns>恢复的事件实例</returns>
        public override GameEvent FromNetworkData(object data)
        {
            return this; // 直接返回当前实例
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
        // 控制卡牌使用、抽取等行为的同步开关

        /// <summary>
        /// 控制法力消耗、恢复等行为的同步
        /// </summary>
        public bool EnableManaSync { get; set; } = true;
        // 控制法力消耗、恢复等行为的同步开关

        /// <summary>
        /// 控制伤害、状态效果等战斗行为的同步
        /// </summary>
        public bool EnableBattleSync { get; set; } = true;
        // 控制伤害、状态效果等战斗行为的同步开关

        /// <summary>
        /// 控制地图探索、节点状态等地图行为的同步
        /// </summary>
        public bool EnableMapSync { get; set; } = true;
        // 控制地图探索、节点状态等地图行为的同步开关

        /// <summary>
        /// 网络不可用时，事件队列的最大容量
        /// </summary>
        public int MaxQueueSize { get; set; } = 100;
        // 网络不可用时，事件队列的最大容量限制

        /// <summary>
        /// 状态缓存的存活时间，超过此时间的缓存将被清理
        /// </summary>
        public TimeSpan StateCacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
        // 状态缓存的存活时间，超过此时间的缓存将被自动清理
    }
}