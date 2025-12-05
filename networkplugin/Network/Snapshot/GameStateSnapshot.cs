namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 游戏全局状态快照
/// </summary>
public class GameStateSnapshot
{
    /// <summary>
    /// 当前游戏阶段（Menu/InGame/Combat/Rest/Event）
    /// </summary>
    public string GamePhase { get; set; } = "Unknown";

    /// <summary>
    /// 当前关卡（Act）
    /// </summary>
    public int CurrentAct { get; set; } = 1;

    /// <summary>
    /// 当前楼层
    /// </summary>
    public int CurrentFloor { get; set; } = 0;

    /// <summary>
    /// 游戏是否开始
    /// </summary>
    public bool GameStarted { get; set; } = false;

    /// <summary>
    /// 游戏是否结束
    /// </summary>
    public bool GameEnded { get; set; } = false;

    /// <summary>
    /// 胜利/失败
    /// </summary>
    public string? GameResult { get; set; }

    /// <summary>
    /// 当前活动玩家ID（回合制）
    /// </summary>
    public string? ActivePlayerId { get; set; }

    /// <summary>
    /// 回合数
    /// </summary>
    public int TurnCount { get; set; } = 0;

    /// <summary>
    /// 游戏种子（用于复现随机事件）
    /// </summary>
    public int GameSeed { get; set; } = 0;

    /// <summary>
    /// 房间ID
    /// </summary>
    public string RoomId { get; set; } = "unknown";

    /// <summary>
    /// 主机玩家ID
    /// </summary>
    public string HostPlayerId { get; set; } = "unknown";
}
