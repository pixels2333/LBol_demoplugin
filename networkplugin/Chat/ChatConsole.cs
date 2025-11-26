using System;
using System.Collections.Generic;
using System.Text.Json;
using BepInEx.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天控制台 - 管理聊天消息的收发、历史记录和事件处理
/// </summary>
/// <remarks>
/// 该类负责处理网络聊天功能，包括：
/// - 发送和接收聊天消息
/// - 管理聊天历史记录
/// - 提供聊天事件通知
/// - 支持多种消息类型（普通、系统、动作等）
///
/// 设计参考: 杀戮尖塔 Together in Spire 的聊天系统
/// </remarks>
public class ChatConsole
{
    /// <summary>
    /// 网络客户端接口，用于发送聊天消息
    /// </summary>
    private readonly INetworkClient _networkClient;

    /// <summary>
    /// 日志记录器，用于记录聊天相关的操作和错误
    /// </summary>
    private readonly ManualLogSource _logger;

    /// <summary>
    /// 聊天历史记录列表，按时间顺序存储所有消息
    /// </summary>
    private readonly List<ChatMessage> _chatHistory;

    /// <summary>
    /// 聊天历史记录的最大条数，超过此数量将删除最旧的消息
    /// </summary>
    private const int MaxHistorySize = 100;

    /// <summary>
    /// 当接收到新聊天消息时触发的事件
    /// </summary>
    public event Action<ChatMessage> OnMessageReceived;

    /// <summary>
    /// 当成功发送聊天消息时触发的事件
    /// </summary>
    public event Action<ChatMessage> OnMessageSent;

    /// <summary>
    /// 初始化聊天控制台实例
    /// </summary>
    /// <param name="networkClient">网络客户端接口，用于发送消息</param>
    /// <param name="logger">日志记录器，用于记录操作日志</param>
    /// <exception cref="ArgumentNullException">当任一参数为 null 时抛出</exception>
    public ChatConsole(INetworkClient networkClient, ManualLogSource logger)
    {
        _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chatHistory = [];
    }

    /// <summary>
    /// 发送聊天消息到网络中的其他玩家
    /// </summary>
    /// <param name="content">消息内容</param>
    /// <param name="type">消息类型，默认为普通消息</param>
    /// <remarks>
    /// 该方法会：
    /// 1. 创建聊天消息对象（当前使用占位符玩家ID和用户名）
    /// 2. 将消息序列化为JSON格式
    /// 3. 通过网络客户端发送消息
    /// 4. 将消息添加到本地历史记录
    /// 5. 触发OnMessageSent事件
    ///
    /// TODO: 集成真实的玩家信息获取机制
    /// </remarks>
    public void SendMessage(string content, ChatMessageType type = ChatMessageType.Normal)
    {
        // TODO: 获取当前玩家信息
        // string playerId = GetLocalPlayerId();
        // string username = GetLocalPlayerName();

        var message = new ChatMessage("local_player_id", "Player", content, type);

        try
        {
            var json = JsonSerializer.Serialize(message);
            _networkClient.SendRequest("ChatMessage", json);

            AddToHistory(message);
            OnMessageSent?.Invoke(message);

            _logger.LogInfo($"[Chat] Sent message: {content}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Chat] Failed to send message: {ex.Message}");
        }
    }

    /// <summary>
    /// 接收并处理来自网络的聊天消息
    /// </summary>
    /// <param name="jsonData">JSON格式的消息数据</param>
    /// <remarks>
    /// 该方法会：
    /// 1. 将JSON数据反序列化为ChatMessage对象
    /// 2. 验证消息的有效性
    /// 3. 将消息添加到本地历史记录
    /// 4. 触发OnMessageReceived事件
    /// 5. 记录接收日志
    ///
    /// TODO: 集成游戏UI显示功能
    /// </remarks>
    public void ReceiveMessage(string jsonData)
    {
        try
        {
            var message = JsonSerializer.Deserialize<ChatMessage>(jsonData);
            if (message == null)
            {
                _logger.LogWarning("[Chat] Failed to deserialize message");
                return;
            }

            AddToHistory(message);
            OnMessageReceived?.Invoke(message);

            // TODO: 显示消息到游戏UI
            // DisplayMessageInUI(message);

            _logger.LogInfo($"[Chat] Received message from {message.Username}: {message.Content}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Chat] Failed to receive message: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送系统消息
    /// </summary>
    /// <param name="content">系统消息内容</param>
    /// <remarks>
    /// 系统消息用于显示游戏状态、连接状态等重要信息
    /// 会自动设置为ChatMessageType.System类型
    /// </remarks>
    public void SendSystemMessage(string content)
    {
        SendMessage(content, ChatMessageType.System);
    }

    /// <summary>
    /// 发送动作消息
    /// </summary>
    /// <param name="actionDescription">动作描述文本</param>
    /// <remarks>
    /// 动作消息用于通知其他玩家游戏内的重要操作，如：
    /// - 玩家使用了某张卡牌
    /// - 玩家购买了道具
    /// - 玩家进入了新区域
    /// 会自动设置为ChatMessageType.Action类型
    /// </remarks>
    public void SendActionMessage(string actionDescription)
    {
        SendMessage(actionDescription, ChatMessageType.Action);
    }

    /// <summary>
    /// 获取聊天历史记录的只读副本
    /// </summary>
    /// <returns>包含所有聊天消息的只读列表</returns>
    /// <remarks>
    /// 返回的是只读列表，外部代码无法修改历史记录
    /// 历史记录按时间顺序排列，最新的消息在列表末尾
    /// </remarks>
    public IReadOnlyList<ChatMessage> GetChatHistory()
    {
        return _chatHistory.AsReadOnly();
    }

    /// <summary>
    /// 清空聊天历史记录
    /// </summary>
    /// <remarks>
    /// 此操作不可逆，清空后所有历史消息将被永久删除
    /// 通常在游戏重新开始或断开连接时调用
    /// </remarks>
    public void ClearHistory()
    {
        _chatHistory.Clear();
    }

    /// <summary>
    /// 将消息添加到聊天历史记录中
    /// </summary>
    /// <param name="message">要添加的聊天消息</param>
    /// <remarks>
    /// 如果历史记录超过最大限制，会自动删除最旧的消息
    /// 保持FIFO（先进先出）原则
    /// </remarks>
    private void AddToHistory(ChatMessage message)
    {
        _chatHistory.Add(message);

        // 保持历史记录大小不超过最大限制
        if (_chatHistory.Count > MaxHistorySize)
        {
            _chatHistory.RemoveAt(0);
        }
    }

    #region TODO 功能实现
    // 以下方法待实现，用于完善聊天系统的完整功能

    /// <summary>
    /// 获取本地玩家的唯一标识符
    /// </summary>
    /// <returns>玩家ID字符串</returns>
    /// <remarks>
    /// 需要与游戏系统的玩家管理模块集成
    /// </remarks>
    // private string GetLocalPlayerId()

    /// <summary>
    /// 获取本地玩家的显示名称
    /// </summary>
    /// <returns>玩家名称字符串</returns>
    /// <remarks>
    /// 需要与游戏系统的玩家配置模块集成
    /// </remarks>
    // private string GetLocalPlayerName()

    /// <summary>
    /// 在游戏UI中显示聊天消息
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <remarks>
    /// 需要与游戏UI系统集成，支持不同类型消息的样式显示
    /// </remarks>
    // private void DisplayMessageInUI(ChatMessage message)

    #endregion
}
