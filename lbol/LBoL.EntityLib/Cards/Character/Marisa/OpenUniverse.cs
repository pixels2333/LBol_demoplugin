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

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000434 RID: 1076
	[UsedImplicitly]
	public sealed class OpenUniverse : Card
	{
		// Token: 0x06000EB0 RID: 3760 RVA: 0x0001AD04 File Offset: 0x00018F04
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZoneToShow, (Card card) => card.CanBeDuplicated));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(1, 1, list, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000EB1 RID: 3761 RVA: 0x0001AD5A File Offset: 0x00018F5A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards[0] : null);
			if (card != null)
			{
				List<Card> cards = new List<Card>();
				for (int i = 0; i < 2; i++)
				{
					Card card2 = card.CloneBattleCard();
					card2.SetTurnCost(base.Mana);
					card2.IsExile = true;
					cards.Add(card2);
				}
				yield return new RemoveCardAction(card);
				yield return new AddCardsToHandAction(cards, AddCardsType.Normal);
				cards = null;
			}
			yield break;
		}
	}
}
