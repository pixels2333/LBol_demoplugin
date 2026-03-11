using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NetworkPlugin.Utils;
using BepInEx.Logging;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天控制台类，管理LBoL联机MOD中玩家间聊天功能。
/// </summary>
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
    /// 单条消息最大长度（字符）
    /// </summary>
    private const int MaxMessageLength = 500;

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
    /// 发送聊天消息到网络中的其他玩家。
    /// </summary>
    public void SendMessage(string content, ChatMessageType type = ChatMessageType.Normal)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("[Chat] 尝试发送空消息，操作已取消");
            return;
        }

        if (content.Length > MaxMessageLength)
        {
            _logger.LogWarning($"[Chat] 消息过长，将被截断: len={content.Length}");
            content = content.Substring(0, MaxMessageLength);
        }

        string playerId = GetLocalPlayerId();
        string playerName = GetLocalPlayerName();

        ChatMessage message = new ChatMessage(playerId, playerName, content, type);

        try
        {
            string json = JsonCompat.Serialize(message);
            _networkClient.SendRequest(NetworkMessageTypes.ChatMessage, json);
            AddToHistory(message);
            OnMessageSent?.Invoke(message);
            _logger.LogInfo($"[Chat] 消息发送成功: {content} (类型: {type})");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Chat] 消息发送失败: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static string GetLocalPlayerId()
    {
        try
        {
            string id = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            string id = GameStateUtils.GetCurrentPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }
        catch
        {
            // ignore
        }

        return "local_player_id";
    }

    private static string GetLocalPlayerName()
    {
        try
        {
            string playerName = GameStateUtils.GetCurrentPlayerName();
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                return playerName;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            string id = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }
        catch
        {
            // ignore
        }

        return "玩家";
    }

    /// <summary>
    /// 接收并处理来自网络的聊天消息。
    /// </summary>
    public void ReceiveMessage(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogWarning("[Chat] 接收到空的JSON数据，忽略处理");
            return;
        }

        try
        {
            var message = JsonSerializer.Deserialize<ChatMessage>(jsonData);

            if (message == null)
            {
                _logger.LogWarning("[Chat] JSON反序列化失败，无法解析消息对象");
                return;
            }

            if (string.IsNullOrWhiteSpace(message.MessageId))
            {
                message.MessageId = Guid.NewGuid().ToString();
            }

            if (message.Timestamp == default)
            {
                message.Timestamp = DateTime.UtcNow;
            }

            if (string.IsNullOrWhiteSpace(message.Content))
            {
                _logger.LogWarning("[Chat] 接收到空内容的消息，已忽略");
                return;
            }

            if (message.Content.Length > MaxMessageLength)
            {
                message.Content = message.Content.Substring(0, MaxMessageLength);
            }

            message.PlayerName = message.GetDisplayPlayerName();

            AddToHistory(message);

            OnMessageReceived?.Invoke(message);

            // 备注：在游戏 UI 中显示消息需与 UI 系统集成
            // DisplayMessageInUI(message);

            _logger.LogInfo($"[Chat] 接收到消息 - 发送者: {message.GetDisplayPlayerName()}, 内容: {message.Content}, 类型: {message.MessageType}");
        }
        catch (JsonException ex)
        {
            _logger.LogError($"[Chat] JSON解析错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Chat] 消息接收处理异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 发送系统消息。
    /// </summary>
    public void SendSystemMessage(string content)
    {
        SendMessage(content, ChatMessageType.System);
    }

    /// <summary>
    /// 发送动作消息。
    /// </summary>
    public void SendActionMessage(string actionDescription)
    {
        SendMessage(actionDescription, ChatMessageType.Action);
    }

    /// <summary>
    /// 获取聊天历史记录的只读副本。
    /// </summary>
    public IReadOnlyList<ChatMessage> GetChatHistory()
    {
        return _chatHistory.AsReadOnly();
    }

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
        // 简单去重：避免重复消息刷屏（历史上限较小，线性检查足够）
        if (!string.IsNullOrWhiteSpace(message?.MessageId) &&
            _chatHistory.Any(m => string.Equals(m?.MessageId, message.MessageId, StringComparison.Ordinal)))
        {
            _logger.LogDebug($"[Chat] Duplicate message ignored: {message.MessageId}");
            return;
        }

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

    // 兼容说明：
    // - 本地玩家 ID/名称获取已在 GetLocalPlayerId()/GetLocalPlayerName() 内实现。
    // - 聊天 UI 的展示由 UI 层负责（例如 ChatUI）。
}
