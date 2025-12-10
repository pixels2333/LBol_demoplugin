using System.Collections.Generic;

namespace NetworkPlugin.Network.Messages
{
    /// <summary>
    /// 消息优先级配置
    /// </summary>
    public static class MessagePriorities
    {
        private static readonly Dictionary<string, MessagePriority> _priorities = new()
        {
            // 系统消息 - 高优先级
            [NetworkMessageTypes.Heartbeat] = MessagePriority.High,
            [NetworkMessageTypes.HeartbeatResponse] = MessagePriority.High,
            [NetworkMessageTypes.PlayerJoined] = MessagePriority.High,
            [NetworkMessageTypes.PlayerLeft] = MessagePriority.High,
            [NetworkMessageTypes.HostChanged] = MessagePriority.High,

            // 游戏同步消息 - 正常优先级
            [NetworkMessageTypes.OnCardPlayStart] = MessagePriority.Normal,
            [NetworkMessageTypes.OnCardPlayComplete] = MessagePriority.Normal,
            [NetworkMessageTypes.ManaConsumeStarted] = MessagePriority.Normal,
            [NetworkMessageTypes.ManaConsumeCompleted] = MessagePriority.Normal,

            // 状态管理消息 - 高优先级
            [NetworkMessageTypes.StateSyncRequest] = MessagePriority.High,
            [NetworkMessageTypes.StateSyncResponse] = MessagePriority.High,
            [NetworkMessageTypes.FullStateSyncRequest] = MessagePriority.Critical,
            [NetworkMessageTypes.FullStateSyncResponse] = MessagePriority.Critical,

            // 聊天消息 - 低优先级
            [NetworkMessageTypes.ChatMessage] = MessagePriority.Low
        };

        /// <summary>
        /// 获取消息优先级
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <returns>消息优先级</returns>
        public static MessagePriority GetPriority(string messageType)
        {
            // 尝试从优先级字典中查找指定消息类型的优先级
            return _priorities.TryGetValue(messageType, out var priority)
                ? priority  // 如果找到则返回对应的优先级
                : MessagePriority.Normal; // 如果未找到则返回默认的正常优先级
        }
    }
}