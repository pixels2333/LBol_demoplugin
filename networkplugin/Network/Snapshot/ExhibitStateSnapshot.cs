using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class ExhibitStateSnapshot
{
        public string ExhibitId { get; set; } = string.Empty;

        public string ExhibitName { get; set; } = string.Empty;

        public string ExhibitType { get; set; } = "Unknown";

        public string Rarity { get; set; } = "Common";

        public bool IsActive { get; set; } = false;

        public bool IsBlackout { get; set; } = false;

        public bool HasCounter { get; set; } = false;

        public int Counter { get; set; } = 0;

        public string IconName { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsBossExhibit { get; set; } = false;

        public bool IsStarterExhibit { get; set; } = false;

        public Dictionary<string, object> Config { get; set; } = [];
}
