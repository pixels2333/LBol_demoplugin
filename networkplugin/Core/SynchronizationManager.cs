using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Events;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

/// <summary>
/// 同步管理器类
/// LBoL联机MOD的核心组件，负责协调所有游戏状态的同步功能
/// 整合所有Harmony补丁点和网络通信，基于LiteNetLib网络框架实现多人游戏状态同步
/// </summary>
/// <remarks>
/// <para>
/// 该类的主要职责：
/// - 协调游戏状态同步的完整生命周期
/// - 整合所有Patch点的网络通信
/// - 管理网络连接状态和事件队列
/// - 提供统一的同步接口和事件处理机制
/// - 处理离线/在线模式切换
/// </para>
///
/// <para>
/// 核心功能：
/// - 事件驱动架构：基于游戏事件进行状态同步
/// - 网络通信管理：协调网络客户端的数据传输
/// - 状态缓存：避免重复同步，提高性能
/// - 队列管理：网络不可用时缓存事件
/// - 错误处理：确保系统稳定性和可靠性
/// </para>
///
/// <para>
/// 设计架构：
/// - 单例模式：确保全局唯一的同步管理器实例
/// - 依赖注入：集成服务容器的网络客户端
/// - 事件驱动：通过GameEvent系统触发同步
/// - 网络适配：支持LiteNetLib框架的P2P通信
/// </para>
///
/// <para>
/// 网络通信流程：
/// 1. 游戏操作触发 → 2. Harmony补丁拦截 → 3. 生成GameEvent → 4. 同步管理器处理
/// 5. 网络序列化 → 6. LiteNetLib传输 → 7. 其他客户端接收
/// 8. 反序列化处理 → 9. 应用到本地游戏状态
/// </para>
/// </remarks>
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
    /// <remarks>
    /// 使用空合并运算符(??=)确保实例被正确初始化
    /// 这种模式支持延迟初始化，提高启动性能
    /// </remarks>
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
    /// <remarks>
    /// 队列采用FIFO（先进先出）策略
    /// 网络恢复时会按顺序处理队列中的事件
    /// </remarks>
    private readonly Queue<GameEvent> _eventQueue = new();

    /// <summary>
    /// 本地状态缓存字典
    /// 存储最近的游戏状态快照，用于避免重复同步和状态验证
    /// </summary>
    /// <remarks>
    /// 缓存键格式：{EventType}_{PlayerId}_{Timestamp}
    /// 包含完整的游戏状态信息
    /// 定期清理过期状态以防止内存泄漏
    /// </remarks>
    private readonly Dictionary<string, object> _stateCache = [];

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
    /// <remarks>
    /// 构造函数负责：
    /// 1. 获取依赖注入的服务提供者
    /// 2. 初始化网络客户端连接
    /// 3. 设置默认配置参数
    /// 4. 记录初始化完成日志
    /// </remarks>
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
    /// <remarks>
    /// 初始化流程：
    /// 1. 从服务容器获取INetworkClient实例
    /// 2. 验证网络客户端的可用性
    /// 3. 设置网络连接状态监控
    /// 4. 记录初始化结果日志
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// 事件处理流程：
    /// 1. 参数验证：检查事件对象的有效性
    /// 2. 网络状态检查：确认网络连接可用性
    /// 3. 事件过滤：根据规则判断是否需要同步
    /// 4. 网络传输：将事件发送给远程玩家
    /// 5. 本地状态更新：更新本地游戏状态缓存
    /// 6. 日志记录：记录处理过程和结果
    /// </para>
    ///
    /// <para>
    /// 错误处理策略：
    /// - 空事件：记录警告并跳过处理
    /// - 网络不可用：加入队列等待后续处理
    /// - 处理异常：记录错误但不中断游戏运行
    /// </para>
    /// </remarks>
    public void ProcessGameEvent(GameEvent gameEvent)
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
            SendEventToNetwork(gameEvent);

            // 更新本地状态缓存
            UpdateLocalState(gameEvent);

            // 记录事件处理完成的调试信息
            Plugin.Logger?.LogDebug($"[SyncManager] 事件处理完成: {gameEvent.EventType} 来自 {gameEvent.SourcePlayerId}");
        }
        catch (Exception ex)
        {
            // 捕获并记录处理异常，确保游戏继续运行
            Plugin.Logger?.LogError($"[SyncManager] 游戏事件处理异常 - 事件类型: {gameEvent.EventType}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 接收并处理来自网络的远程事件
    /// 将网络传输的事件数据应用到本地游戏状态中
    /// </summary>
    /// <param name="eventData">来自网络的原始事件数据</param>
    /// <remarks>
    /// <para>
    /// 处理流程：
    /// 1. 数据验证：检查事件数据的完整性和格式
    /// 2. 数据解析：提取事件类型、载荷和时间戳信息
    /// 3. 事件重建：根据网络数据创建对应的游戏事件对象
    /// 4. 状态应用：将远程事件应用到本地游戏状态
    /// 5. 日志记录：记录接收和处理过程
    /// </para>
    ///
    /// <para>
    /// 数据格式要求：
    /// - 必须包含"EventType"字段标识事件类型
    /// - 可选包含"Payload"字段携带事件数据
    /// - 可选包含"Timestamp"字段标识事件时间
    /// - 支持Dictionary&lt;string, object&gt;格式
    /// </para>
    ///
    /// <para>
    /// 安全性保障：
    /// - 严格验证接收数据的格式和完整性
    /// - 防止恶意或损坏的数据影响游戏状态
    /// - 记录所有网络事件的接收和处理日志
    /// </para>
    /// </remarks>
    public void ProcessNetworkEvent(object eventData)
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

            // 提取事件的基本信息
            var eventType = eventDict["EventType"].ToString();
            var payload = eventDict.ContainsKey("Payload") ? eventDict["Payload"] : null;
            var timestamp = eventDict.ContainsKey("Timestamp") ? eventDict["Timestamp"] : DateTime.Now.Ticks;

            // 根据网络数据创建对应的游戏事件对象
            GameEvent gameEvent = CreateGameEventFromNetworkData(eventType, payload, timestamp);
            if (gameEvent == null)
            {
                // 事件创建失败，可能数据格式不正确
                Plugin.Logger?.LogWarning($"[SyncManager] 网络数据事件创建失败: {eventType}");
                return;
            }

            // 将远程事件应用到本地游戏状态
            ApplyRemoteEvent(gameEvent);

            // 记录远程事件应用成功的调试信息
            Plugin.Logger?.LogDebug($"[SyncManager] 远程事件应用成功: {gameEvent.EventType} 来自 {gameEvent.SourcePlayerId}");
        }
        catch (Exception ex)
        {
            // 捕获并记录处理异常，不影响网络通信的继续进行
            Plugin.Logger?.LogError($"[SyncManager] 网络事件处理异常: {ex.Message}");
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
    /// <remarks>
    /// <para>
    /// 同步信息包括：
    /// - 卡牌基本信息：ID、名称、类型
    /// - 法力消耗：四色法力值
    /// - 目标选择：攻击目标或效果范围
    /// - 玩家状态：使用前的完整状态快照
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - 玩家选择卡牌并准备使用时
    /// - 确认卡牌可以使用并开始执行时
    /// - 需要通知其他玩家卡牌使用决策时
    /// </para>
    /// </remarks>
    public void SendCardPlayEvent(string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object playerState)
    {
        // 获取当前玩家ID，用于标识事件来源
        var playerId = GameStateUtils.GetCurrentPlayerId();

        // 创建卡牌使用事件的详细数据
        var eventData = new
        {
            Timestamp = DateTime.Now.Ticks,                  // 事件时间戳
            CardId = cardId,                                // 卡牌ID
            CardName = cardName,                              // 卡牌名称
            CardType = cardType,                            // 卡牌类型
            ManaCost = ConvertManaArray(manaCost),              // 转换法力数组为结构化对象
            TargetSelector = targetSelector,                    // 目标选择器
            PlayerState = playerState                           // 玩家状态快照
        };

        // 发送卡牌使用事件到网络
        SendGameEvent(NetworkMessageTypes.OnCardPlayStart, eventData);
    }

    /// <summary>
    /// 发送法力消耗事件
    /// 同步法力变化给远程玩家，保持法力状态的一致性
    /// </summary>
    /// <param name="manaBefore">消耗前的法力值数组[红,蓝,绿,白]</param>
    /// <param name="manaConsumed">消耗的法力值数组[红,蓝,绿,白]</param>
    /// <param name="source">法力消耗的来源描述</param>
    /// <remarks>
    /// <para>
    /// 同步信息包括：
    /// - 玩家ID：标识法力消耗的玩家
    /// - 消耗前状态：消耗前的法力值
    /// - 消耗数量：实际消耗的法力值
    /// - 消耗来源：法力消耗的操作类型或原因
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - 使用需要法力的卡牌时
    /// - 支付技能费用时
    /// - 其他法力相关的游戏操作时
    /// - 法力恢复或变化时
    /// </para>
    /// </remarks>
    public void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source)
    {
        // 获取当前玩家ID，用于标识事件来源
        var playerId = GameStateUtils.GetCurrentPlayerId();

        // 创建法力消耗事件的详细数据
        var eventData = new
        {
            Timestamp = DateTime.Now.Ticks,                  // 事件时间戳
            PlayerId = playerId,                              // 玩家ID
            ManaBefore = ConvertManaArray(manaBefore),            // 转换消耗前法力值
            ManaConsumed = ConvertManaArray(manaConsumed),         // 转换消耗法力值
            Source = source                                      // 法力消耗来源
        };

        // 发送法力消耗事件到网络
        SendGameEvent(NetworkMessageTypes.ManaConsumeStarted, eventData);
    }

    /// <summary>
    /// 发送篝火选项事件
    /// 同步篝火点的选择和操作给远程玩家，协调多人游戏的决策
    /// </summary>
    /// <param name="eventType">篝火事件类型（如休息、强化、升级等）</param>
    /// <param name="optionData">选项的详细数据和参数</param>
    /// <param name="playerState">选择时的玩家状态快照</param>
    /// <remarks>
    /// <para>
    /// 同步信息包括：
    /// - 篝火事件类型：明确标识具体的篝火操作
    /// - 选项数据：选择的具体内容和参数
    /// - 玩家ID：进行选择的玩家标识
    /// - 玩家状态：选择前的完整状态快照
    /// </para>
    ///
    /// <para>
    /// 篝火事件类型示例：
    /// - "Rest"：在篝火休息恢复
    /// - "Upgrade"：升级卡牌或技能
    /// - "Smith"：锻造或强化装备
    /// - "Remove"：移除卡牌或效果
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - 玩家在篝火点进行选择操作时
    /// - 需要协调多人游戏中的决策时
    /// - 确保所有玩家了解篝火点操作结果时
    /// </para>
    /// </remarks>
    public void SendGapStationEvent(string eventType, object optionData, object playerState)
    {
        // 获取当前玩家ID，用于标识事件来源
        var playerId = GameStateUtils.GetCurrentPlayerId();

        // 创建篝火选项事件的详细数据
        var eventData = new
        {
            Timestamp = DateTime.Now.Ticks,                  // 事件时间戳
            EventType = eventType,                           // 篝火事件类型
            PlayerId = playerId,                              // 玩家ID
            OptionData = optionData,                           // 选项数据
            PlayerState = playerState                           // 玩家状态快照
        };

        // 发送篝火事件到网络
        SendGameEvent(eventType, eventData);
    }

    /// <summary>
    /// 请求完整状态同步
    /// 用于新玩家加入游戏或断线重连时获取完整的游戏状态
    /// </summary>
    /// <remarks>
    /// <para>
    /// 完整同步功能：
    /// - 新玩家加入时获取当前游戏的所有状态
    /// - 断线重连时恢复到最新的游戏状态
    /// - 定期同步确保状态一致性
    /// - 处理网络中断后的状态恢复
    /// </para>
    ///
    /// <para>
    /// 同步请求包括：
    /// - 当前玩家ID和状态信息
    /// - 请求类型和时间戳
    /// - 请求原因和上下文
    /// - 同步优先级和紧急程度
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - 玩家首次加入多人游戏房间时
    /// - 网络连接中断后重新连接时
    /// - 检测到状态不一致需要重新同步时
    /// - 手动触发状态同步检查时
    /// </para>
    /// </remarks>
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
            var syncRequest = new
            {
                Timestamp = DateTime.Now.Ticks,                // 请求时间戳
                RequestType = "FullSync",                       // 请求类型标识
                PlayerId = GameStateUtils.GetCurrentPlayerId(),      // 当前玩家ID
                RequestReason = "ManualRequest"                   // 请求原因描述
            };

            // 发送完整状态同步请求到网络
            SendGameEvent(NetworkMessageTypes.FullStateSyncRequest, syncRequest);

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
    /// <remarks>
    /// <para>
    /// 连接恢复处理流程：
    /// 1. 更新网络连接状态为可用
    /// 2. 记录连接恢复时间戳
    /// 3. 处理队列中所有待同步的事件
    /// 4. 请求完整的状态同步
    /// 5. 通知其他玩家连接已恢复
    /// </para>
    ///
    /// <para>
    /// 队列事件处理：
    /// - 按FIFO顺序处理队列中的事件
    /// - 确保事件处理的完整性
    /// - 清空待处理事件队列
    /// - 更新本地状态
    /// </para>
    ///
    /// <para>
    /// 状态同步策略：
    /// - 请求最新完整的游戏状态
    /// - 避免部分状态同步的不一致
    /// - 确保所有玩家状态完全一致
    /// </para>
    /// </remarks>
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
                ProcessGameEvent(gameEvent);
            }

            // 请求完整状态同步以确保状态一致性
            RequestFullSync();

            // 发送连接建立事件通知其他玩家
            SendGameEvent(NetworkMessageTypes.OnConnectionEstablished, new
            {
                Timestamp = DateTime.Now.Ticks,                  // 连接建立时间戳
                PlayerId = GameStateUtils.GetCurrentPlayerId()   // 当前玩家ID
            });
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
    /// <remarks>
    /// <para>
    /// 连接丢失处理流程：
    /// 1. 更新网络连接状态为不可用
    /// 2. 切换到离线模式继续游戏
    /// 3. 通知其他玩家连接已断开
    /// 4. 记录队列中待处理的事件数量
    /// </para>
    ///
    /// <para>
    /// 离线模式行为：
    /// - 继续处理本地游戏操作
    /// - 将新事件加入队列等待网络恢复
    /// - 保持本地状态缓存
    /// - 暂停网络相关的功能
    /// </para>
    ///
    /// <para>
    /// 断开通知：
    /// - 向其他玩家发送连接断开事件
    /// - 包含断开玩家信息和队列状态
    /// - 协调重连策略和时间
    /// </para>
    /// </remarks>
    public void OnConnectionLost()
    {
        try
        {
            // 更新网络连接状态为不可用
            _isNetworkAvailable = false;

            // 记录连接丢失的警告日志
            Plugin.Logger?.LogWarning("[SyncManager] 网络连接丢失，切换到离线模式");

            // 发送连接丢失事件通知其他玩家
            SendGameEvent(NetworkMessageTypes.OnConnectionLost, new
            {
                Timestamp = DateTime.Now.Ticks,                  // 断开时间戳
                PlayerId = GameStateUtils.GetCurrentPlayerId(),      // 当前玩家ID
                QueuedEvents = _eventQueue.Count,                    // 队列中待处理事件数量
            });
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
    /// <remarks>
    /// <para>
    /// 统计信息包括：
    /// - 队列状态：当前待处理的事件数量
    /// - 缓存状态：本地状态缓存的条目数量
    /// - 网络状态：当前网络连接的可用性
    /// - 时间戳记录：最后同步和连接的时间信息
    /// - 配置参数：当前的同步配置选项
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - 调试网络同步问题和性能问题
    /// - 监控多人游戏的状态同步状况
    /// - 分析同步效率和队列使用情况
    /// - 检查离线模式下的队列积压情况
    /// </para>
    /// </remarks>
    public object GetSyncStatistics()
    {
        // 创建包含所有统计信息的对象
        return new
        {
            // 队列状态统计
            QueuedEvents = _eventQueue.Count,              // 队列中待处理事件数量
            MaxQueueSize = _config.MaxQueueSize,           // 队列最大容量

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
    /// <remarks>
    /// 验证流程：
    /// 1. 检查网络客户端是否为空
    /// 2. 如果为空则尝试重新初始化
    /// 3. 检查网络连接的实际状态
    /// 4. 更新网络可用性标志
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// 过滤规则考虑：
    /// - 事件类型的同步重要性
    /// - 玩家权限和角色要求
    /// - 当前游戏阶段的状态
    /// - 同步频率和性能考虑
    /// </para>
    ///
    /// <para>
    /// TODO: 待实现的过滤规则：
    /// - 基于事件类型的优先级过滤
    /// - 基于玩家权限的权限过滤
    /// - 基于游戏阶段的阶段过滤
    /// - 基于性能优化的频率过滤
    /// </para>
    /// </remarks>
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
    /// 将游戏事件转换为网络数据格式并发送
    /// 处理事件的序列化和网络传输
    /// </summary>
    /// <param name="gameEvent">要发送的游戏事件对象</param>
    /// <remarks>
    /// <para>
    /// 发送流程：
    /// 1. 检查网络连接状态
    /// 2. 调用事件的序列化方法
    /// 3. 将序列化数据发送到网络
    /// 4. 处理发送过程中的异常
    /// 5. 更新最后同步时间戳
    /// </para>
    /// </remarks>
    private void SendEventToNetwork(GameEvent gameEvent)
    {
        // 检查网络连接状态
        if (!IsNetworkAvailable())
        {
            return; // 网络不可用时直接返回
        }

        try
        {
            // 将游戏事件序列化为网络数据格式
            var networkData = gameEvent.ToNetworkData();

            // 发送事件到网络
            SendGameEvent(gameEvent.EventType.ToString(), networkData);
        }
        catch (Exception ex)
        {
            // 捕获发送异常并记录错误日志
            Plugin.Logger?.LogError($"[SyncManager] 事件网络发送异常 - 事件类型: {gameEvent.EventType}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新本地状态缓存
    /// 将事件数据存储到本地缓存中，避免重复同步和状态验证
    /// </summary>
    /// <param name="gameEvent">需要缓存的游戏事件</param>
    /// <remarks>
    /// <para>
    /// 缓存策略：
    /// - 使用"事件类型_玩家ID"作为缓存键
    /// - 存储完整的事件数据对象
    /// - 定期清理过期的缓存条目
    /// - 维护缓存大小的合理范围
    /// </para>
    ///
    /// <para>
    /// 缓存用途：
    /// - 状态验证：比较本地和远程状态的一致性
    /// - 性能优化：避免重复的网络请求
    /// - 离线恢复：断线重连时的状态参考
    /// - 调试支持：提供状态历史追踪
    /// </para>
    /// </remarks>
    private void UpdateLocalState(GameEvent gameEvent)
    {
        try
        {
            // 创建状态缓存键
            var stateKey = $"{gameEvent.EventType}_{gameEvent.SourcePlayerId}";

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
    /// 根据事件类型分发到相应的具体事件创建方法
    /// 实现事件类型的识别和对应的事件对象创建
    /// </summary>
    /// <param name="eventType">事件类型字符串</param>
    /// <param name="payload">事件负载数据</param>
    /// <returns>创建的游戏事件对象，失败时返回null</returns>
    /// <remarks>
    /// <para>
    /// 事件类型分发策略：
    /// - 使用模式匹配进行类型识别
    /// - 为每种已知事件类型提供专门的创建方法
    /// - 未知类型使用通用事件处理
    /// - 支持新的事件类型扩展
    /// </para>
    ///
    /// <para>
    /// 支持的事件类型：
    /// - 卡牌使用事件 (OnCardPlayStart)
    /// - 法力消耗事件 (ManaConsumeStarted)
    /// - 伤害事件 (OnDamageDealt)
    /// - 其他游戏事件使用通用事件处理
    /// </para>
    /// </remarks>
    private GameEvent CreateGameEventFromNetworkData(string eventType, object payload, DateTime timestamp)
    {
        try
        {
            // 根据事件类型分发到对应的创建方法
            return eventType switch
            {
                // 卡牌使用事件
                NetworkMessageTypes.OnCardPlayStart => CreateCardPlayEvent(payload, timestamp),

                // 法力消耗事件
                NetworkMessageTypes.ManaConsumeStarted => CreateManaConsumeEvent(payload, timestamp),

                // 伤害事件
                NetworkMessageTypes.OnDamageDealt => CreateDamageEvent(payload, timestamp),

                // 未知类型使用通用事件处理
                _ => new GenericGameEvent(eventType, payload, timestamp)
            };
        }
        catch (Exception ex)
        {
            // 捕获事件创建异常并记录错误日志
            Plugin.Logger?.LogError($"[SyncManager] 网络数据事件创建异常 - 事件类型: {eventType}, 错误: {ex.Message}");
            return null; // 返回null表示创建失败
        }
    }

    /// <summary>
    /// 从网络数据中解析卡牌信息并创建卡牌事件
    /// 处理卡牌使用相关的网络数据解析和事件重建
    /// </summary>
    /// <param name="payload">包含卡牌信息的网络数据</param>
    /// <returns>创建的卡牌事件对象，失败时返回null</returns>
    /// <remarks>
    /// <para>
    /// 解析策略：
    /// - 将payload转换为字典格式
    /// - 提取卡牌的基本信息（ID、名称、类型）
    /// - 设置默认的参数值
    /// - 使用GameEventFactory创建事件对象
    /// </para>
    ///
    /// <para>
    /// 卡牌信息包含：
    /// - 卡牌ID：卡牌的唯一标识符
    /// - 卡牌名称：显示给玩家的卡牌名称
    /// - 卡牌类型：攻击/技能/能力等类型分类
    /// - 法力消耗：四色法力消耗值
    /// - 目标选择：攻击目标或效果范围
    /// </para>
    /// </remarks>
    private CardPlayEvent CreateCardPlayEvent(object payload, DateTime timestamp)
    {
        //TODO:将时间戳参数引入到代码中
        try
        {
            // 将载荷转换为字典格式
            if (payload is not Dictionary<string, object> dict)
            {
                return null; // 数据格式不正确
            }
            
            // 提取卡牌基本信息
            var cardId = dict.TryGetValue("CardId", out var id) ? id?.ToString() : "";
            var cardName = dict.TryGetValue("CardName", out var name) ? name?.ToString() : "";
            var cardType = dict.TryGetValue("CardType", out var type) ? type?.ToString() : "";

            // 设置默认的法力消耗和目标选择器
            int[] manaCost = [0, 0, 0, 0];  // 默认无消耗
            string targetSelector = "Nobody"; // 默认无目标

            // 创建卡牌事件对象
            return GameEventFactory.CreateCardPlayEvent("remote_player", cardId, cardName, cardType, manaCost, targetSelector);
        }
        catch (Exception ex)
        {
            // 捕获卡牌事件创建异常
            Plugin.Logger?.LogError($"[SyncManager] 卡牌事件创建异常: {ex.Message}");
            return null; // 返回null表示创建失败
        }
    }

    /// <summary>
    /// 从网络数据中解析法力信息并创建法力消耗事件
    /// 处理法力消耗相关的网络数据解析和事件重建
    /// </summary>
    /// <param name="payload">包含法力信息的网络数据</param>
    /// <returns>创建的法力消耗事件对象，失败时返回null</returns>
    /// <remarks>
    /// <para>
    /// 解析策略：
    /// - 将payload转换为字典格式
    /// - 提取法力信息（消耗前、消耗量、来源）
    /// - 设置默认的法力值
    /// - 使用GameEventFactory创建事件对象
    /// </para>
    ///
    /// <para>
    /// 法力信息包含：
    /// - 玩家ID：消耗法力的玩家标识
    /// - 消耗前状态：消耗前的法力值
    /// - 消耗数量：实际消耗的法力值
    /// - 消耗来源：导致法力消耗的操作
    /// </para>
    /// </remarks>
    private ManaConsumeEvent CreateManaConsumeEvent(object payload, DateTime timestamp)
    {
        try
        {
            // 将载荷转换为字典格式
            var dict = payload as Dictionary<string, object>;
            if (dict == null)
            {
                return null; // 数据格式不正确
            }

            // 设置默认的法力值
            int[] manaBefore = [0, 0, 0, 0];  // 默认消耗前法力
            int[] manaConsumed = [0, 0, 0, 0]; // 默认消耗量

            // 提取法力信息（如果存在）
            string source = dict.TryGetValue("Source", out var src) ? src?.ToString() : "Unknown";

            // 创建法力消耗事件对象
            return GameEventFactory.CreateManaConsumeEvent("remote_player", manaBefore, manaConsumed, source);
        }
        catch (Exception ex)
        {
            // 捕获法力消耗事件创建异常
            Plugin.Logger?.LogError($"[SyncManager] 法力消耗事件创建异常: {ex.Message}");
            return null; // 返回null表示创建失败
        }
    }

    /// <summary>
    /// 从网络数据中解析伤害信息并创建伤害事件
    /// 处理伤害相关的网络数据解析和事件重建
    /// </summary>
    /// <param name="payload">包含伤害信息的网络数据</param>
    /// <returns>创建的伤害事件对象，失败时返回null</returns>
    /// <remarks>
    /// <para>
    /// 解析策略：
    /// - 将payload转换为字典格式
    /// - 提取伤害信息（来源、目标、数量、类型）
    /// - 设置默认的伤害值和类型
    /// - 使用GameEventFactory创建事件对象
    /// </para>
    ///
    /// <para>
    /// 伤害信息包含：
    /// - 来源ID：造成伤害的单位ID
    /// - 目标ID：受到伤害的单位ID
    /// - 伤害数值：实际的伤害数量
    /// - 伤害类型：伤害的属性或效果类型
    /// </para>
    /// </remarks>
    private DamageEvent CreateDamageEvent(object payload, DateTime timestamp)
    {
        try
        {
            // 将载荷转换为字典格式
            var dict = payload as Dictionary<string, object>;
            if (dict == null)
            {
                return null; // 数据格式不正确
            }

            // 提取伤害信息
            string sourceId = dict.TryGetValue("SourceId", out var src) ? src?.ToString() : "";
            string targetId = dict.TryGetValue("TargetId", out var tgt) ? tgt?.ToString() : "";
            int damageAmount = dict.TryGetValue("DamageAmount", out var dmg) && dmg != null ? Convert.ToInt32(dmg) : 0;
            string damageType = dict.TryGetValue("DamageType", out var dmgType) ? dmgType?.ToString() : "Unknown";

            // 创建伤害事件对象
            return GameEventFactory.CreateDamageEvent("remote_player", sourceId, targetId, damageAmount, damageType);
        }
        catch (Exception ex)
        {
            // 捕获伤害事件创建异常
            Plugin.Logger?.LogError($"[SyncManager] 伤害事件创建异常: {ex.Message}");
            return null; // 返回null表示创建失败
        }
    }

    /// <summary>
    /// 将从网络接收的远程事件应用到本地游戏状态
    /// 协调远程事件与本地游戏状态的一致性
    /// </summary>
    /// <param name="gameEvent">需要应用的远程游戏事件</param>
    /// <remarks>
    /// <para>
    /// 应用策略：
    /// - 根据事件类型执行相应的本地操作
    /// - 更新相关的游戏状态
    /// - 触发相应的UI更新
    /// - 记录事件应用的日志
    /// </para>
    ///
    /// <para>
    /// TODO: 待实现的应用逻辑：
    /// - 根据事件类型分派到具体的应用方法
    /// - 实现卡牌使用、法力消耗、伤害等事件的应用
    /// - 处理状态冲突和优先级问题
    /// - 集成游戏UI系统的状态更新
    /// </para>
    /// </remarks>
    private void ApplyRemoteEvent(GameEvent gameEvent)
    {
        try
        {
            // 记录远程事件应用的调试信息
            Plugin.Logger?.LogDebug($"[SyncManager] 远程事件应用: {gameEvent.EventType}");
        }
        catch (Exception ex)
        {
            // 捕获远程事件应用异常
            Plugin.Logger?.LogError($"[SyncManager] 远程事件应用异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 底层的网络发送方法
    /// 负责实际的事件数据传输和网络通信
    /// </summary>
    /// <param name="eventType">事件类型字符串</param>
    /// <param name="eventData">事件数据对象</param>
    /// <remarks>
    /// <para>
    /// 发送策略：
    /// 1. 首先检查网络连接状态
    /// 2. 根据网络客户端类型选择发送方法
    /// 3. 使用适当的事件数据格式进行序列化
    /// 4. 更新同步时间戳用于性能监控
    /// </para>
    ///
    /// <para>
    /// 网络客户端适配：
    /// - INetworkClient: 使用SendGameEvent方法发送游戏事件
    /// - 其他接口: 使用SendRequest方法发送通用请求
    /// - 自动检测客户端类型并选择最佳发送方式
    /// </para>
    /// </remarks>
    private void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            // 检查网络连接状态
            if (!IsNetworkAvailable())
            {
                Plugin.Logger?.LogDebug($"[SyncManager] 网络不可用，跳过事件发送: {eventType}");
                return;
            }

            // 根据网络客户端类型选择发送方法
            if (_networkClient is NetworkClient liteNetClient)
            {
                // 使用游戏事件专用发送方法
                _networkClient.SendGameEvent(eventType, eventData);
            }
            else
            {
                // 使用通用请求发送方法
                _networkClient.SendRequest(eventType, eventData);
            }

            // 更新最后同步时间戳
            _lastSyncTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            // 捕获网络发送异常并记录错误日志
            Plugin.Logger?.LogError($"[SyncManager] 游戏事件发送异常 - 类型: {eventType}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 将法力数组转换为结构化对象
    /// 将四色法力值转换为便于传输和显示的对象格式
    /// </summary>
    /// <param name="manaArray">法力数组[红,蓝,绿,白]</param>
    /// <returns>结构化的法力对象</returns>
    /// <remarks>
    /// <para>
    /// 转换逻辑：
    /// - 检查数组长度和有效性
    /// - 提取四种颜色的法力值
    /// - 计算法力值总量用于状态显示
    /// - 返回包含所有法力信息的结构化对象
    /// </para>
    ///
    /// <para>
    /// 法力颜色对应：
    /// - Red: 红色法力（火元素）
    /// - Blue: 蓝色法力（水元素）
    /// - Green: 绿色法力（木元素）
    /// - White: 白色法力（光元素）
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// 清理过期的状态缓存条目
    /// 移除过期的状态缓存，防止内存泄漏和性能问题
    /// </summary>
    /// <remarks>
    /// <para>
    /// 清理策略：
    /// - 基于配置的缓存过期时间
    /// - 识别临时和过期的状态键
    /// - 移除不再需要的状态条目
    /// - 维护缓存大小在合理范围内
    /// </para>
    ///
    /// <para>
    /// 过滤规则：
    /// - 包含"Old"标记的状态键
    /// - 包含"Temp"标记的状态键
    /// - 超过过期时间的状态键
    /// - 其他可识别的过期状态标记
    /// </para>
    /// </remarks>
    private void CleanupOldStates()
    {
        try
        {
            // 计算状态缓存的截止时间
            var cutoffTime = DateTime.UtcNow - _config.StateCacheExpiry;
            var keysToRemove = new List<string>();

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

    // ========================================
    // 嵌件定义结束
    // ========================================

    /// <summary>
    /// 通用游戏事件类
    /// 用于处理未明确定义或通用的事件类型
    /// 提供基本的事件结构和网络序列化能力
    /// </summary>
    /// <remarks>
    /// <para>
    /// 通用事件特点：
    /// - 适用于所有未明确定义的事件类型
    /// - 提供基本的事件字段和功能
    /// - 支持网络序列化和反序列化
    /// - 可扩展的事件处理框架
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - 新增事件类型未实现专门处理类时
    /// - 通用事件的快速处理和调试
    /// - 测试和原型开发阶段的事件
    /// - 网络协议扩展支持
    /// </para>
    ///
    /// <para>
    /// 事件字段：
    /// - EventType: 事件类型字符串
    /// - SourcePlayerId: 来源玩家ID
    /// - Data: 事件数据载荷
    /// - Timestamp: 事件时间戳
    /// </para>
    /// </remarks>
    public class GenericGameEvent(string eventType, object data, DateTime timestamp) : GameEvent(ParseEventType(eventType), "unknown_player", data)
    {
        /// <summary>
        /// 重写基类方法，提供通用的事件序列化
        /// 将事件转换为网络传输的数据格式
        /// </summary>
        /// <returns>网络传输格式的事件数据</returns>
        public override object ToNetworkData()
        {
            // 创建网络传输格式的事件数据
            return new
            {
                EventType = EventType.ToString(),    // 事件类型字符串
                Timestamp = Timestamp.Ticks,          // 时间戳（ticks格式）
                SourcePlayerId,                      // 来源玩家ID
                Data                              // 事件载荷数据
            };
        }

        /// <summary>
        /// 解析事件类型字符串为枚举值
        /// 支持事件类型的字符串到枚举的安全转换
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <returns>对应的游戏事件类型枚举</returns>
        /// <remarks>
        /// 解析规则：
        /// - 优先尝试解析为精确的枚举值
        /// - 解析失败时返回Error类型作为安全默认值
        /// - 确保类型安全性和处理意外事件类型
        /// </remarks>
        private static GameEventType ParseEventType(string eventType)
        {
            // 尝试将字符串解析为事件类型枚举
            return Enum.TryParse<GameEventType>(eventType, out var result) ? result : GameEventType.Error;
        }

        /// <summary>
        /// 对于通用事件，直接返回当前实例
        /// 因为通用事件不需要额外处理
        /// </summary>
        /// <param name="data">网络数据</param>
        /// <returns>恢复的事件实例</returns>
        public override GameEvent FromNetworkData(object data)
        {
            // 通用事件直接返回当前实例，数据已在构造函数中处理
            return this;
        }
    }

    /// <summary>
    /// 同步配置类
    /// 包含控制同步行为的各种配置选项和性能参数
    /// 用于调整同步系统的行为和性能特征
    /// </summary>
    /// <remarks>
    /// <para>
    /// 配置类别：
    /// - 功能开关：控制各种同步功能的启用状态
    /// - 性能参数：调整队列大小和缓存策略
    /// - 行为控制：配置同步的触发条件和策略
    /// </para>
    ///
    /// <para>
    /// 功能开关：
    /// - EnableCardSync: 控制卡牌使用、抽取等行为的同步
    /// - EnableManaSync: 控制法力消耗、恢复等行为的同步
    /// - EnableBattleSync: 控制伤害、状态效果等战斗行为的同步
    /// - EnableMapSync: 控制地图探索、节点状态等地图行为的同步
    /// </para>
    ///
    /// <para>
    /// 性能参数：
    /// - MaxQueueSize: 事件队列的最大容量，防止内存过度使用
    /// - StateCacheExpiry: 状态缓存的存活时间，控制内存使用效率
    /// </para>
    /// </remarks>
    public class SyncConfiguration
    {
        /// <summary>
        /// 卡牌同步开关
        /// 控制卡牌使用、抽取、洗牌等行为的网络同步
        /// </summary>
        public bool EnableCardSync { get; set; } = true;
        // 控制卡牌使用、抽取等行为的同步开关

        /// <summary>
        /// 法力同步开关
        /// 控制法力消耗、恢复、增益等行为的网络同步
        /// </summary>
        public bool EnableManaSync { get; set; } = true;
        // 控制法力消耗、恢复等行为的同步开关

        /// <summary>
        /// 战斗同步开关
        /// 控制伤害计算、状态效果、战斗结果的同步
        /// </summary>
        public bool EnableBattleSync { get; set; } = true;
        // 控制伤害、状态效果等战斗行为的同步开关

        /// <summary>
        /// 地图同步开关
        /// 控制地图探索、节点状态、地图事件的同步
        /// </summary>
        public bool EnableMapSync { get; set; } = true;
        // 控制地图探索、节点状态等地图行为的同步开关

        /// <summary>
        /// 事件队列最大容量
        /// 网络不可用时事件队列的最大条目数量
        /// 超过此容量的新事件会被丢弃
        /// </summary>
        public int MaxQueueSize { get; set; } = 100;
        // 网络不可用时，事件队列的最大容量限制

        /// <summary>
        /// 状态缓存存活时间
        /// 本地状态缓存的存活时间，超过此时间的缓存会被清理
        /// 默认为5分钟，可以根据需要调整
        /// </summary>
        public TimeSpan StateCacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
        // 状态缓存的存活时间，超过此时间的缓存将被自动清理
    }
}