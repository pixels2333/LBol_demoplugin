using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B3 RID: 435
	[UsedImplicitly]
	public sealed class ZhanshuDaoliqi : Exhibit
	{
		// Token: 0x0600063F RID: 1599 RVA: 0x0000E748 File Offset: 0x0000C948
		protected override void OnEnterBattle()
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleBattleEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0000E7F7 File Offset: 0x0000C9F7
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0000E805 File Offset: 0x0000CA05
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0000E813 File Offset: 0x0000CA13
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			this.SetMana(args.DestinationCard);
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0000E821 File Offset: 0x0000CA21
		private void SetMana(Card card)
		{
			if (card.CardType == CardType.Skill)
			{
				base.NotifyActivating();
				card.AuraCost -= base.Mana;
			}
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0000E84C File Offset: 0x0000CA4C
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

		// Token: 0x06000645 RID: 1605 RVA: 0x0000E8BC File Offset: 0x0000CABC
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
