using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B7 RID: 695
	[UsedImplicitly]
	public sealed class XingMoney : Card
	{
		// Token: 0x06000AAF RID: 2735 RVA: 0x00016050 File Offset: 0x00014250
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x0001606F File Offset: 0x0001426F
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (args.DieSource == this && !args.Unit.HasStatusEffect<Servant>())
			{
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.None);
			}
			yield break;
		}
	}
}
