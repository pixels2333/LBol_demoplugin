using System;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Chat;

public class ChatMessage
{
        [JsonPropertyName("messageId")]
    public string MessageId { get; set; }

        [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

        [JsonPropertyName("playerName")]
    public string PlayerName { get; set; }

    [JsonPropertyName("username")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string LegacyUsername
    {
        get => null;
        set => ApplyLegacyPlayerName(value);
    }

    [JsonPropertyName("UserName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string LegacyUsernameUpper
    {
        get => null;
        set => ApplyLegacyPlayerName(value);
    }

        [JsonPropertyName("content")]
    public string Content { get; set; }

        [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

        [JsonPropertyName("messageType")]
    public ChatMessageType MessageType { get; set; }

        public ChatMessage()
    {

        MessageId = Guid.NewGuid().ToString();

        Timestamp = DateTime.UtcNow;

        MessageType = ChatMessageType.Normal;
    }

        public ChatMessage(string playerId, string playerName, string content, ChatMessageType type = ChatMessageType.Normal)
        : this()
    {

        PlayerId = playerId;
        PlayerName = playerName;

        Content = content;

        MessageType = type;
    }

        public string GetFormattedTime()
    {
        return Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string GetDisplayPlayerName()
    {
        if (!string.IsNullOrWhiteSpace(PlayerName))
        {
            return PlayerName;
        }

        if (!string.IsNullOrWhiteSpace(PlayerId))
        {
            return PlayerId;
        }

        return "玩家";
    }

        public string GetShortDescription()
    {
        string shortContent = Content.Length > 50
            ? Content.Substring(0, 50) + "..."
            : Content;

        return $"[{MessageType}] {GetDisplayPlayerName()}: {shortContent}";
    }

    private void ApplyLegacyPlayerName(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(PlayerName))
        {
            PlayerName = value;
        }
    }
}

public enum ChatMessageType
{
        Normal,

        System,

        Error,

        Whisper,

        Action,

        Battle
}
