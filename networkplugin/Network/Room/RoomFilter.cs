namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间列表查询过滤器，支持按公开性、容量、游戏状态、模式等维度筛选。
/// </summary>
public class RoomFilter
{
    /// <summary>仅返回公开/私有房间；null 表示不限制。</summary>
    public bool? IsPublic { get; set; }

    /// <summary>仅返回指定最大容量的房间；null 表示不限制。</summary>
    public int? MaxPlayers { get; set; }

    /// <summary>仅返回已开局/未开局房间；null 表示不限制。</summary>
    public bool? IsInGame { get; set; }

    /// <summary>仅返回指定游戏模式的房间；null 表示不限制。</summary>
    public string? GameMode { get; set; }
}
