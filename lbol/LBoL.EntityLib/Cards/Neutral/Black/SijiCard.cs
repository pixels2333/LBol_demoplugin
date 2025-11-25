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
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class SijiCard : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			Card card = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards[0] : null);
			if (card == null)
			{
				yield break;
			}
			CardType cardType = card.CardType;
			if (cardType == CardType.Status || cardType == CardType.Misfortune)
			{
				yield return new ExileCardAction(card);
			}
			else
			{
				card.DecreaseBaseCost(base.Mana);
				if (!card.IsRetain && !card.Summoned)
				{
					card.IsTempRetain = true;
				}
			}
			yield break;
		}
	}
}
