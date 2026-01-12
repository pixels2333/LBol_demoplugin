// 服务器入站消息：在 PollEvents 过程中入队，再按优先级有序处理，避免直接在回调中做重逻辑。
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

