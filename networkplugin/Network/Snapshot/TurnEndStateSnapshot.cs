using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 玩家回合结束时的最小状态快照。
/// </summary>
/// <remarks>
/// 用途：作为回合边界事件（OnTurnEnd）的 payload，供服务端/其他客户端进行时间线对齐或状态校验。
/// 当前版本刻意保持字段少且与 TurnStartStateSnapshot 结构一致，便于后续合并/扩展。
/// </remarks>
public class TurnEndStateSnapshot
{
    public TurnEndStateSnapshot(
        List<StatusEffectStateSnapshot> statusEffectStateSnapshot,
        PlayerStateSnapshot playerStateSnapshot,
        IntentionSnapshot intentionSnapshot,
        string battleId,
        int round)
    {
        this.statusEffectStateSnapshot = statusEffectStateSnapshot;
        this.playerStateSnapshot = playerStateSnapshot;
        this.intentionSnapshot = intentionSnapshot;
        BattleId = battleId;
        Round = round;
    }

    /// <summary>
    /// 状态效果快照（当前实现可为空/空列表，预留扩展）。
    /// </summary>
    public List<StatusEffectStateSnapshot> statusEffectStateSnapshot { get; set; }

    /// <summary>
    /// 玩家状态快照。
    /// </summary>
    public PlayerStateSnapshot playerStateSnapshot { get; set; }

    /// <summary>
    /// 敌方意图快照（或战场意图信息）。
    /// </summary>
    public IntentionSnapshot intentionSnapshot { get; set; }

    /// <summary>
    /// 战斗/节点标识（用于跨客户端对齐同一场战斗）。
    /// </summary>
    public string BattleId { get; set; }

    /// <summary>
    /// 回合/轮次计数（通常来自 BattleController.RoundCounter）。
    /// </summary>
    public int Round { get; set; }

    /// <summary>
    /// 发送时间戳（Ticks）。
    /// </summary>
    public long TimestampTicks { get; set; } = DateTime.Now.Ticks;
}
