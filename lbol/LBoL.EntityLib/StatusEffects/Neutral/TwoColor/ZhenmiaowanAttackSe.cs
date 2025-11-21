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

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000052 RID: 82
	public sealed class ZhenmiaowanAttackSe : StatusEffect
	{
		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000108 RID: 264 RVA: 0x00003D34 File Offset: 0x00001F34
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00003D41 File Offset: 0x00001F41
		protected override string GetBaseDescription()
		{
			if (base.Limit != 1)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00003D5C File Offset: 0x00001F5C
		protected override void OnAdded(Unit unit)
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleOwnerEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00003E28 File Offset: 0x00002028
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				if (base.Limit == 1 || card.CardType == CardType.Skill)
				{
					card.AuraCost += this.Mana;
				}
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00003E9C File Offset: 0x0000209C
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00003EAA File Offset: 0x000020AA
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00003EB8 File Offset: 0x000020B8
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			this.SetMana(args.DestinationCard);
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00003EC6 File Offset: 0x000020C6
		private void SetMana(Card card)
		{
			if (base.Limit == 1 || card.CardType == CardType.Skill)
			{
				base.NotifyActivating();
				card.AuraCost -= this.Mana;
			}
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00003EF8 File Offset: 0x000020F8
		private void SetMana(IEnumerable<Card> cards)
		{
			bool flag = true;
			foreach (Card card in cards)
			{
				if (base.Limit == 1 || card.CardType == CardType.Skill)
				{
					if (flag)
					{
						base.NotifyActivating();
						flag = false;
					}
					card.AuraCost -= this.Mana;
				}
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00003F70 File Offset: 0x00002170
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				IEnumerable<Card> enumerable = base.Battle.EnumerateAllCards();
				bool flag2 = true;
				foreach (Card card in enumerable)
				{
					if (base.Limit == 1 || card.CardType == CardType.Skill)
					{
						if (flag2)
						{
							base.NotifyActivating();
							flag2 = false;
						}
						card.AuraCost -= ManaGroup.Anys(other.Level);
					}
					else if (other.Limit == 1)
					{
						if (flag2)
						{
							base.NotifyActivating();
							flag2 = false;
						}
						card.AuraCost -= ManaGroup.Anys(base.Level + other.Level);
					}
				}
			}
			return flag;
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00004044 File Offset: 0x00002244
		private IEnumerable<BattleAction> OnOwnerTurnEnding(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
