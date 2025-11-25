using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Events;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin.Core
{
    /// <summary>
    /// 核心同步管理器 - 负责协调所有游戏状态的同步
    /// 这是联机MOD的大脑，整合了所有Patch点和网络通信
    /// 参考杀戮尖塔联机Mod的多人游戏状态管理系统
    /// </summary>
    public class SynchronizationManager
    {
        private static SynchronizationManager _instance;
        private readonly IServiceProvider _serviceProvider;
        private readonly INetworkClient _networkClient;

        // 事件队列
        private readonly Queue<GameEvent> _eventQueue = new Queue<GameEvent>();

        // 状态缓存
        private readonly Dictionary<string, object> _stateCache = new Dictionary<string, object>();

        // 同步配置
        private readonly SyncConfiguration _config = new SyncConfiguration();

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
            _networkClient = _serviceProvider?.GetService<INetworkClient>();

            Plugin.Logger?.LogInfo("[SyncManager] Synchronization Manager initialized");
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
                // 解析网络事件
                var gameEvent = ParseNetworkEvent(eventData);
                if (gameEvent == null)
                {
                    Plugin.Logger?.LogWarning("[SyncManager] Failed to parse network event");
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
                ManaCost = manaCost,
                TargetSelector = targetSelector,
                PlayerState = playerState
            };

            SendNetworkRequest("OnCardPlayStart", eventData);
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

            SendNetworkRequest("ManaConsumeStarted", eventData);
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

            SendNetworkRequest(eventType, eventData);
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

                var json = JsonSerializer.Serialize(syncRequest);
                _networkClient.SendRequest("StateSyncRequest", json);

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
                Plugin.Logger?.LogInfo("[SyncManager] Connection restored, processing queued events");

                // 处理队列中的事件
                while (_eventQueue.Count > 0 && IsNetworkAvailable())
                {
                    var gameEvent = _eventQueue.Dequeue();
                    ProcessGameEvent(gameEvent);
                }

                // 请求完整状态同步
                RequestFullSync();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error handling connection restoration: {ex.Message}");
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
                IsNetworkAvailable = IsNetworkAvailable(),
                LastSyncTime = GetLastSyncTime(),
                Configuration = _config
            };
        }

        // 私有方法

        private bool IsNetworkAvailable()
        {
            return _networkClient?.IsConnected == true;
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
                var json = JsonSerializer.Serialize(networkData);
                _networkClient.SendRequest(gameEvent.EventType.ToString(), json);
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

        private GameEvent ParseNetworkEvent(object eventData)
        {
            try
            {
                // TODO: 实现网络事件解析逻辑
                // 根据事件类型创建相应的GameEvent实例
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error parsing network event: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error applying remote event: {ex.Message}");
            }
        }

        private void SendNetworkRequest(string requestType, object eventData)
        {
            try
            {
                if (!IsNetworkAvailable())
                {
                    Plugin.Logger?.LogDebug($"[SyncManager] Network not available for {requestType}");
                    return;
                }

                var json = JsonSerializer.Serialize(eventData);
                _networkClient.SendRequest(requestType, json);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SyncManager] Error sending network request {requestType}: {ex.Message}");
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
            // TODO: 实现状态缓存清理逻辑
            // 移除过期的状态条目
        }

        private DateTime GetLastSyncTime()
        {
            // TODO: 实现获取最后同步时间的逻辑
            return DateTime.Now;
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