using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class BattleRewardSnapshot
{
        public int GoldReward { get; set; } = 0;

        public List<string> CardRewards { get; set; } = [];

        public List<string> ExhibitRewards { get; set; } = [];

        public List<string> ToolCardRewards { get; set; } = [];

        public bool IsRewardClaimed { get; set; } = false;
}
