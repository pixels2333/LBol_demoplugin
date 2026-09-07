using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

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

        public List<StatusEffectStateSnapshot> statusEffectStateSnapshot { get; set; }

        public PlayerStateSnapshot playerStateSnapshot { get; set; }

        public IntentionSnapshot intentionSnapshot { get; set; }

        public string BattleId { get; set; }

        public int Round { get; set; }

        public long TimestampTicks { get; set; } = DateTime.Now.Ticks;
}
