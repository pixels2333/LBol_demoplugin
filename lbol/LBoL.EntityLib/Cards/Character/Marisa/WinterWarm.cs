using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000451 RID: 1105
	[UsedImplicitly]
	public sealed class WinterWarm : Card
	{
		// Token: 0x06000F00 RID: 3840 RVA: 0x0001B29C File Offset: 0x0001949C
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value1, list);
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x0001B2E3 File Offset: 0x000194E3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> cards = ((SelectHandInteraction)precondition).SelectedCards;
				if (cards.Count > 0)
				{
					yield return new ExileManyCardAction(cards);
					yield return base.BuffAction<Concentration>(cards.Count, 0, 0, 0, 0.2f);
				}
				cards = null;
			}
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
