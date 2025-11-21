using System;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000012 RID: 18
	public sealed class AutoKnifeSe : StatusEffect
	{
		// Token: 0x06000020 RID: 32 RVA: 0x00002324 File Offset: 0x00000524
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

		// Token: 0x06000021 RID: 33 RVA: 0x000023F4 File Offset: 0x000005F4
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

		// Token: 0x06000022 RID: 34 RVA: 0x0000242C File Offset: 0x0000062C
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
