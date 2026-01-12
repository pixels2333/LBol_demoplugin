// 默认的服务器消息编解码：按 “messageType(string) + jsonPayload(string)” 进行读写。
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Server.Core;

public sealed class DefaultServerMessageCodec : IServerMessageCodec
{
    public bool TryDecode(NetPacketReader reader, out string messageType, out string jsonPayload)
    {
        messageType = null;
        jsonPayload = string.Empty;

        try
        {
            if (reader == null || reader.AvailableBytes <= 0)
            {
                return false;
            }

            messageType = reader.GetString();
            jsonPayload = reader.AvailableBytes > 0 ? reader.GetString() : string.Empty;
            return !string.IsNullOrWhiteSpace(messageType);
        }
        catch
        {
            return false;
        }
    }

    public void Encode(NetDataWriter writer, string messageType, string jsonPayload)
    {
        writer.Put(messageType ?? string.Empty);
        writer.Put(jsonPayload ?? string.Empty);
    }
}
