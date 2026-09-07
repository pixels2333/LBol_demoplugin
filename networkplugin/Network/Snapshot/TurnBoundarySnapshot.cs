using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public enum BoundaryType
{
        Start,
        End
}

public class TurnBoundarySnapshot
{
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

        public List<StatusEffectStateSnapshot> StatusEffects { get; set; }
        public PlayerStateSnapshot PlayerState { get; set; }
        public IntentionSnapshot Intentions { get; set; }

        public BoundaryType BoundaryType { get; set; }
        public string BattleId { get; set; }
        public int Round { get; set; }
        public long TimestampTicks { get; set; } = DateTime.Now.Ticks;
}
