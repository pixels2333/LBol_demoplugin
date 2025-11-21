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
	// Token: 0x02000059 RID: 89
	[UsedImplicitly]
	public sealed class ZhenmiaowanAbilitySe : StatusEffect
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000130 RID: 304 RVA: 0x0000447A File Offset: 0x0000267A
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00004488 File Offset: 0x00002688
		protected override void OnAdded(Unit unit)
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleOwnerEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00004538 File Offset: 0x00002738
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				card.AuraCost += this.Mana;
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00004598 File Offset: 0x00002798
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x000045A6 File Offset: 0x000027A6
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x000045B4 File Offset: 0x000027B4
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			this.SetMana(args.DestinationCard);
		}

		// Token: 0x06000136 RID: 310 RVA: 0x000045C2 File Offset: 0x000027C2
		private void SetMana(Card card)
		{
			base.NotifyActivating();
			card.AuraCost -= this.Mana;
		}

		// Token: 0x06000137 RID: 311 RVA: 0x000045E4 File Offset: 0x000027E4
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

		// Token: 0x06000138 RID: 312 RVA: 0x00004648 File Offset: 0x00002848
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
