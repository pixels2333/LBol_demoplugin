using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000129 RID: 297
	[UsedImplicitly]
	public sealed class Gongjuxiang : ShiningExhibit
	{
		// Token: 0x06000412 RID: 1042 RVA: 0x0000B1EB File Offset: 0x000093EB
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

		// Token: 0x06000413 RID: 1043 RVA: 0x0000B20A File Offset: 0x0000940A
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
