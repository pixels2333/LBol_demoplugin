using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Black
{
	// Token: 0x0200005F RID: 95
	public sealed class BailianBlackSe : StatusEffect
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600014A RID: 330 RVA: 0x0000481F File Offset: 0x00002A1F
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				if (base.Limit != 1)
				{
					return ManaGroup.Anys(1);
				}
				return ManaGroup.Empty;
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00004838 File Offset: 0x00002A38
		protected override void OnAdded(Unit unit)
		{
			this.SetManaAndExile(base.Battle.EnumerateAllCards());
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnCardsAddedToDrawZone));
		}

		// Token: 0x0600014C RID: 332 RVA: 0x000048CA File Offset: 0x00002ACA
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetManaAndExile(args.Cards);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x000048D8 File Offset: 0x00002AD8
		private void OnCardsAddedToDrawZone(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetManaAndExile(args.Cards);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000048E8 File Offset: 0x00002AE8
		private void SetManaAndExile(IEnumerable<Card> cards)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(cards, delegate(Card card)
			{
				CardType cardType = card.CardType;
				return cardType == CardType.Defense || cardType == CardType.Skill || cardType == CardType.Status;
			}));
			if (list.Count > 0)
			{
				base.NotifyActivating();
				foreach (Card card2 in list)
				{
					card2.IsExile = true;
					if (!card2.BaseCost.IsSubset(this.Mana))
					{
						card2.SetBaseCost(this.Mana);
					}
					if (!card2.TurnCost.IsSubset(this.Mana))
					{
						card2.SetTurnCost(this.Mana);
					}
				}
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x000049B0 File Offset: 0x00002BB0
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				this.SetManaAndExile(base.Battle.EnumerateAllCards());
			}
			return flag;
		}
	}
}
