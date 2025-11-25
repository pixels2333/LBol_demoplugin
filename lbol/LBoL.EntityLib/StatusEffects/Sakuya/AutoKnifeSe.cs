using System;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;
namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	public sealed class AutoKnifeSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				if (card is Knife)
				{
					card.IsAutoExile = true;
				}
			}
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
		}
		private void OnAddCard(CardsEventArgs args)
		{
			foreach (Card card in args.Cards)
			{
				if (card is Knife)
				{
					card.IsAutoExile = true;
				}
			}
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			foreach (Card card in args.Cards)
			{
				if (card is Knife)
				{
					card.IsAutoExile = true;
				}
			}
		}
	}
}
