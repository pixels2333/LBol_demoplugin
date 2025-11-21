using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000440 RID: 1088
	[UsedImplicitly]
	public sealed class RunAway : Card
	{
		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000ED8 RID: 3800 RVA: 0x0001B02C File Offset: 0x0001922C
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x0001B02F File Offset: 0x0001922F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> hand = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (hand.Count > 0)
			{
				yield return new DiscardManyAction(hand);
				foreach (Card card2 in hand)
				{
					card2.DecreaseTurnCost(base.Mana);
				}
			}
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Value2 > 0)
			{
				yield return new DrawManyCardAction(base.Value2);
			}
			yield break;
		}
	}
}
