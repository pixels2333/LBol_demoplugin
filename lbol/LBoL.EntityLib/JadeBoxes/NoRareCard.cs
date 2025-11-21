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
	// Token: 0x02000115 RID: 277
	[UsedImplicitly]
	public sealed class NoRareCard : JadeBox
	{
		// Token: 0x060003D1 RID: 977 RVA: 0x0000AA1B File Offset: 0x00008C1B
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
			base.ReactBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnCardAdded));
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0000AA57 File Offset: 0x00008C57
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

		// Token: 0x060003D3 RID: 979 RVA: 0x0000AA6E File Offset: 0x00008C6E
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
