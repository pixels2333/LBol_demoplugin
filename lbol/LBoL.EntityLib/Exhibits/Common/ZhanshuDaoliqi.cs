using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class ZhanshuDaoliqi : Exhibit
	{
		protected override void OnEnterBattle()
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleBattleEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
		}
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			this.SetMana(args.DestinationCard);
		}
		private void SetMana(Card card)
		{
			if (card.CardType == CardType.Skill)
			{
				base.NotifyActivating();
				card.AuraCost -= base.Mana;
			}
		}
		private void SetMana(IEnumerable<Card> cards)
		{
			bool flag = true;
			foreach (Card card in cards)
			{
				if (card.CardType == CardType.Skill)
				{
					if (flag)
					{
						base.NotifyActivating();
						flag = false;
					}
					card.AuraCost -= base.Mana;
				}
			}
		}
		protected override void OnLeaveBattle()
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				if (card.CardType == CardType.Skill)
				{
					card.AuraCost += base.Mana;
				}
			}
		}
	}
}
