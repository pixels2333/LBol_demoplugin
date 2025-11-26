using System;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天消息数据传输对象 - 用于玩家间的文字通信系统
/// <para>
/// 该类封装了聊天消息的所有必要信息，支持消息的唯一标识、发送者信息、
/// 消息内容、时间戳和消息类型分类。设计参考了杀戮尖塔Together in Spire的聊天系统架构。
/// </para>
/// <para>
/// 所有属性都使用JsonPropertyName特性进行JSON序列化映射，确保网络传输中的兼容性。
/// </para>
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 消息唯一标识符
    /// <para>使用GUID确保每个消息都有全局唯一标识，用于消息去重和追踪</para>
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; }

    /// <summary>
    /// 发送者玩家ID
    /// <para>标识消息发送者的唯一标识，与游戏中的玩家ID对应</para>
    /// </summary>
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    /// <summary>
    /// 发送者用户名
    /// <para>显示在聊天界面中的发送者名称，用于用户识别</para>
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }

    /// <summary>
    /// 消息内容
    /// <para>实际的消息文本内容，支持UTF-8编码的字符串</para>
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// 消息发送时间戳
    /// <para>记录消息创建的精确时间，用于消息排序和历史记录</para>
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 消息类型分类
    /// <para>用于区分不同类型的聊天消息，支持UI层面的差异化显示</para>
    /// </summary>
    [JsonPropertyName("messageType")]
    public ChatMessageType MessageType { get; set; }

    /// <summary>
    /// 默认构造函数
    /// <para>创建一个新的聊天消息实例，自动生成唯一ID和时间戳，默认消息类型为Normal</para>
    /// </summary>
    public ChatMessage()
    {
        MessageId = Guid.NewGuid().ToString();
        Timestamp = DateTime.Now;
        MessageType = ChatMessageType.Normal;
    }

    /// <summary>
    /// 完整参数构造函数
    /// <para>使用指定参数创建聊天消息，调用默认构造函数初始化基础属性</para>
    /// </summary>
    /// <param name="playerId">发送者玩家ID</param>
    /// <param name="username">发送者用户名</param>
    /// <param name="content">消息内容</param>
    /// <param name="type">消息类型，默认为Normal</param>
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
/// 聊天消息类型枚举
/// <para>定义聊天系统中支持的所有消息类型，用于UI显示和行为控制</para>
/// </summary>
public enum ChatMessageType
{
    /// <summary>
    /// 普通聊天消息
    /// <para>玩家间的一般对话内容，使用标准样式显示</para>
    /// </summary>
    Normal,

    /// <summary>
    /// 系统消息
    /// <para>由系统自动生成的消息，如玩家加入/离开提示等</para>
    /// </summary>
    System,

    /// <summary>
    /// 错误消息
    /// <para>用于显示错误和警告信息，通常使用红色或警告样式</para>
    /// </summary>
    Error,

    /// <summary>
    /// 私聊消息
    /// <para>玩家间的私人对话，仅对发送者和接收者可见</para>
    /// </summary>
    Whispper,

    /// <summary>
    /// 动作消息
    /// <para>记录玩家游戏操作的消息，如抽牌、使用卡牌等游戏行为</para>
    /// </summary>
    Action
}
