using System.Collections.Generic;

namespace NetworkPlugin.Network.Messages
{
        public static class MessagePriorities
    {
        private static readonly Dictionary<string, MessagePriority> _priorities = new()
        {

            [NetworkMessageTypes.Heartbeat] = MessagePriority.High,
            [NetworkMessageTypes.HeartbeatResponse] = MessagePriority.High,
            [NetworkMessageTypes.PlayerJoined] = MessagePriority.High,
            [NetworkMessageTypes.PlayerLeft] = MessagePriority.High,
            [NetworkMessageTypes.HostChanged] = MessagePriority.High,

            [NetworkMessageTypes.OnCardPlayStart] = MessagePriority.Normal,
            [NetworkMessageTypes.OnCardPlayComplete] = MessagePriority.Normal,
            [NetworkMessageTypes.ManaConsumeStarted] = MessagePriority.Normal,
            [NetworkMessageTypes.ManaConsumeCompleted] = MessagePriority.Normal,

            [NetworkMessageTypes.StateSyncRequest] = MessagePriority.High,
            [NetworkMessageTypes.StateSyncResponse] = MessagePriority.High,
            [NetworkMessageTypes.FullStateSyncRequest] = MessagePriority.Critical,
            [NetworkMessageTypes.FullStateSyncResponse] = MessagePriority.Critical,

            [NetworkMessageTypes.RoomStateRequest] = MessagePriority.Critical,
            [NetworkMessageTypes.RoomStateResponse] = MessagePriority.Critical,
            [NetworkMessageTypes.RoomStateUpload] = MessagePriority.High,
            [NetworkMessageTypes.RoomStateBroadcast] = MessagePriority.High,

            [NetworkMessageTypes.ChatMessage] = MessagePriority.Low
        };

                public static MessagePriority GetPriority(string messageType)
        {

            return _priorities.TryGetValue(messageType, out var priority)
                ? priority
                : MessagePriority.Normal;
        }
    }
}
