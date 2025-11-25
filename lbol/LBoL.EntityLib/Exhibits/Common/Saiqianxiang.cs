using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class Saiqianxiang : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				base.NotifyActivating();
				GapStation gapStation = (GapStation)args.Station;
				GetMoney getMoney = Library.CreateGapOption<GetMoney>();
				getMoney.Value = base.Value1;
				gapStation.GapOptions.Add(getMoney);
			});
			base.GameRun.RewardCardAbandonMoney += base.Value2;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.RewardCardAbandonMoney -= base.Value2;
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
