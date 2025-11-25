using System;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天消息类 - 用于玩家间文字通信
/// 参考: 杀戮尖塔Together in Spire的聊天系统
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; }

    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("messageType")]
    public ChatMessageType MessageType { get; set; }

    public ChatMessage()
    {
        MessageId = Guid.NewGuid().ToString();
        Timestamp = DateTime.Now;
        MessageType = ChatMessageType.Normal;
    }

    public ChatMessage(string playerId, string username, string content, ChatMessageType type = ChatMessageType.Normal)
        : this()
    {
        PlayerId = playerId;
        Username = username;
        Content = content;
        MessageType = type;
    }
}

/// <summary>
/// 聊天消息类型
/// </summary>
public enum ChatMessageType
{
    Normal,      // 普通消息
    System,      // 系统消息
    Error,       // 错误消息
    Whispper,    // 私聊消息
    Action       // 动作消息（如：玩家进行了某操作）
}
