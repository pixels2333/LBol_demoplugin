using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 回合开始时状态快照，包含回合开始瞬间的完整玩家状态、状态效果和敌人意图
/// </summary>
public class TurnStartStateSnapshot
{
    /// <summary>
    /// 初始化回合开始状态快照
    /// </summary>
    /// <param name="statusEffectStateSnapshot">状态效果快照列表</param>
    /// <param name="playerStateSnapshot">玩家状态快照</param>
    /// <param name="intentionSnapshot">敌人意图快照</param>
    public TurnStartStateSnapshot(List<StatusEffectStateSnapshot> statusEffectStateSnapshot, PlayerStateSnapshot playerStateSnapshot, IntentionSnapshot intentionSnapshot)
    {
        this.statusEffectStateSnapshot = statusEffectStateSnapshot;
        this.playerStateSnapshot = playerStateSnapshot;
        this.intentionSnapshot = intentionSnapshot;
    }

    /// <summary>
    /// 当前回合的状态效果快照列表
    /// </summary>
    public List<StatusEffectStateSnapshot> statusEffectStateSnapshot { get; set; }
    
    /// <summary>
    /// 当前回合的玩家状态快照
    /// </summary>
    public PlayerStateSnapshot playerStateSnapshot { get; set; }
    
    /// <summary>
    /// 当前回合的敌人意图快照
    /// </summary>
    public IntentionSnapshot intentionSnapshot { get; set; }

}
