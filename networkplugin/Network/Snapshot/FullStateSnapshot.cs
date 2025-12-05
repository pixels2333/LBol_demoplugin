using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 完整游戏状态快照（用于断线重连和中途加入）
/// </summary>
public class FullStateSnapshot
{
    /// <summary>
    /// 快照创建时间
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 游戏全局状态
    /// </summary>
    public GameStateSnapshot GameState { get; set; } = new();

    /// <summary>
    /// 所有玩家的状态
    /// </summary>
    public List<PlayerStateSnapshot> PlayerStates { get; set; } = [];

    /// <summary>
    /// 战斗状态（如果在战斗中）
    /// </summary>
    public BattleStateSnapshot? BattleState { get; set; }

    /// <summary>
    /// 地图状态
    /// </summary>
    public MapStateSnapshot MapState { get; set; } = new();

    /// <summary>
    /// 事件历史索引（用于追赶）
    /// </summary>
    public long EventIndex { get; set; }

    /// <summary>
    /// 游戏版本（用于兼容性检查）
    /// </summary>
    public string GameVersion { get; set; } = "1.0.0";

    /// <summary>
    /// 模组版本（用于MOD兼容性）
    /// </summary>
    public string ModVersion { get; set; } = "1.0.0";
}
