namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间列表过滤器
/// </summary>
public class RoomFilter
{
    /// <summary>是否公开房间</summary>
    public bool? IsPublic { get; set; }
    /// <summary>最大玩家数过滤</summary>
    public int? MaxPlayers { get; set; }
    /// <summary>是否在游戏中</summary>
    public bool? IsInGame { get; set; }
    /// <summary>游戏模式过滤</summary>
    public string? GameMode { get; set; }
}
