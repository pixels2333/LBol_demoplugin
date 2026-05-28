// 服务器入站消息：在 PollEvents 过程中入队，再按优先级有序处理，避免直接在回调中做重逻辑。
using LiteNetLib;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 服务器入站消息结构体
/// 在 PollEvents 过程中入队，再按优先级有序处理，避免直接在回调中执行重逻辑
/// </summary>
public readonly struct ServerInboundMessage
{
    /// <summary>发送方网络对等端</summary>
    public NetPeer FromPeer { get; }
    /// <summary>消息类型</summary>
    public string Type { get; }
    /// <summary>消息 JSON 负载</summary>
    public string JsonPayload { get; }
    /// <summary>消息投递方式（可靠/不可靠等）</summary>
    public DeliveryMethod DeliveryMethod { get; }
    /// <summary>消息优先级</summary>
    public MessagePriority Priority { get; }

    /// <summary>
    /// 初始化服务器入站消息
    /// </summary>
    /// <param name="fromPeer">发送方网络对等端</param>
    /// <param name="type">消息类型</param>
    /// <param name="jsonPayload">JSON 负载</param>
    /// <param name="deliveryMethod">投递方式</param>
    /// <param name="priority">优先级</param>
    public ServerInboundMessage(NetPeer fromPeer, string type, string jsonPayload, DeliveryMethod deliveryMethod, MessagePriority priority)
    {
        FromPeer = fromPeer;
        Type = type;
        JsonPayload = jsonPayload;
        DeliveryMethod = deliveryMethod;
        Priority = priority;
    }
}
