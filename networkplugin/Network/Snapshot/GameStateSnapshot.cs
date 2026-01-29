using System.Collections.Generic;

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
    /// 主种子（LBoL: GameRunController.RootSeed）。
    /// 用于中途加入/重连时对齐确定性生成（地图/随机池等）。
    /// </summary>
    public ulong? RootSeed { get; set; }

    /// <summary>
    /// UI 种子（LBoL: GameRunController.UISeed）。
    /// </summary>
    public ulong? UISeed { get; set; }

    /// <summary>
    /// 当前关卡索引（LBoL: GameRunSaveData.StageIndex / GameRunController.CurrentStage.Index）。
    /// </summary>
    public int? StageIndex { get; set; }

    // --- Host start config (for mid-game join) ---
    // Joiner can choose character, but must start a run using the host's difficulty/seed/stages
    // so that Stage.MapSeed and MapSeedUlong align deterministically.

    /// <summary>
    /// 主机难度（LBoL: GameRunController.Difficulty）。
    /// </summary>
    public int? Difficulty { get; set; }

    /// <summary>
    /// 主机谜题标记（LBoL: GameRunController.Puzzles）。
    /// </summary>
    public int? Puzzles { get; set; }

    /// <summary>
    /// 主机游戏模式（LBoL: GameRunController.Mode）。
    /// </summary>
    public int? GameMode { get; set; }

    /// <summary>
    /// 主机是否显示随机结果（LBoL: GameRunController.ShowRandomResult）。
    /// </summary>
    public bool? ShowRandomResult { get; set; }

    /// <summary>
    /// 主机的关卡类型列表（按顺序）。值为 Stage.GetType().Name。
    /// </summary>
    public List<string> StageTypeNames { get; set; } = [];

    /// <summary>
    /// Stage0 的 DebutAdventureType 名称（Type.Name），用于构造一致的开局流程。
    /// </summary>
    public string? DebutAdventureTypeName { get; set; }

    /// <summary>
    /// 房间ID
    /// </summary>
    public string RoomId { get; set; } = "unknown";

    /// <summary>
    /// 主机玩家ID
    /// </summary>
    public string HostPlayerId { get; set; } = "unknown";
}
