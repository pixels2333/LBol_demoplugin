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
	// Token: 0x02000125 RID: 293
	[UsedImplicitly]
	public sealed class ClassicWunvfu : ShiningExhibit
	{
		// Token: 0x06000406 RID: 1030 RVA: 0x0000B09B File Offset: 0x0000929B
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x0000B0BA File Offset: 0x000092BA
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
