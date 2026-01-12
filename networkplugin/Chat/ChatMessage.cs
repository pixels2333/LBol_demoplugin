using System;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Chat;

/// <summary>
/// 聊天消息数据传输对象
/// 用于LBoL联机MOD中玩家间的文字通信系统，封装聊天消息的所有必要信息
/// </summary>
/// <remarks>
/// <para>
/// 该类封装了聊天消息的所有必要信息，支持：
/// - 消息的唯一标识 - 使用GUID确保全局唯一性
/// - 发送者信息 - 玩家ID和显示名称
/// - 消息内容 - 支持UTF-8编码的文本内容
/// - 时间戳 - 用于消息排序和历史记录
/// - 消息类型分类 - 支持UI层面的差异化显示
/// </para>
///
/// <para>
/// 设计特点：
/// - 所有属性都使用JsonPropertyName特性进行JSON序列化映射
/// - 确保网络传输中的兼容性和数据完整性
/// - 支持自动生成唯一ID和时间戳
/// - 提供便捷的构造函数重载
/// </para>
///
/// <para>
/// 设计参考: 杀戮尖塔Together in Spire的聊天系统架构
/// 适配了LBoL游戏的消息传输需求
/// </para>
/// </remarks>
public class ChatMessage
{
    /// <summary>
    /// 消息唯一标识符
    /// 使用GUID确保每个消息都有全局唯一标识，用于消息去重、追踪和引用
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; }

    /// <summary>
    /// 发送者玩家ID
    /// 标识消息发送者的唯一标识，与游戏中的玩家ID对应
    /// 用于区分不同玩家发送的消息
    /// </summary>
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    /// <summary>
    /// 发送者用户名
    /// 显示在聊天界面中的发送者名称，用于用户识别和社交体验
    /// 可以是游戏角色名或自定义昵称
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }

    /// <summary>
    /// 消息内容
    /// 实际的消息文本内容，支持UTF-8编码的字符串
    /// 包含玩家想要传达的文字信息
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// 消息发送时间戳
    /// 记录消息创建的精确时间，用于消息排序、历史记录和时间显示
    /// 使用UTC时间确保不同时区玩家的时间一致性
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 消息类型分类
    /// 用于区分不同类型的聊天消息，支持UI层面的差异化显示和行为控制
    /// 不同类型可能使用不同的颜色、字体或显示样式
    /// </summary>
    [JsonPropertyName("messageType")]
    public ChatMessageType MessageType { get; set; }

    /// <summary>
    /// 默认构造函数
    /// 创建一个新的聊天消息实例，自动生成唯一ID和时间戳
    /// </summary>
    /// <remarks>
    /// 初始化逻辑：
    /// - 自动生成GUID作为消息唯一标识
    /// - 设置当前时间作为消息时间戳
    /// - 默认消息类型设置为Normal（普通消息）
    /// </remarks>
    public ChatMessage()
    {
        // 生成全局唯一的消息标识符
        MessageId = Guid.NewGuid().ToString();

        // 设置消息创建时间为当前UTC时间
        Timestamp = DateTime.UtcNow;

        // 默认消息类型为普通聊天消息
        MessageType = ChatMessageType.Normal;
    }

    /// <summary>
    /// 完整参数构造函数
    /// 使用指定参数创建聊天消息，自动调用默认构造函数初始化基础属性
    /// </summary>
    /// <param name="playerId">发送者玩家的唯一标识符</param>
    /// <param name="username">发送者的显示名称</param>
    /// <param name="content">消息的文本内容</param>
    /// <param name="type">消息类型，默认为Normal普通消息</param>
    /// <remarks>
    /// 构造流程：
    /// 1. 调用默认构造函数初始化MessageId、Timestamp等基础属性
    /// 2. 设置发送者信息和消息内容
    /// 3. 根据参数设置消息类型
    /// </remarks>
    public ChatMessage(string playerId, string username, string content, ChatMessageType type = ChatMessageType.Normal)
        : this()
    {
        // 设置发送者信息
        PlayerId = playerId;
        Username = username;

        // 设置消息内容
        Content = content;

        // 设置消息类型
        MessageType = type;
    }

    /// <summary>
    /// 获取消息的本地化时间字符串
    /// 返回适合本地显示的时间格式字符串
    /// </summary>
    /// <returns>格式化的本地时间字符串，如"2024-01-01 12:30:45"</returns>
    /// <remarks>
    /// 可以用于UI中的时间显示，根据用户时区自动转换
    /// </remarks>
    public string GetFormattedTime()
    {
        return Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 获取消息的简短描述
    /// 用于日志记录或调试时快速识别消息内容
    /// </summary>
    /// <returns>包含发送者、类型和部分内容的描述字符串</returns>
    /// <remarks>
    /// 如果消息内容过长，会自动截取前50个字符
    /// </remarks>
    public string GetShortDescription()
    {
        string shortContent = Content.Length > 50
            ? Content.Substring(0, 50) + "..."
            : Content;

        return $"[{MessageType}] {Username}: {shortContent}";
    }
}

/// <summary>
/// 聊天消息类型枚举
/// 定义LBoL联机MOD聊天系统中支持的所有消息类型，用于UI显示和行为控制
/// </summary>
/// <remarks>
/// <para>
/// 消息类型的作用：
/// - UI显示差异：不同类型使用不同的颜色、字体或图标
/// - 行为控制：某些类型可能有特殊的行为，如错误消息需要特殊处理
/// - 过滤功能：玩家可以根据消息类型过滤显示内容
/// - 权限控制：某些消息类型可能有发送权限限制
/// </para>
///
/// <para>
/// 扩展性考虑：
/// - 枚举设计便于后续添加新的消息类型
/// - 每个类型都有明确的用途和使用场景
/// - 支持版本兼容性的消息类型扩展
/// </para>
/// </remarks>
public enum ChatMessageType
{
    /// <summary>
    /// 普通聊天消息
    /// 玩家间的一般对话内容，使用标准样式显示
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 玩家间的日常交流
    /// - 游戏策略讨论
    /// - 社交互动和问候
    /// UI建议：使用默认的文字颜色和样式
    /// </remarks>
    Normal,

    /// <summary>
    /// 系统消息
    /// 由系统自动生成的消息，如玩家加入/离开提示等
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 玩家连接/断开通知："玩家XXX加入了游戏"
    /// - 游戏状态变更："游戏开始前请等待其他玩家"
    /// - 网络状态提示："与服务器连接已断开"
    /// UI建议：使用蓝色或灰色，可能带有系统图标
    /// </remarks>
    System,

    /// <summary>
    /// 错误消息
    /// 用于显示错误和警告信息，通常使用红色或警告样式
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 消息发送失败："消息发送失败，请重试"
    /// - 网络连接错误："网络连接不稳定"
    /// - 系统异常："聊天系统暂时不可用"
    /// UI建议：使用红色文字和警告图标，可能带有特殊效果
    /// </remarks>
    Error,

    /// <summary>
    /// 私聊消息
    /// 玩家间的私人对话，仅对发送者和接收者可见
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 玩家间的私下交流
    /// - 敏感信息的分享
    /// - 不想公开讨论的话题
    /// UI建议：使用紫色或特殊标记，显示私聊图标
    /// TODO: 需要实现私聊功能，包括接收者指定和权限控制
    /// </remarks>
    Whisper,

    /// <summary>
    /// 动作消息
    /// 记录玩家游戏操作的消息，如抽牌、使用卡牌等游戏行为
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 卡牌操作："使用了攻击牌：弹幕"
    /// - 道具使用："购买了血瓶"
    /// - 地图移动："进入了新的战斗区域"
    /// - 事件完成："获得了宝物：魔法灯笼"
    /// UI建议：使用绿色或橙色，可能带有动作图标
    /// </remarks>
    Action,

    /// <summary>
    /// 战斗消息
    /// 战斗中的重要事件通知，如战斗开始、结束等
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 战斗开始："进入了战斗"
    /// - 战斗结束："战斗胜利/失败"
    /// - 重要战斗事件："Boss战开始"
    /// UI建议：使用红色或橙色，可能带有战斗相关的图标
    /// 预留：为将来的功能预留，当前可能不会使用
    /// </remarks>
    Battle
}
