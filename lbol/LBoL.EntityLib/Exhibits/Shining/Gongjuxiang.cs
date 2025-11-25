using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class Gongjuxiang : ShiningExhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationRewardGenerating, delegate(StationEventArgs args)
			{
				Station station = args.Station;
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					Card card = station.Stage.GetShopToolCards(1)[0];
					station.Rewards.Add(StationReward.CreateToolCard(card));
				}
			});
		}
		protected override void OnLeaveBattle()
		{
			if (base.Counter == 0)
			{
				base.Active = false;
			}
			base.Counter = (base.Counter + 1) % base.Value1;
			if (base.Counter == 1)
			{
				base.Active = true;
			}
		}
	}
}
