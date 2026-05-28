namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间配置
/// </summary>
public class RoomConfig
{
    /// <summary>最大玩家数</summary>
    public int MaxPlayers { get; set; } = 4;
    /// <summary>游戏是否已开始</summary>
    public bool IsGameStarted { get; set; } = false;
    /// <summary>房间是否公开</summary>
    public bool IsPublic { get; set; } = true;
    /// <summary>房间密码（私有房间）</summary>
    public string? Password { get; set; }
    /// <summary>游戏模式</summary>
    public string? GameMode { get; set; }
    /// <summary>房间描述</summary>
    public string? Description { get; set; }

    /// <summary>
    /// 创建默认房间配置
    /// </summary>
    public static RoomConfig Default()
    {
        return new RoomConfig { MaxPlayers = 4 };
    }
}
