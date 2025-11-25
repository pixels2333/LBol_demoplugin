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
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 2)]
	public sealed class YizangnuoWuzhi : ShiningExhibit
	{
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
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
