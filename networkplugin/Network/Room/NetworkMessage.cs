// 房间消息模型：用于 Relay/房间广播的消息封装，兼容 Payload 为对象或 JSON 字符串两种形态。
using System.Text.Json;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间网络消息模型，用于 Relay/房间广播的消息封装
/// 兼容 Payload 为对象或 JSON 字符串两种形态
/// </summary>
public class NetworkMessage
{
    /// <summary>消息类型</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>消息负载（对象或JSON字符串）</summary>
    public object Payload { get; set; }
    /// <summary>发送方玩家ID</summary>
    public string SenderPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 获取指定类型的负载数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的负载对象，失败时返回默认值</returns>
    public T GetPayload<T>()
    {
        if (Payload == null)
        {
            return default;
        }

        try
        {
            if (Payload is T t)
            {
                return t;
            }

            if (Payload is string json)
            {
                return JsonCompat.Deserialize<T>(json);
            }

            if (Payload is JsonElement element)
            {
                return JsonCompat.Deserialize<T>(element.GetRawText());
            }

            return JsonCompat.Deserialize<T>(JsonCompat.Serialize(Payload));
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 获取房间配置负载
    /// </summary>
    /// <returns>房间配置对象，失败时返回默认配置</returns>
    public RoomConfig GetRoomConfigPayload()
    {
        return GetPayload<RoomConfig>() ?? RoomConfig.Default();
    }
}
