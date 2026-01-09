// 服务器侧连接封装：为房间广播/定向发送提供统一的 NetPeer 发送能力。
using System;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Network.Room;

namespace NetworkPlugin.Network.Messages;

public class NetworkConnection
{
    public string CurrentRoomId { get; internal set; }
    public string PlayerId { get; internal set; }
    public NetPeer Peer { get; internal set; }

    private readonly Action<NetPeer, string, string, DeliveryMethod>? _sendRaw;

    public NetworkConnection(NetPeer peer, string playerId, string currentRoomId = "", Action<NetPeer, string, string, DeliveryMethod>? sendRaw = null)
    {
        Peer = peer ?? throw new ArgumentNullException(nameof(peer));
        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        CurrentRoomId = currentRoomId ?? string.Empty;
        _sendRaw = sendRaw;
    }

    public void SendMessage(NetworkMessage message, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        if (message == null)
        {
            return;
        }

        string payloadJson = message.Payload switch
        {
            null => string.Empty,
            string s => s,
            _ => JsonSerializer.Serialize(message.Payload),
        };

        if (_sendRaw != null)
        {
            _sendRaw(Peer, message.Type, payloadJson, deliveryMethod);
            return;
        }

        NetDataWriter writer = new();
        writer.Put(message.Type);
        writer.Put(payloadJson);
        Peer.Send(writer, deliveryMethod);
    }
}
