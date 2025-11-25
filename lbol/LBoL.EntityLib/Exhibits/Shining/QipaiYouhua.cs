using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class QipaiYouhua : ShiningExhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMoney(base.Value1, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Entity,
				Source = this
			});
			base.GameRun.UpgradeRandomCards(base.Value2, default(CardType?));
			base.GameRun.GainMaxHp(base.Value3, true, true);
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
