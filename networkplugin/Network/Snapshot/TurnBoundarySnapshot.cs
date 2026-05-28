using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 回合边界类型，标识是回合开始还是回合结束
/// </summary>
public enum BoundaryType
{
    /// <summary>回合开始</summary>
    Start,
    /// <summary>回合结束</summary>
    End
}

/// <summary>
/// 回合边界快照，记录回合切换时刻的完整游戏状态
/// 用于断线重连时的状态恢复和回合同步
/// </summary>
public class TurnBoundarySnapshot
{
    /// <summary>
    /// 初始化回合边界快照
    /// </summary>
    /// <param name="statusEffects">状态效果快照列表</param>
    /// <param name="playerState">玩家状态快照</param>
    /// <param name="intentions">敌人意图快照</param>
    /// <param name="boundaryType">边界类型（开始/结束）</param>
    /// <param name="battleId">战斗ID（可选）</param>
    /// <param name="round">回合数（可选）</param>
    public TurnBoundarySnapshot(
        List<StatusEffectStateSnapshot> statusEffects,
        PlayerStateSnapshot playerState,
        IntentionSnapshot intentions,
        BoundaryType boundaryType,
        string battleId = null,
        int round = 0)
    {
        StatusEffects = statusEffects;
        PlayerState = playerState;
        Intentions = intentions;
        BoundaryType = boundaryType;
        BattleId = battleId;
        Round = round;
    }

    /// <summary>边界时的状态效果快照列表</summary>
    public List<StatusEffectStateSnapshot> StatusEffects { get; set; }
    /// <summary>边界时的玩家状态快照</summary>
    public PlayerStateSnapshot PlayerState { get; set; }
    /// <summary>边界时的敌人意图快照</summary>
    public IntentionSnapshot Intentions { get; set; }

    /// <summary>边界类型（开始/结束）</summary>
    public BoundaryType BoundaryType { get; set; }
    /// <summary>战斗ID</summary>
    public string BattleId { get; set; }
    /// <summary>当前回合数</summary>
    public int Round { get; set; }
    /// <summary>快照创建时间戳（UTC刻度）</summary>
    public long TimestampTicks { get; set; } = DateTime.Now.Ticks;
}
