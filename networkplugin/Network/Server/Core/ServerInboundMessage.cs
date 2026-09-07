using LiteNetLib;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Network.Server.Core;

public readonly struct ServerInboundMessage
{
        public NetPeer FromPeer { get; }
        public string Type { get; }
        public string JsonPayload { get; }
        public DeliveryMethod DeliveryMethod { get; }
        public MessagePriority Priority { get; }

        public ServerInboundMessage(NetPeer fromPeer, string type, string jsonPayload, DeliveryMethod deliveryMethod, MessagePriority priority)
    {
        FromPeer = fromPeer;
        Type = type;
        JsonPayload = jsonPayload;
        DeliveryMethod = deliveryMethod;
        Priority = priority;
    }
}
