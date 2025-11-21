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
	// Token: 0x02000419 RID: 1049
	[UsedImplicitly]
	public sealed class DiscardToCharging : Card
	{
		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000E6D RID: 3693 RVA: 0x0001A7BB File Offset: 0x000189BB
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x0001A7C0 File Offset: 0x000189C0
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, list.Count, list);
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x0001A807 File Offset: 0x00018A07
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> cards = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			if (cards != null)
			{
				yield return new DiscardManyAction(cards);
				yield return base.BuffAction<Charging>(base.Value1 * cards.Count, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
