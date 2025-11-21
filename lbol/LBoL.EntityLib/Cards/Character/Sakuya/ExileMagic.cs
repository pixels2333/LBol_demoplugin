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

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200038B RID: 907
	[UsedImplicitly]
	public sealed class ExileMagic : Card
	{
		// Token: 0x06000CEF RID: 3311 RVA: 0x00018C88 File Offset: 0x00016E88
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value1, list);
		}

		// Token: 0x06000CF0 RID: 3312 RVA: 0x00018CCF File Offset: 0x00016ECF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> cards = ((SelectHandInteraction)precondition).SelectedCards;
				if (cards.Count > 0)
				{
					yield return new ExileManyCardAction(cards);
					yield return new AddCardsToHandAction(Library.CreateCards<Knife>(cards.Count, false), AddCardsType.Normal);
				}
				cards = null;
			}
			yield break;
		}
	}
}
