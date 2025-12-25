using System;
using System.Collections.Generic;
using System.Text.Json;
using BepInEx.Logging;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天控制台类
/// 管理LBoL联机MOD中的聊天功能，负责聊天消息的发送、接收、历史记录和事件处理
/// 这是玩家之间进行实时交流的核心组件
/// </summary>
/// <remarks>
/// <para>
/// 该类负责处理网络聊天功能，包括：
/// - 发送和接收聊天消息 - 支持实时通信
/// - 管理聊天历史记录 - 维护消息历史，支持查询和清理
/// - 提供聊天事件通知 - 通过事件系统通知UI更新
/// - 支持多种消息类型 - 普通、系统、动作等不同类型的消息
/// </para>
///
/// <para>
/// 消息类型说明：
/// - Normal: 普通聊天消息，玩家之间的日常交流
/// - System: 系统消息，如连接状态、错误提示等
/// - Action: 动作消息，如打牌、购买道具等游戏操作通知
/// - Battle: 战斗消息，战斗中的重要事件通知
/// </para>
///
/// <para>
/// 设计参考: 杀戮尖塔 Together in Spire 的聊天系统架构
/// 采用了事件驱动的消息处理机制，便于与游戏UI系统集成
/// </para>
/// </remarks>
/// <param name="networkClient">网络客户端接口，用于发送消息到其他玩家</param>
/// <param name="logger">日志记录器，用于记录聊天相关的操作和错误</param>
/// <exception cref="ArgumentNullException">当任一参数为null时抛出异常</exception>
public class ChatConsole(INetworkClient networkClient, ManualLogSource logger)
{
    /// <summary>
    /// 网络客户端接口，用于发送聊天消息到网络中的其他玩家
    /// </summary>
    private readonly INetworkClient _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));

    /// <summary>
    /// 日志记录器，用于记录聊天相关的操作和错误信息
    /// </summary>
    private readonly ManualLogSource _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// 聊天历史记录列表，按时间顺序存储所有接收和发送的消息
    /// </summary>
    private readonly List<ChatMessage> _chatHistory = [];

    /// <summary>
    /// 聊天历史记录的最大条数限制
    /// 超过此数量时会自动删除最旧的消息，防止内存占用过大
    /// </summary>
    private const int MaxHistorySize = 100;

    /// <summary>
    /// 当接收到新聊天消息时触发的事件
    /// UI组件可以订阅此事件来实时更新聊天界面显示
    /// </summary>
    public event Action<ChatMessage> OnMessageReceived;

    /// <summary>
    /// 当成功发送聊天消息时触发的事件
    /// UI组件可以订阅此事件来更新发送状态和界面
    /// </summary>
    public event Action<ChatMessage> OnMessageSent;

    /// <summary>
    /// 发送聊天消息到网络中的其他玩家
    /// 这是玩家发送消息的主要入口点
    /// </summary>
    /// <param name="content">消息内容文本</param>
    /// <param name="type">消息类型，默认为普通消息</param>
    /// <remarks>
    /// <para>
    /// 该方法会执行以下操作流程：
    /// 1. 创建聊天消息对象（包含发送者信息和时间戳）
    /// 2. 将消息对象序列化为JSON格式进行网络传输
    /// 3. 通过网络客户端将消息发送给所有连接的玩家
    /// 4. 将消息添加到本地聊天历史记录
    /// 5. 触发OnMessageSent事件通知UI更新
    /// 6. 记录操作日志用于调试和监控
    /// </para>
    ///
    /// <para>
    /// TODO: 待完善的功能：
    /// - 集成真实的玩家信息获取机制，替换当前的占位符
    /// - 添加消息发送失败的回调处理
    /// - 支持消息格式验证和长度限制
    /// </para>
    /// </remarks>
    public void SendMessage(string content, ChatMessageType type = ChatMessageType.Normal)
    {
        // 参数验证：确保消息内容不为空
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("[Chat] 尝试发送空消息，操作已取消");
            return;
        }

        // TODO: 获取当前玩家真实信息
        // 需要与游戏系统的玩家管理模块集成
        // string playerId = GetLocalPlayerId();
        // string username = GetLocalPlayerName();

        // 临时使用占位符信息创建消息对象
        ChatMessage message = new ChatMessage("local_player_id", "玩家", content, type);

        try
        {
            // 将消息对象序列化为JSON字符串
            string json = JsonSerializer.Serialize(message);

            // 通过网络客户端发送消息
            _networkClient.SendRequest("ChatMessage", json);

            // 添加消息到本地历史记录
            AddToHistory(message);

            // 触发消息发送事件，通知UI更新
            OnMessageSent?.Invoke(message);

            // 记录成功发送的日志
            _logger.LogInfo($"[Chat] 消息发送成功: {content} (类型: {type})");
        }
        catch (Exception ex)
        {
            // 记录发送失败的错误日志
            _logger.LogError($"[Chat] 消息发送失败: {ex.Message}\n{ex.StackTrace}");
        }
    } // 发送聊天消息到网络，序列化后通过网络客户端传输并添加到历史记录

    /// <summary>
    /// 接收并处理来自网络的聊天消息
    /// 当网络层接收到其他玩家的聊天消息时调用此方法
    /// </summary>
    /// <param name="jsonData">JSON格式的消息数据字符串</param>
    /// <remarks>
    /// <para>
    /// 该方法会执行以下处理流程：
    /// 1. 将JSON数据反序列化为ChatMessage对象
    /// 2. 验证消息对象的有效性和完整性
    /// 3. 将验证通过的消息添加到本地历史记录
    /// 4. 触发OnMessageReceived事件通知UI更新
    /// 5. 在游戏UI中显示接收到的消息
    /// 6. 记录接收日志用于监控和调试
    /// </para>
    ///
    /// <para>
    /// TODO: 待完善的功能：
    /// - 集成游戏UI显示功能，在游戏界面中显示消息
    /// - 添加消息验证逻辑，防止恶意或格式错误的消息
    /// - 支持消息过滤和屏蔽功能
    /// - 添加消息重复检测机制
    /// </para>
    /// </remarks>
    public void ReceiveMessage(string jsonData)
    {
        // 参数验证：确保JSON数据不为空
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogWarning("[Chat] 接收到空的JSON数据，忽略处理");
            return;
        }

        try
        {
            // 将JSON数据反序列化为ChatMessage对象
            var message = JsonSerializer.Deserialize<ChatMessage>(jsonData);

            // 验证反序列化结果
            if (message == null)
            {
                _logger.LogWarning("[Chat] JSON反序列化失败，无法解析消息对象");
                return;
            }

            // 验证消息内容的完整性
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                _logger.LogWarning("[Chat] 接收到空内容的消息，已忽略");
                return;
            }

            // 添加消息到本地历史记录
            AddToHistory(message);

            // 触发消息接收事件，通知UI更新
            OnMessageReceived?.Invoke(message);

            // TODO: 在游戏UI中显示消息
            // DisplayMessageInUI(message);

            // 记录消息接收日志
            _logger.LogInfo($"[Chat] 接收到消息 - 发送者: {message.Username}, 内容: {message.Content}, 类型: {message.Type}");
        }
        catch (JsonException ex)
        {
            // JSON解析异常处理
            _logger.LogError($"[Chat] JSON解析错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            // 其他异常处理
            _logger.LogError($"[Chat] 消息接收处理异常: {ex.Message}\n{ex.StackTrace}");
        }
    } // 接收并反序列化网络聊天消息，添加到历史记录并触发事件通知

    /// <summary>
    /// 发送系统消息
    /// 用于发送系统级别的通知消息，如连接状态、错误提示等
    /// </summary>
    /// <param name="content">系统消息内容文本</param>
    /// <remarks>
    /// <para>
    /// 系统消息的特点：
    /// - 自动设置为ChatMessageType.System类型
    /// - 通常使用特殊的样式在UI中显示
    /// - 用于显示重要的系统状态信息
    /// - 包括：玩家连接/断开、游戏状态变更、错误提示等
    /// </para>
    ///
    /// <para>
    /// 使用场景示例：
    /// - "玩家XXX加入了游戏"
    /// - "与服务器连接已断开"
    /// - "游戏开始前请等待其他玩家"
    /// </para>
    /// </remarks>
    public void SendSystemMessage(string content)
    {
        // 调用基础发送方法，指定系统消息类型
        SendMessage(content, ChatMessageType.System);
    } // 发送系统消息，用于显示重要的游戏状态和通知信息

    /// <summary>
    /// 发送动作消息
    /// 用于通知其他玩家游戏内的重要操作和动作
    /// </summary>
    /// <param name="actionDescription">动作描述文本，说明具体的操作内容</param>
    /// <remarks>
    /// <para>
    /// 动作消息用于同步玩家的游戏操作，让其他玩家了解当前的游戏进展：
    /// - 玩家使用了某张卡牌 - "使用了攻击牌：弹幕"
    /// - 玩家购买了道具 - "购买了血瓶"
    /// - 玩家进入了新区域 - "进入了战斗区域"
    /// - 玩家完成了某个事件 - "获得了宝物：魔法灯笼"
    /// </para>
    ///
    /// <para>
    /// 这些消息有助于：
    /// 1. 增强游戏的社交体验
    /// 2. 让其他玩家了解游戏进度
    /// 3. 提供实时的游戏状态同步
    /// 4. 增加游戏的趣味性和互动性
    /// </para>
    /// </remarks>
    public void SendActionMessage(string actionDescription)
    {
        // 调用基础发送方法，指定动作消息类型
        SendMessage(actionDescription, ChatMessageType.Action);
    } // 发送动作消息，通知其他玩家的游戏内重要操作和行动

    /// <summary>
    /// 获取聊天历史记录的只读副本
    /// 提供对聊天历史的安全访问，防止外部代码修改历史记录
    /// </summary>
    /// <returns>包含所有聊天消息的只读列表，按时间顺序排列</returns>
    /// <remarks>
    /// <para>
    /// 返回特性：
    /// - 返回的是只读列表，外部代码无法修改历史记录
    /// - 历史记录按时间顺序排列，最早的消息在列表开头
    /// - 最新的消息在列表末尾
    /// - 包含所有类型的消息（普通、系统、动作等）
    /// </para>
    ///
    /// <para>
    /// 使用场景：
    /// - UI组件显示聊天历史
    /// - 查询特定时间段的聊天记录
    /// - 导出聊天记录用于分析
    /// - 搜索特定的聊天内容
    /// </para>
    /// </remarks>
    public IReadOnlyList<ChatMessage> GetChatHistory()
    {
        // 返回历史记录的只读包装，确保外部无法修改原数据
        return _chatHistory.AsReadOnly();
    } // 获取聊天历史记录的只读副本，确保外部无法修改内部数据

    /// <summary>
    /// 清空聊天历史记录
    /// 删除所有已存储的聊天消息，释放内存空间
    /// </summary>
    /// <remarks>
    /// <para>
    /// 清空操作的特性：
    /// - 此操作不可逆，清空后所有历史消息将被永久删除
    /// - 不会影响其他玩家的聊天历史记录
    /// - 新收到的消息会重新开始记录
    /// </para>
    ///
    /// <para>
    /// 典型调用场景：
    /// - 游戏重新开始时
    /// - 断开连接重新加入时
    /// - 用户手动清空聊天记录
    /// - 内存占用过高需要清理时
    /// </para>
    /// </remarks>
    public void ClearHistory()
    {
        // 记录清空操作前的历史记录数量
        int count = _chatHistory.Count;

        // 清空历史记录列表
        _chatHistory.Clear();

        // 记录清空操作的日志
        _logger.LogInfo($"[Chat] 聊天历史记录已清空，删除了 {count} 条消息");
    }

    /// <summary>
    /// 将消息添加到聊天历史记录中
    /// 内部方法，用于维护聊天历史的一致性
    /// </summary>
    /// <param name="message">要添加的聊天消息对象</param>
    /// <remarks>
    /// <para>
    /// 添加逻辑：
    /// 1. 将新消息添加到历史记录列表末尾
    /// 2. 检查历史记录是否超过最大限制
    /// 3. 如果超过限制，删除最旧的消息（FIFO原则）
    /// 4. 保持历史记录的大小在合理范围内
    /// </para>
    ///
    /// <para>
    /// FIFO（先进先出）策略：
    /// - 最先添加的消息最先被删除
    /// - 确保历史记录始终包含最新的消息
    /// - 防止内存无限增长
    /// </para>
    /// </remarks>
    private void AddToHistory(ChatMessage message)
    {
        // 添加新消息到历史记录末尾
        _chatHistory.Add(message);

        // 检查历史记录是否超过最大限制
        if (_chatHistory.Count > MaxHistorySize)
        {
            // 删除最旧的消息（索引为0的元素）
            _chatHistory.RemoveAt(0);

            // 记录历史记录清理日志
            _logger.LogDebug($"[Chat] 聊天历史记录已达到最大限制，已删除最旧的消息");
        }
    }

    // ========================================
    // TODO: 待实现的功能方法
    // ========================================

    #region TODO 功能实现
    // 以下方法待实现，用于完善聊天系统的完整功能

    /// <summary>
    /// 获取本地玩家的唯一标识符
    /// 从游戏系统获取当前玩家的唯一ID
    /// </summary>
    /// <returns>玩家ID字符串，如"player_001"</returns>
    /// <remarks>
    /// TODO: 需要与游戏系统的玩家管理模块集成
    /// 可能的实现方式：
    /// - 从GameStateUtils获取玩家ID
    /// - 从网络客户端获取本地玩家标识
    /// - 从游戏会话管理器获取玩家信息
    /// </remarks>
    // private string GetLocalPlayerId()
    // {
    //     // 实现逻辑：从游戏系统获取玩家ID
    //     return "local_player_id"; // 临时返回值
    // }

    /// <summary>
    /// 获取本地玩家的显示名称
    /// 从游戏系统获取当前玩家的显示名称
    /// </summary>
    /// <returns>玩家名称字符串，如"玩家小明"</returns>
    /// <remarks>
    /// TODO: 需要与游戏系统的玩家配置模块集成
    /// 可能的实现方式：
    /// - 从玩家配置文件读取显示名称
    /// - 从游戏角色系统获取角色名称
    /// - 使用用户自定义的昵称
    /// </remarks>
    // private string GetLocalPlayerName()
    // {
    //     // 实现逻辑：从游戏系统获取玩家名称
    //     return "玩家"; // 临时返回值
    // }

    /// <summary>
    /// 在游戏UI中显示聊天消息
    /// 将接收到的消息显示在游戏界面的聊天窗口中
    /// </summary>
    /// <param name="message">要显示的聊天消息对象</param>
    /// <remarks>
    /// TODO: 需要与游戏UI系统集成
    /// 功能要求：
    /// - 支持不同类型消息的样式显示
    /// - 支持消息的自动滚动和分页
    /// - 支持消息的时间戳显示
    /// - 支持消息的颜色和字体样式
    /// - 支持消息的过滤和搜索
    /// </remarks>
    // private void DisplayMessageInUI(ChatMessage message)
    // {
    //     // 实现逻辑：将消息添加到游戏UI的聊天窗口
    //     // 需要调用游戏UI系统的相关API
    // }

    #endregion
}