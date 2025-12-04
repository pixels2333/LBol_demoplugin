namespace NetworkPlugin.Network.Room;

public class NetworkMessage
{
    public string Type { get; set; }
    public object Payload { get; set; }
    public string SenderPlayerId { get;  set; }
}
