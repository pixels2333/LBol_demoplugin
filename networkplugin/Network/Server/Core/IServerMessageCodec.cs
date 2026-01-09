// 服务器消息编解码抽象：兼容现有 client/server 的 “messageType + jsonPayload” 传输格式。
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Server.Core;

public interface IServerMessageCodec
{
    bool TryDecode(NetPacketReader reader, out string messageType, out string jsonPayload);
    void Encode(NetDataWriter writer, string messageType, string jsonPayload);
}
