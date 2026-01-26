// 房间消息模型：用于 Relay/房间广播的消息封装，兼容 Payload 为对象或 JSON 字符串两种形态。
using System;
using System.Text.Json;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.Room;

public class NetworkMessage
{
    public string Type { get; set; } = string.Empty;
    public object Payload { get; set; }
    public string SenderPlayerId { get; set; } = string.Empty;

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

    public RoomConfig GetRoomConfigPayload()
    {
        return GetPayload<RoomConfig>() ?? RoomConfig.Default();
    }
}
