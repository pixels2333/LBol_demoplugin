using System;
using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;
using BepInEx.Logging;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天控制台 - 管理聊天消息的收发和显示
/// 参考: 杀戮尖塔Together in Spire的聊天系统
/// TODO: 实现完整的聊天UI集成
/// </summary>
public class ChatConsole
{
    private readonly INetworkClient _networkClient;
    private readonly ManualLogSource _logger;

    // 聊天记录
    private readonly List<ChatMessage> _chatHistory;
    private const int MaxHistorySize = 100;

    // 事件
    public event Action<ChatMessage> OnMessageReceived;
    public event Action<ChatMessage> OnMessageSent;

    public ChatConsole(INetworkClient networkClient, ManualLogSource logger)
    {
        _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chatHistory = new List<ChatMessage>();
    }

    /// <summary>
    /// 发送聊天消息
    /// </summary>
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
    /// 接收并处理聊天消息
    /// </summary>
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
    public void SendSystemMessage(string content)
    {
        SendMessage(content, ChatMessageType.System);
    }

    /// <summary>
    /// 发送动作消息（如：玩家使用了某卡牌）
    /// </summary>
    public void SendActionMessage(string actionDescription)
    {
        SendMessage(actionDescription, ChatMessageType.Action);
    }

    /// <summary>
    /// 获取聊天记录
    /// </summary>
    public IReadOnlyList<ChatMessage> GetChatHistory()
    {
        return _chatHistory.AsReadOnly();
    }

    /// <summary>
    /// 清空聊天记录
    /// </summary>
    public void ClearHistory()
    {
        _chatHistory.Clear();
    }

    private void AddToHistory(ChatMessage message)
    {
        _chatHistory.Add(message);

        // 保持历史记录大小
        if (_chatHistory.Count > MaxHistorySize)
        {
            _chatHistory.RemoveAt(0);
        }
    }

    // TODO: 实现以下方法
    // private string GetLocalPlayerId()
    // private string GetLocalPlayerName()
    // private void DisplayMessageInUI(ChatMessage message)
}
