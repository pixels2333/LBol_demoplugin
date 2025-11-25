using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.MultiColor
{
	[UsedImplicitly]
	public sealed class ZhenmiaowanAbilitySe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleOwnerEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
		}
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				card.AuraCost += this.Mana;
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
			base.NotifyActivating();
			card.AuraCost -= this.Mana;
		}
		private void SetMana(IEnumerable<Card> cards)
		{
			bool flag = true;
			foreach (Card card in cards)
			{
				if (flag)
				{
					base.NotifyActivating();
					flag = false;
				}
				card.AuraCost -= this.Mana;
			}
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				IEnumerable<Card> enumerable = base.Battle.EnumerateAllCards();
				bool flag2 = true;
				foreach (Card card in enumerable)
				{
					if (flag2)
					{
						base.NotifyActivating();
						flag2 = false;
					}
					card.AuraCost -= ManaGroup.Anys(other.Level);
				}
			}
			return flag;
		}
	}
}
