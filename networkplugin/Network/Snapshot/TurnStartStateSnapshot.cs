using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class TurnStartStateSnapshot
{
    public TurnStartStateSnapshot(List<StatusEffectStateSnapshot> statusEffectStateSnapshot, PlayerStateSnapshot playerStateSnapshot, IntentionSnapshot intentionSnapshot)
    {
        this.statusEffectStateSnapshot = statusEffectStateSnapshot;
        this.playerStateSnapshot = playerStateSnapshot;
        this.intentionSnapshot = intentionSnapshot;
    }



    // 新的构造函数 - 接受完整的意图数据字典
    public List<StatusEffectStateSnapshot> statusEffectStateSnapshot { get; set; }
    public PlayerStateSnapshot playerStateSnapshot { get; set; }
    public IntentionSnapshot intentionSnapshot { get; set; }

}
