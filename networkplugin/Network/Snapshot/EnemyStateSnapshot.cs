using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class EnemyStateSnapshot
{
        public string EnemyId { get; set; } = string.Empty;

        public string EnemyName { get; set; } = "";

        public string EnemyType { get; set; } = "Normal";

        public int Health { get; set; } = 0;

        public int MaxHealth { get; set; } = 0;

        public int Block { get; set; } = 0;

        public int Shield { get; set; } = 0;

        public List<StatusEffectStateSnapshot> StatusEffects { get; set; } = [];

        public IntentionSnapshot Intention { get; set; } = new();

        public int Index { get; set; } = 0;

        public bool IsAlive { get; set; } = true;
}
