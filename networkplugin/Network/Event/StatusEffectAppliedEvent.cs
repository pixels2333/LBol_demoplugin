using System.Collections.Generic;

namespace NetworkPlugin.Network.Event;

public class StatusEffectAppliedEvent : GameEvent
{
    public StatusEffectAppliedEvent() : base()
    {
        EventType = "StatusEffectApplied";
    }

    public StatusEffectAppliedEvent(string playerId, string targetId, string effectId, string effectName, int level, bool isDebuff)
        : base("StatusEffectApplied", playerId, new Dictionary<string, object>
        {
            { "TargetId", targetId },
            { "EffectId", effectId },
            { "EffectName", effectName },
            { "Level", level },
            { "IsDebuff", isDebuff }
        })
    {
    }
}
