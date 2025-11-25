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
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.Cards.Neutral.Red
{
	[UsedImplicitly]
	public sealed class KongMana : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count == 1)
			{
				this._oneTargetHand = list[0];
			}
			if (list.Count <= 1)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new ExileCardAction(card);
					yield return new GainManaAction(card.CostToMana(false));
				}
				card = null;
			}
			else if (this._oneTargetHand != null)
			{
				yield return new ExileCardAction(this._oneTargetHand);
				yield return new GainManaAction(this._oneTargetHand.CostToMana(false));
				this._oneTargetHand = null;
			}
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<PManaCard>() });
			yield break;
		}
		private Card _oneTargetHand;
	}
}
