using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class ReimuFreeAttackSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this.CardToFree(base.Battle.EnumerateAllCards());
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.CardToFree(args.Cards);
		}
		private void OnAddCard(CardsEventArgs args)
		{
			this.CardToFree(args.Cards);
		}
		private void CardToFree(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
			{
				if (card.CardType == CardType.Attack)
				{
					card.FreeCost = true;
				}
			}
		}
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				if (card.CardType == CardType.Attack)
				{
					card.FreeCost = false;
				}
			}
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack)
			{
				int num = base.Level - 1;
				base.Level = num;
				if (base.Level <= 0)
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}
		[UsedImplicitly]
		public ManaGroup Mana = ManaGroup.Empty;
	}
}
