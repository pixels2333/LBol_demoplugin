using System;
using System.Collections.Generic;
using LBoL.Core.Stations;
namespace LBoL.Presentation.UI.Panels
{
	public class ShowRewardContent
	{
		public RewardType RewardType { get; set; }
		public List<StationReward> Rewards { get; set; }
		public Station Station { get; set; }
		public bool ShowNextButton { get; set; }
	}
}
