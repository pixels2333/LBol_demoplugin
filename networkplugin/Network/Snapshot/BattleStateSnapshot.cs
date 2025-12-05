using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 战��状态快照
/// </summary>
public class BattleStateSnapshot
{
    /// <summary>
    /// 是否在战斗中
    /// </summary>
    public bool IsInBattle { get; set; } = false;

    /// <summary>
    /// 战斗ID
    /// </summary>
    public string BattleId { get; set; } = "";

    /// <summary>
    /// 当前回合
    /// </summary>
    public int CurrentTurn { get; set; } = 1;

    /// <summary>
    /// 当前回合玩家ID
    /// </summary>
    public string CurrentTurnPlayerId { get; set; } = "unknown";

    /// <summary>
    /// 回合阶段（Player/Enemy）
    /// </summary>
    public string TurnPhase { get; set; } = "Player";

    /// <summary>
    /// 敌人状态
    /// </summary>
    public List<EnemyStateSnapshot> Enemies { get; set; } = [];

    /// <summary>
    /// 战斗开始时间
    /// </summary>
    public long BattleStartTime { get; set; } = 0;

    /// <summary>
    /// 是否是Boss战
    /// </summary>
    public bool IsBossBattle { get; set; } = false;

    /// <summary>
    /// 奖励预览（击败敌人后的奖励）
    /// </summary>
    public BattleRewardSnapshot Rewards { get; set; } = new();

    /// <summary>
    /// 战斗类型（Normal/Elite/Boss/Event）
    /// </summary>
    public string BattleType { get; set; } = "Normal";
}
