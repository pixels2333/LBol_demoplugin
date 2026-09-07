using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class CardStateSnapshot
{
        public string CardId { get; set; } = string.Empty;

        public string CardName { get; set; } = string.Empty;

        public string CardType { get; set; } = "Unknown";

        public string Rarity { get; set; } = "Common";

        public int ManaCost { get; set; } = 1;

        public int UpgradeCount { get; set; } = 0;

        public bool InHand { get; set; } = false;

        public bool InDeck { get; set; } = false;

        public bool InDiscard { get; set; } = false;

        public bool IsExhausted { get; set; } = false;

        public bool IsBanished { get; set; } = false;

        public bool IsTemporary { get; set; } = false;

        public int ZoneIndex { get; set; } = -1;

        public Dictionary<string, object> Metadata { get; set; } = [];
}
