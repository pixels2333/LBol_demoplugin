using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class FastMana : JadeBox
	{
		public ManaGroup AuraMana { get; set; }
		protected override void OnEnterBattle()
		{
			this.AuraMana = ManaGroup.Anys(-1);
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				card.AuraCost += this.AuraMana;
			}
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleBattleEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
		}
		private void OnPlayerTurnStarting(UnitEventArgs args)
		{
			int turnCounter = base.Battle.Player.TurnCounter;
			if (turnCounter > 1 && (turnCounter - 1) % base.Value1 == 0)
			{
				this.AuraMana += ManaGroup.Anys(1);
				foreach (Card card in base.Battle.EnumerateAllCards())
				{
					card.AuraCost += ManaGroup.Anys(1);
				}
			}
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
			card.AuraCost += this.AuraMana;
		}
		private void SetMana(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
			{
				card.AuraCost += this.AuraMana;
			}
		}
	}
}
