using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000176 RID: 374
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class JimuWanju : Exhibit
	{
		// Token: 0x06000535 RID: 1333 RVA: 0x0000CE91 File Offset: 0x0000B091
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

		// Token: 0x06000536 RID: 1334 RVA: 0x0000CEB0 File Offset: 0x0000B0B0
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x0000CEB9 File Offset: 0x0000B0B9
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
