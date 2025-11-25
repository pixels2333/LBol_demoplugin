using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class NoRareCard : JadeBox
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
			base.ReactBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnCardAdded));
		}
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			Card card = args.Card;
			if (card.Config.Rarity == Rarity.Rare)
			{
				yield return new ExileCardAction(card);
				yield return new DrawCardAction();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnCardAdded(CardsEventArgs args)
		{
			Card[] cards = args.Cards;
			foreach (Card card in cards)
			{
				if (card.Config.Rarity == Rarity.Rare)
				{
					yield return new ExileCardAction(card);
					yield return new DrawCardAction();
					yield return new GainManaAction(base.Mana);
				}
			}
			Card[] array = null;
			yield break;
		}
	}
}
