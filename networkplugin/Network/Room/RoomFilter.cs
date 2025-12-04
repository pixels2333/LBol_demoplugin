namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间列表过滤器
/// </summary>
public class RoomFilter
{
    public bool? IsPublic { get; set; }
    public int? MaxPlayers { get; set; }
    public bool? IsInGame { get; set; }
    public string? GameMode { get; set; }
}
