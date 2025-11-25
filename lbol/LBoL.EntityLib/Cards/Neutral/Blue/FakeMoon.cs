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
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class FakeMoon : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && hand.CanBeDuplicated));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card origin = ((SelectHandInteraction)precondition).SelectedCards[0];
				List<Card> list = new List<Card>();
				for (int i = 0; i < base.Value1; i++)
				{
					Card card = origin.CloneBattleCard();
					card.SetTurnCost(base.Mana);
					card.IsExile = true;
					card.IsEthereal = true;
					list.Add(card);
				}
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
				if (origin.CardType == CardType.Ability || origin.IsExile)
				{
					origin.IsCopy = true;
				}
				origin = null;
			}
			yield break;
		}
	}
}
