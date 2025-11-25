using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class JimuWanju : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationRewardGenerating, delegate(StationEventArgs args)
			{
				Station station = args.Station;
				if (station.Type == StationType.Enemy)
				{
					base.NotifyActivating();
					station.Rewards.Add(station.Stage.GetEnemyCardReward());
				}
			});
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
