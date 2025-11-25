using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class ClassicWunvfu : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			int num = Enumerable.Count<Card>(base.Battle.HandZone, (Card card) => card.IsUpgraded);
			if (num > 0)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, base.Owner, new BlockInfo(num * base.Value1, BlockShieldType.Direct), false);
			}
			yield break;
		}
	}
}
