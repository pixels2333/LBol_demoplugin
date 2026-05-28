namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间配置类，定义房间的容量、可见性、密码及游戏模式等参数。
/// </summary>
public class RoomConfig
{
    /// <summary>房间最大玩家数，默认 4 人。</summary>
    public int MaxPlayers { get; set; } = 4;

    /// <summary>游戏是否已开始；开始后禁止新玩家加入。</summary>
    public bool IsGameStarted { get; set; } = false;

    /// <summary>房间是否公开可见于大厅列表。</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>私有房间的加入密码；公开房间此字段为 null。</summary>
    public string? Password { get; set; }

    /// <summary>游戏模式标识（如 "Standard"、"Hardcore"）。</summary>
    public string? GameMode { get; set; }

    /// <summary>房间描述，展示于房间列表 UI。</summary>
    public string? Description { get; set; }

    /// <summary>创建默认房间配置（4 人公开房）。</summary>
    public static RoomConfig Default()
    {
        return new RoomConfig { MaxPlayers = 4 };
    }
}
