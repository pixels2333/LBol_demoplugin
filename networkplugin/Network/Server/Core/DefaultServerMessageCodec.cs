// 默认的服务器消息编解码：按 “messageType(string) + jsonPayload(string)” 进行读写。
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 默认的服务器消息编解码实现
/// 按 "messageType(string) + jsonPayload(string)" 格式进行读写
/// </summary>
public sealed class DefaultServerMessageCodec : IServerMessageCodec
{
    /// <summary>
    /// 尝试从数据读取器中解码消息类型和 JSON 负载
    /// </summary>
    /// <param name="reader">网络数据读取器</param>
    /// <param name="messageType">解码出的消息类型</param>
    /// <param name="jsonPayload">解码出的 JSON 负载</param>
    /// <returns>是否解码成功</returns>
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

    /// <summary>
    /// 编码消息类型和 JSON 负载到数据写入器
    /// </summary>
    /// <param name="writer">网络数据写入器</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="jsonPayload">JSON 负载</param>
    public void Encode(NetDataWriter writer, string messageType, string jsonPayload)
    {
        writer.Put(messageType ?? string.Empty);
        writer.Put(jsonPayload ?? string.Empty);
    }
}
