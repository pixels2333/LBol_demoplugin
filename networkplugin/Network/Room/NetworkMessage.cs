using System;

namespace NetworkPlugin.Network.Room;

public class NetworkMessage
{
    public string Type { get; set; }
    public object Payload { get; set; }
    public string SenderPlayerId { get;  set; }

    public RoomConfig GetRoomConfigPayload()
    {
        //TODO: 具体逻辑待定
        return (RoomConfig)Payload;
    }
}
