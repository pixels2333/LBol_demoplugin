using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000142 RID: 322
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 2)]
	public sealed class YizangnuoWuzhi : ShiningExhibit
	{
		// Token: 0x0600046B RID: 1131 RVA: 0x0000BBC1 File Offset: 0x00009DC1
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationRewardGenerating, delegate(StationEventArgs args)
			{
				Station station = args.Station;
				if (station is EliteEnemyStation)
				{
					base.NotifyActivating();
					station.Rewards.Add(StationReward.CreateExhibit(station.Stage.GetEliteEnemyExhibit()));
				}
			});
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x0000BBE0 File Offset: 0x00009DE0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0000BBFF File Offset: 0x00009DFF
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
