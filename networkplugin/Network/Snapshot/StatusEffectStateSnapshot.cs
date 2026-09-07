namespace NetworkPlugin.Network.Snapshot;

public class StatusEffectStateSnapshot
{
        public string EffectId { get; set; } = string.Empty;

        public string EffectName { get; set; } = string.Empty;

        public string EffectType { get; set; } = "Unknown";

        public int Level { get; set; } = 1;

        public int Duration { get; set; } = 0;

        public bool IsDebuff { get; set; } = false;

        public bool IsPermanent { get; set; } = false;

        public int EffectValue { get; set; } = 0;

        public string Description { get; set; } = "";

        public string SourceId { get; set; } = "";
}
