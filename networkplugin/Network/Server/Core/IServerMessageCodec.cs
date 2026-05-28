// 服务器消息编解码抽象：兼容现有 client/server 的 “messageType + jsonPayload” 传输格式。
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 服务器消息编解码接口
/// 兼容现有 client/server 的 "messageType + jsonPayload" 传输格式
/// </summary>
public interface IServerMessageCodec
{
    /// <summary>
    /// 尝试从数据读取器中解码消息
    /// </summary>
    /// <param name="reader">网络数据读取器</param>
    /// <param name="messageType">解码出的消息类型</param>
    /// <param name="jsonPayload">解码出的 JSON 负载</param>
    /// <returns>是否解码成功</returns>
    bool TryDecode(NetPacketReader reader, out string messageType, out string jsonPayload);
    /// <summary>
    /// 编码消息到数据写入器
    /// </summary>
    /// <param name="writer">网络数据写入器</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="jsonPayload">JSON 负载</param>
    void Encode(NetDataWriter writer, string messageType, string jsonPayload);
}
