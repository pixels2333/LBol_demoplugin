using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

/// <summary>
/// 同步管理器类
/// LBoL联机MOD的核心组件，负责协调所有游戏状态的同步功能
/// 整合所有Harmony补丁点和网络通信，基于LiteNetLib网络框架实现多人游戏状态同步
/// </summary>

public class SynchronizationManager
{
    // ========================================
    // 单例模式相关字段
    // ========================================

    /// <summary>
    /// 全局唯一的同步管理器实例
    /// 使用单例模式确保整个游戏系统中只有一个同步管理器
    /// </summary>
    private static SynchronizationManager _instance;

    /// <summary>
    /// 获取全局唯一的同步管理器实例
    /// 提供线程安全的单例访问方式
    /// </summary>
    /// <returns>同步管理器的全局实例</returns>

    public static SynchronizationManager Instance
    {
        get
        {
            // 使用空合并运算符确保实例被正确初始化
            _instance ??= new SynchronizationManager();
            return _instance;
        }
    }

    // ========================================
    // 依赖注入和网络服务
    // ========================================

    /// <summary>
    /// 依赖注入服务提供者
    /// 用于获取和管理其他MOD服务的依赖关系
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 网络客户端接口
    /// 负责与LiteNetLib网络框架的通信
    /// </summary>
    private INetworkClient _networkClient;

    // ========================================
    /// 状态管理和缓存
    // ========================================

    /// <summary>
    /// 网络不可用时的事件队列
    /// 存储在断线或连接问题时需要同步的游戏事件
    /// </summary>

    private readonly Queue<GameEvent> _eventQueue = new();

    private readonly SortedList<long, NetworkEventBuffer> _remoteEventBuffer = [];

    /// <summary>
    /// 本地状态缓存字典
    /// 存储最近的游戏状态快照，用于避免重复同步和状态验证
    /// </summary>

    private readonly Dictionary<string, object> _stateCache = [];

    /// <summary>
    /// 远程事件缓冲区清理阈值
    /// 超过此时间的事件将被视为超时并自动清理
    /// </summary>
    private static readonly TimeSpan EventBufferTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 同步配置对象
    /// 包含各种同步行为和性能参数的配置选项
    /// </summary>
    private readonly SyncConfiguration _config = new();

    // ========================================
    /// 网络状态管理
    // ========================================

    /// <summary>
    /// 网络连接状态标志
    /// 标识当前是否可以与远程玩家进行网络通信
    /// </summary>
    private bool _isNetworkAvailable = false;

    /// <summary>
    /// 最后网络连接时间
    /// 记录最近一次成功建立网络连接的时间戳
    /// 用于连接状态监控和重连策略
    /// </summary>
    private DateTime _lastConnectionTime = DateTime.MinValue;

    /// <summary>
    /// 最后状态同步时间
    /// 记录最近一次成功完成状态同步的时间戳
    /// 用于同步频率控制和性能监控
    /// </summary>
    private DateTime _lastSyncTime = DateTime.MinValue;

    /// <summary>
    /// 私有构造函数
    /// 初始化同步管理器的依赖项和基础配置
    /// </summary>

    private SynchronizationManager()
    {
        // 获取MOD服务提供者，用于依赖注入
        _serviceProvider = ModService.ServiceProvider;

        // 初始化网络客户端连接
        InitializeNetworkClient();

        // 记录同步管理器初始化完成的日志
        Plugin.Logger?.LogInfo("[SyncManager] 同步管理器初始化完成");
    }

    /// <summary>
    /// 从依赖注入容器获取并初始化网络客户端
    /// 建立与LiteNetLib网络框架的连接
    /// </summary>

    private void InitializeNetworkClient()
    {
        try
        {
            // 从服务容器获取网络客户端实例
            _networkClient = _serviceProvider?.GetService<INetworkClient>();

            // 验证网络客户端是否成功获取
            if (_networkClient != null)
            {
                // 网络客户端初始化成功
                Plugin.Logger?.LogInfo("[SyncManager] 网络客户端初始化成功");
            }
            else
            {
                // 网络客户端不可用，运行在离线模式
                Plugin.Logger?.LogWarning("[SyncManager] 网络客户端不可用 - 运行在离线模式");
            }
        }
        catch (Exception ex)
        {
            // 捕获初始化过程中的异常
            Plugin.Logger?.LogError($"[SyncManager] 网络客户端初始化错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理游戏事件的主要入口点
    /// 这是所有游戏状态同步的核心方法，协调本地和远程状态的一致性
    /// </summary>
    /// <param name="gameEvent">需要处理的游戏事件对象</param>
    public void SyncGameEventToNetwork(GameEvent gameEvent)
    {
        // 参数验证：确保事件对象不为空
        if (gameEvent == null)
        {
            Plugin.Logger?.LogWarning("[SyncManager] 尝试处理空的游戏事件");
            return;
        }

        try
        {
            // 检查网络连接状态
            if (!IsNetworkAvailable())
            {
                // 网络不可用时将事件加入队列
                Plugin.Logger?.LogDebug("[SyncManager] 网络不可用，事件加入队列");
                _eventQueue.Enqueue(gameEvent);
                return;
            }

            // 检查事件是否需要同步
            if (!ShouldSyncEvent(gameEvent))
            {
                // 事件被过滤规则排除，跳过同步
                Plugin.Logger?.LogDebug($"[SyncManager] 事件 {gameEvent.EventType} 被同步规则过滤");
                return;
            }

            // 发送事件到网络进行远程同步
            SendGameEvent(gameEvent);

            // 更新本地状态缓存
            UpdateLocalState(gameEvent);

            // 记录事件处理完成的调试信息
            Plugin.Logger?.LogDebug($"[SyncManager] 事件处理完成: {gameEvent.EventType} 来自 {gameEvent.PlayerId}");
        }
        catch (Exception ex)
        {
            // 捕获并记录处理异常，确保游戏继续运行
            Plugin.Logger?.LogError($"[SyncManager] 游戏事件处理异常 - 事件类型: {gameEvent.EventType}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 接收并处理来自网络的远程事件
    /// 将网络传输的事件数据按时间戳有序地应用到本地游戏状态中
    /// </summary>
    /// <param name="eventData">来自网络的原始事件数据</param>

    public void ProcessEventFromNetwork(object eventData)
    {
        // 参数验证：确保事件数据不为空
        if (eventData == null)
        {
            Plugin.Logger?.LogWarning("[SyncManager] 接收到空的网络事件数据");
            return;
        }

        try
        {
            // 验证数据格式是否正确
            if (eventData is not Dictionary<string, object> eventDict || !eventDict.ContainsKey("EventType"))
            {
                Plugin.Logger?.LogWarning("[SyncManager] 无效的网络事件数据格式");
                return;
            }

            // 提取时间戳，如果不存在则使用当前时间
            long timestamp = eventDict.ContainsKey("Timestamp")
                ? Convert.ToInt64(eventDict["Timestamp"])
                : DateTime.Now.Ticks;

            // 创建网络事件缓冲区并添加到排序缓冲区
            NetworkEventBuffer eventBuffer = new(timestamp, eventDict);
            _remoteEventBuffer.Add(timestamp, eventBuffer);

            // 记录事件接收的调试信息
            string eventType = eventDict["EventType"].ToString();
            Plugin.Logger?.LogDebug($"[SyncManager] 接收到网络事件: {eventType}, 时间戳: {timestamp}");

            // 处理缓冲区中的事件（按时间戳顺序）
            ProcessBufferedEvents();
        }
        catch (Exception ex)
        {
            // 捕获并记录处理异常，不影响网络通信的继续进行
            Plugin.Logger?.LogError($"[SyncManager] 网络事件处理异常: {ex.Message}");
        }
    }

    // ========================================
    // 有序事件处理方法
    // ========================================

    /// <summary>
    /// 处理缓冲区中的网络事件
    /// 按时间戳顺序处理所有可用的网络事件
    /// </summary>
    private void ProcessBufferedEvents()
    {
        try
        {
            // 首先清理超时的事件
            CleanupTimeoutEvents();

            // 按时间戳顺序处理所有等待处理的事件
            List<long> timestampsToRemove = [];

            foreach (var kvp in _remoteEventBuffer)
            {
                long timestamp = kvp.Key;
                NetworkEventBuffer eventBuffer = kvp.Value;

                // 检查事件状态和处理条件
                if (eventBuffer.Status != NetworkEventBuffer.ProcessingStatus.Pending)
                {
                    continue; // 跳过非等待处理状态的事件
                }

                // 检查事件是否超时
                if (eventBuffer.IsTimeout(EventBufferTimeout))
                {
                    Plugin.Logger?.LogWarning($"[SyncManager] 事件超时，丢弃: {eventBuffer.OriginalData["EventType"]}, 时间戳: {timestamp}");
                    eventBuffer.Status = NetworkEventBuffer.ProcessingStatus.Discarded;
                    timestampsToRemove.Add(timestamp);
                    continue;
                }

                try
                {
                    // 标记事件为处理中
                    eventBuffer.Status = NetworkEventBuffer.ProcessingStatus.Processing;

                    // 处理单个网络事件
                    ProcessSingleNetworkEvent(eventBuffer);

                    // 标记事件为处理完成
                    eventBuffer.Status = NetworkEventBuffer.ProcessingStatus.Completed;

                    // 记录处理成功日志
                    string eventType = eventBuffer.OriginalData["EventType"].ToString();
                    Plugin.Logger?.LogDebug($"[SyncManager] 事件处理成功: {eventType}, 时间戳: {timestamp}");
                }
                catch (Exception ex)
                {
                    // 事件处理失败，记录错误并跳过
                    Plugin.Logger?.LogError($"[SyncManager] 事件处理失败 - 时间戳: {timestamp}, 错误: {ex.Message}");

                    // 标记事件为已丢弃，避免重复处理失败
                    eventBuffer.Status = NetworkEventBuffer.ProcessingStatus.Discarded;
                    timestampsToRemove.Add(timestamp);
                }
            }

            // 移除已处理或丢弃的事件
            foreach (var timestamp in timestampsToRemove)
            {
                _remoteEventBuffer.Remove(timestamp);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SyncManager] 缓冲区事件处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理单个网络事件
    /// 将事件数据转换为游戏事件并应用到本地状态
    /// </summary>
    /// <param name="eventBuffer">网络事件缓冲区</param>
    private void ProcessSingleNetworkEvent(NetworkEventBuffer eventBuffer)
    {
        Dictionary<string, object> eventDict = eventBuffer.OriginalData;

        // 提取事件的基本信息
        string eventType = eventDict["EventType"].ToString();
        var payload = eventDict.ContainsKey("Payload") ? eventDict["Payload"] : string.Empty;

        // 从时间戳创建DateTime对象
        DateTime timestamp = new(eventBuffer.Timestamp);

        // 根据网络数据创建对应的游戏事件对象
        GameEvent gameEvent = CreateGameEventFromNetworkData(eventType, payload, timestamp) ?? throw new InvalidOperationException($"无法创建游戏事件: {eventType}");

        // 将远程事件应用到本地游戏状态
        ApplyRemoteEvent(gameEvent);

        // 记录事件应用的详细信息
        Plugin.Logger?.LogDebug($"[SyncManager] 单个事件应用成功: {gameEvent.EventType} 来自 {gameEvent.PlayerId} (时间戳: {timestamp})");
    }

    private GameEvent CreateGameEventFromNetworkData(string eventType, object payload, DateTime timestamp)
    {
        //TODO:根据不同的type创建不同的GameEvent
        throw new NotImplementedException();
    }

    /// <summary>
    /// 清理缓冲区中的超时事件
    /// 移除超过指定时间未处理的事件，防止内存泄漏
    /// </summary>


    private void CleanupTimeoutEvents()
    {
        try
        {
            List<long> timestampsToRemove = [];

            foreach (var kvp in _remoteEventBuffer)
            {
                var timestamp = kvp.Key;
                var eventBuffer = kvp.Value;

                // 只清理等待处理且超时的事件
                if (eventBuffer.Status == NetworkEventBuffer.ProcessingStatus.Pending &&
                    eventBuffer.IsTimeout(EventBufferTimeout))
                {
                    Plugin.Logger?.LogWarning($"[SyncManager] 清理超时事件: {eventBuffer.OriginalData["EventType"]}, 时间戳: {timestamp}");
                    timestampsToRemove.Add(timestamp);
                }
            }

            // 移除超时事件
            foreach (var timestamp in timestampsToRemove)
            {
                _remoteEventBuffer.Remove(timestamp);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SyncManager] 超时事件清理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取远程事件缓冲区统计信息
    /// 用于调试和监控有序事件处理的状态
    /// </summary>
    /// <returns>包含缓冲区统计信息的对象</returns>
    public object GetEventBufferStatistics()
    {
        try
        {
            Dictionary<NetworkEventBuffer.ProcessingStatus, int> statusCounts = [];

            // 初始化状态计数
            foreach (NetworkEventBuffer.ProcessingStatus status in Enum.GetValues(typeof(NetworkEventBuffer.ProcessingStatus)))
            {
                statusCounts[status] = 0;
            }

            // 统计各状态的事件数量
            foreach (var kvp in _remoteEventBuffer)
            {
                var status = kvp.Value.Status;
                statusCounts[status]++;
            }

            // 计算时间戳范围
            long? oldestTimestamp = null;
            long? newestTimestamp = null;

            if (_remoteEventBuffer.Count > 0)
            {
                oldestTimestamp = _remoteEventBuffer.Keys[0];
                newestTimestamp = _remoteEventBuffer.Keys[_remoteEventBuffer.Count - 1];
            }

            return new
            {
                // 缓冲区基本信息
                TotalEvents = _remoteEventBuffer.Count,
                BufferSize = _remoteEventBuffer.Capacity,

                // 状态分布统计
                StatusDistribution = statusCounts.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),

                // 时间戳信息
                OldestTimestamp = oldestTimestamp,
                NewestTimestamp = newestTimestamp,
                TimeRange = oldestTimestamp.HasValue && newestTimestamp.HasValue
                    ? TimeSpan.FromTicks(newestTimestamp.Value - oldestTimestamp.Value)
                    : (TimeSpan?)null,

                // 超时信息
                TimeoutThreshold = EventBufferTimeout.TotalSeconds,
                ActiveTimeoutCheck = true
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SyncManager] 获取缓冲区统计异常: {ex.Message}");
            return new { Error = ex.Message };
        }
    }

    // ========================================
    // 专用事件发送方法
    // ========================================

    /// <summary>
    /// 发送卡牌使用事件
    /// 同步卡牌基本信息、法力消耗和目标选择等卡牌使用相关的状态
    /// </summary>
    /// <param name="cardId">使用的卡牌唯一标识符</param>
    /// <param name="cardName">卡牌显示名称</param>
    /// <param name="cardType">卡牌类型（攻击/技能/能力牌等）</param>
    /// <param name="manaCost">法力消耗数组[红,蓝,绿,白]</param>
    /// <param name="targetSelector">目标选择器字符串描述</param>
    /// <param name="playerState">使用卡牌时的玩家状态快照</param>
    public void SendCardPlayEvent(string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object playerState)
    {
        // 获取当前玩家ID，用于标识事件来源
        var playerId = GameStateUtils.GetCurrentPlayerId();

        // 创建卡牌使用事件的详细数据
        var eventData = new Dictionary<string, object>
        {
            ["CardId"] = cardId,
            ["CardName"] = cardName,
            ["CardType"] = cardType,
            ["ManaCost"] = manaCost,
            ["TargetSelector"] = targetSelector,
            ["PlayerState"] = playerState ?? ""
        };

        // 创建卡牌使用事件并发送到网络
        GameEvent cardPlayEvent = new("CardPlayed", playerId, eventData);
        SendGameEvent(cardPlayEvent);
    }

    /// <summary>
    /// 发送法力消耗事件
    /// 同步法力变化给远程玩家，保持法力状态的一致性
    /// </summary>
    /// <param name="manaBefore">消耗前的法力值数组[红,蓝,绿,白]</param>
    /// <param name="manaConsumed">消耗的法力值数组[红,蓝,绿,白]</param>
    /// <param name="source">法力消耗的来源描述</param>
    public void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source)
    {
        // 获取当前玩家ID，用于标识事件来源
        var playerId = GameStateUtils.GetCurrentPlayerId();

        // 创建法力消耗事件的详细数据
        var eventData = new Dictionary<string, object>
        {
            ["ManaBefore"] = ConvertManaArray(manaBefore),
            ["ManaConsumed"] = ConvertManaArray(manaConsumed),
            ["Source"] = source
        };

        // 发送法力消耗事件到网络
        GameEvent manaEvent = new("ManaConsumeStarted", playerId, eventData);
        SendGameEvent(manaEvent);
    }

    /// <summary>
    /// 发送篝火选项事件
    /// 同步篝火点的选择和操作给远程玩家，协调多人游戏的决策
    /// </summary>
    /// <param name="eventType">篝火事件类型（如休息、强化、升级等）</param>
    /// <param name="optionData">选项的详细数据和参数</param>
    /// <param name="playerState">选择时的玩家状态快照</param>
    public void SendGapStationEvent(string eventType, object optionData, object playerState)
    {
        // 获取当前玩家ID，用于标识事件来源
        var playerId = GameStateUtils.GetCurrentPlayerId();

        // 创建篝火选项事件的详细数据
        var eventData = new Dictionary<string, object>
        {
            ["OptionData"] = optionData ?? "",
            ["PlayerState"] = playerState ?? ""
        };

        // 发送篝火事件到网络
        GameEvent campEvent = new(eventType, playerId, eventData);
        SendGameEvent(campEvent);
    }

    /// <summary>
    /// 请求完整状态同步
    /// 用于新玩家加入游戏或断线重连时获取完整的游戏状态
    /// </summary>
    public void RequestFullSync()
    {
        try
        {
            // 检查网络连接状态
            if (!IsNetworkAvailable())
            {
                Plugin.Logger?.LogWarning("[SyncManager] 无法请求完整状态同步 - 网络连接不可用");
                return;
            }

            // 创建完整状态同步请求数据
            var syncRequestData = new Dictionary<string, object>
            {
                ["RequestType"] = "FullSync",                       // 请求类型标识
                ["RequestReason"] = "ManualRequest"                   // 请求原因描述
            };
            var playerId = GameStateUtils.GetCurrentPlayerId();

            // 发送完整状态同步请求到网络
            GameEvent syncEvent = new(NetworkMessageTypes.FullStateSyncRequest.ToString(), playerId, syncRequestData);
            SendGameEvent(syncEvent);

            // 更新最后同步时间戳
            _lastSyncTime = DateTime.Now;

            // 记录完整同步请求日志
            Plugin.Logger?.LogInfo("[SyncManager] 发起完整状态同步请求");
        }
        catch (Exception ex)
        {
            // 捕获并记录同步请求异常
            Plugin.Logger?.LogError($"[SyncManager] 完整状态同步请求异常: {ex.Message}");
        }
    }

    // ========================================
    /// 网络连接状态处理方法
    // ========================================

    /// <summary>
    /// 处理网络连接恢复事件
    /// 当网络重新连接可用时，处理队列中的待处理事件并请求状态同步
    /// </summary>
    public void OnConnectionRestored()
    {
        try
        {
            // 更新网络连接状态
            _isNetworkAvailable = true;

            // 记录连接恢复时间戳
            _lastConnectionTime = DateTime.Now;

            // 记录连接恢复的日志信息
            Plugin.Logger?.LogInfo("[SyncManager] 网络连接已恢复，开始处理队列事件");

            // 处理队列中的所有待处理事件
            while (_eventQueue.Count > 0 && IsNetworkAvailable())
            {
                var gameEvent = _eventQueue.Dequeue();
                SyncGameEventToNetwork(gameEvent);
            }

            // 请求完整状态同步以确保状态一致性
            RequestFullSync();

            // 发送连接建立事件通知其他玩家
            var connectionData = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.Ticks,                  // 连接建立时间戳
                ["PlayerId"] = GameStateUtils.GetCurrentPlayerId()   // 当前玩家ID
            };
            SendGameEvent(new GameEvent(NetworkMessageTypes.OnConnectionEstablished, GameStateUtils.GetCurrentPlayerId(), connectionData));
        }
        catch (Exception ex)
        {
            // 捕获并记录连接恢复处理异常
            Plugin.Logger?.LogError($"[SyncManager] 连接恢复处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理网络连接丢失事件
    /// 当网络连接断开时，切换到离线模式并通知其他玩家
    /// </summary>
    public void OnConnectionLost()
    {
        try
        {
            // 更新网络连接状态为不可用
            _isNetworkAvailable = false;

            // 记录连接丢失的警告日志
            Plugin.Logger?.LogWarning("[SyncManager] 网络连接丢失，切换到离线模式");

            // 发送连接丢失事件通知其他玩家
            var playerId = GameStateUtils.GetCurrentPlayerId();
            var eventData = new Dictionary<string, object>
            {
                ["QueuedEvents"] = _eventQueue.Count
            };
            GameEvent connectionLostEvent = new("ConnectionLost", playerId, eventData);
            SendGameEvent(connectionLostEvent);
        }
        catch (Exception ex)
        {
            // 捕获并记录连接丢失处理异常
            Plugin.Logger?.LogError($"[SyncManager] 连接丢失处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取同步统计信息
    /// 返回同步管理器的运行状态和性能指标，用于调试和系统监控
    /// </summary>
    /// <returns>包含同步统计数据的对象</returns>
    public object GetSyncStatistics()
    {
        // 获取远程事件缓冲区的统计信息
        var bufferStats = GetEventBufferStatistics();

        // 创建包含所有统计信息的对象
        return new
        {
            // 队列状态统计
            QueuedEvents = _eventQueue.Count,              // 队列中待处理事件数量
            MaxQueueSize = _config.MaxQueueSize,           // 队列最大容量

            // 缓冲区状态统计
            EventBuffer = bufferStats,                     // 远程事件缓冲区统计

            // 缓存状态统计
            CachedStates = _stateCache.Count,                  // 状态缓存条目数量
            CacheExpiry = _config.StateCacheExpiry,           // 缓存过期时间

            // 网络连接状态
            IsNetworkAvailable = _isNetworkAvailable,          // 网络连接可用性
            LastSyncTime = _lastSyncTime,                   // 最后同步时间戳
            LastConnectionTime = _lastConnectionTime,           // 最后连接时间戳

            // 配置参数
            Configuration = _config                           // 完整的配置对象
        };
    }

    // ========================================
    /// 私有辅助方法
    // ========================================

    /// <summary>
    /// 验证网络客户端的连接状态
    /// 检查网络客户端是否可用，必要时尝试重新初始化
    /// </summary>
    /// <returns>如果网络可用返回true，否则返回false</returns>
    private bool IsNetworkAvailable()
    {
        try
        {
            // 检查网络客户端是否为空
            if (_networkClient == null)
            {
                // 尝试重新初始化网络客户端
                InitializeNetworkClient();
            }

            // 更新网络可用性状态
            _isNetworkAvailable = _networkClient?.IsConnected ?? false;

            // 返回网络可用性状态
            return _isNetworkAvailable;
        }
        catch (Exception ex)
        {
            // 捕获网络检查异常
            Plugin.Logger?.LogError($"[SyncManager] 网络可用性检查异常: {ex.Message}");

            // 设置网络状态为不可用
            _isNetworkAvailable = false;
            return false;
        }
    }

    /// <summary>
    /// 根据事件类型、玩家权限和游戏阶段判断是否需要同步事件
    /// 实现事件过滤逻辑，避免不必要的网络传输
    /// </summary>
    /// <param name="gameEvent">需要判断同步的游戏事件</param>
    /// <returns>如果事件需要同步返回true，否则返回false</returns>
    private bool ShouldSyncEvent(GameEvent gameEvent)
    {
        // TODO: 实现详细的事件过滤逻辑
        // 1. 根据事件类型判断同步必要性
        // 2. 检查玩家权限设置
        // 3. 考虑当前游戏阶段
        // 4. 评估网络同步的性能影响
        // 5. 应用配置的过滤规则

        // 临时返回true，表示所有事件都需要同步
        return true;
    }


    /// <summary>
    /// 更新本地状态缓存
    /// 将事件数据存储到本地缓存中，避免重复同步和状态验证
    /// </summary>
    /// <param name="gameEvent">需要缓存的游戏事件</param>
    private void UpdateLocalState(GameEvent gameEvent)
    {
        try
        {
            // 创建状态缓存键
            var stateKey = $"{gameEvent.EventType}_{gameEvent.PlayerId}";

            // 将事件数据存储到缓存中
            _stateCache[stateKey] = gameEvent.Data;

            // 清理过期的状态缓存条目
            CleanupOldStates();
        }
        catch (Exception ex)
        {
            // 捕获状态更新异常并记录错误日志
            Plugin.Logger?.LogError($"[SyncManager] 本地状态更新异常: {ex.Message}");
        }
    }




    /// <summary>
    /// 验证事件时间戳的有效性
    /// 检查时间戳的合理性和一致性
    /// </summary>
    /// <param name="timestamp">要验证的时间戳</param>
    /// <returns>如果时间戳有效返回true，否则返回false</returns>
    private bool ValidateEventTimestamp(DateTime timestamp)
    {
        try
        {
            var now = DateTime.Now;
            var maxFutureTime = now.AddSeconds(5);  // 允许5秒的未来偏差
            var minValidTime = now.AddHours(-1);    // 1小时前的事件视为有效

            // 检查时间戳是否在未来（允许合理偏差）
            if (timestamp > maxFutureTime)
            {
                Plugin.Logger?.LogWarning($"[SyncManager] 事件时间戳在未来: {timestamp}, 当前时间: {now}");
                return false;
            }

            // 检查时间戳是否太早（超过1小时）
            if (timestamp < minValidTime)
            {
                Plugin.Logger?.LogWarning($"[SyncManager] 事件时间戳太早: {timestamp}, 当前时间: {now}");
                return false;
            }

            // 检查时间戳是否为最小值或最大值（可能的数据损坏）
            if (timestamp == DateTime.MinValue || timestamp == DateTime.MaxValue)
            {
                Plugin.Logger?.LogWarning($"[SyncManager] 事件时间戳异常: {timestamp}");
                return false;
            }

            // 时间戳验证通过
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SyncManager] 时间戳验证异常: {ex.Message}");
            return false;
        }
    }




    /// <summary>
    /// 将从网络接收的远程事件应用到本地游戏状态
    /// 协调远程事件与本地游戏状态的一致性
    /// </summary>
    /// <param name="gameEvent">需要应用的远程游戏事件</param>
    private void ApplyRemoteEvent(GameEvent gameEvent)
    {
        // TODO:具体逻辑未完成


    }

    /// <summary>
    /// 底层的网络发送方法
    /// 负责实际的事件数据传输和网络通信
    /// </summary>
    /// <param name="eventType">事件类型字符串</param>
    /// <param name="eventData">事件数据对象</param>
    /// TODO:STOP
    public void SendGameEvent(GameEvent gameEvent)
    {
        try
        {
            // 检查网络连接状态
            if (!IsNetworkAvailable())
            {
                Plugin.Logger?.LogDebug($"[SyncManager] 网络不可用，跳过事件发送: {gameEvent.EventType}");
                return;
            }

            // 根据网络客户端类型选择发送方法
            if (_networkClient is NetworkClient liteNetClient)
            {
                // 使用游戏事件专用发送方法
                _networkClient.SendGameEvent(gameEvent.EventType.ToString(), gameEvent.Data);
            }
            else
            {
                // 使用通用请求发送方法
                _networkClient.SendRequest(gameEvent.EventType.ToString(), gameEvent.Data);
            }

            // 更新最后同步时间戳
            _lastSyncTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            // 捕获网络发送异常并记录错误日志
            Plugin.Logger?.LogError($"[SyncManager] 游戏事件发送异常 - 类型: {gameEvent.EventType}, 错误: {ex.Message}");
        }
    }


    /// <summary>
    /// 将法力数组转换为结构化对象
    /// 将四色法力值转换为便于传输和显示的对象格式
    /// </summary>
    /// <param name="manaArray">法力数组[红,蓝,绿,白]</param>
    /// <returns>结构化的法力对象</returns>
    private object ConvertManaArray(int[] manaArray)
    {
        // 检查数组是否为空或长度不足
        if (manaArray == null || manaArray.Length < 4)
        {
            // 返回默认的空法力对象
            return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
        }

        // 创建结构化的法力对象
        return new
        {
            Red = manaArray[0],    // 红色法力值
            Blue = manaArray[1],   // 蓝色法力值
            Green = manaArray[2],  // 绿色法力值
            White = manaArray[3],  // 白色法力值
            Total = manaArray[0] + manaArray[1] + manaArray[2] + manaArray[3] // 法力总量
        };
    }



    private void CleanupOldStates()
    {
        try
        {
            // 计算状态缓存的截止时间
            var cutoffTime = DateTime.UtcNow - _config.StateCacheExpiry;
            List<string> keysToRemove = [];

            // 遍历状态缓存查找过期条目
            foreach (var kvp in _stateCache)
            {
                // 识别临时和过期的状态键
                if (kvp.Key.Contains("Old") || kvp.Key.Contains("Temp"))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            // 删除识别出的过期状态
            foreach (var key in keysToRemove)
            {
                _stateCache.Remove(key);
            }
        }
        catch (Exception ex)
        {
            // 捕获状态清理异常
            Plugin.Logger?.LogError($"[SyncManager] 状态缓存清理异常: {ex.Message}");
        }
    }

}
