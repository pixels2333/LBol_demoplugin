using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000112 RID: 274
	[UsedImplicitly]
	public sealed class FastMana : JadeBox
	{
		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060003BF RID: 959 RVA: 0x0000A688 File Offset: 0x00008888
		// (set) Token: 0x060003C0 RID: 960 RVA: 0x0000A690 File Offset: 0x00008890
		public ManaGroup AuraMana { get; set; }

		// Token: 0x060003C1 RID: 961 RVA: 0x0000A69C File Offset: 0x0000889C
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

		// Token: 0x060003C2 RID: 962 RVA: 0x0000A7BC File Offset: 0x000089BC
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

		// Token: 0x060003C3 RID: 963 RVA: 0x0000A854 File Offset: 0x00008A54
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0000A862 File Offset: 0x00008A62
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x0000A870 File Offset: 0x00008A70
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			this.SetMana(args.DestinationCard);
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x0000A87E File Offset: 0x00008A7E
		private void SetMana(Card card)
		{
			card.AuraCost += this.AuraMana;
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0000A898 File Offset: 0x00008A98
		private void SetMana(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
			{
				card.AuraCost += this.AuraMana;
			}
		}
	}
}
