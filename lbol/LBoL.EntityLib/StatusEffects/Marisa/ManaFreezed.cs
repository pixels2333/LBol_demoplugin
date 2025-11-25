using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Marisa
{
	[UsedImplicitly]
	public sealed class ManaFreezed : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}
		private bool Active { get; set; }
		private int ActiveLevel { get; set; }
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleOwnerEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnTurnStarting));
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}
		private void OnAddCard(CardsEventArgs args)
		{
			if (this.Active)
			{
				Card[] cards = args.Cards;
				for (int i = 0; i < cards.Length; i++)
				{
					cards[i].AuraCost += this.Mana;
				}
			}
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			if (this.Active)
			{
				Card[] cards = args.Cards;
				for (int i = 0; i < cards.Length; i++)
				{
					cards[i].AuraCost += this.Mana;
				}
			}
		}
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			if (this.Active)
			{
				args.DestinationCard.AuraCost += this.Mana;
			}
		}
		private void OnTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			if (!this.Active)
			{
				this.Active = true;
				this.ActiveLevel = base.Level;
				foreach (Card card in base.Battle.EnumerateAllCards())
				{
					base.NotifyActivating();
					card.AuraCost += this.Mana;
				}
			}
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (this.Active)
			{
				foreach (Card card in base.Battle.EnumerateAllCards())
				{
					card.AuraCost -= ManaGroup.Anys(this.ActiveLevel);
				}
				this.Active = false;
				this.ActiveLevel = 0;
			}
		}
		protected override void OnRemoved(Unit unit)
		{
			if (this.Active)
			{
				foreach (Card card in unit.Battle.EnumerateAllCards())
				{
					card.AuraCost -= ManaGroup.Anys(this.ActiveLevel);
				}
				this.Active = false;
				this.ActiveLevel = 0;
			}
		}
	}
}
