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

namespace LBoL.EntityLib.StatusEffects.ExtraTurn
{
	// Token: 0x02000085 RID: 133
	[UsedImplicitly]
	public sealed class TimeIsLimited : StatusEffect
	{
		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060001DA RID: 474 RVA: 0x00005A53 File Offset: 0x00003C53
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}

		// Token: 0x060001DB RID: 475 RVA: 0x00005A60 File Offset: 0x00003C60
		protected override void OnAdded(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				card.AuraCost += this.Mana;
			}
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
			base.HandleOwnerEvent<CardTransformEventArgs>(base.Battle.CardTransformed, new GameEventHandler<CardTransformEventArgs>(this.OnCardTransformed));
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnStarting));
		}

		// Token: 0x060001DC RID: 476 RVA: 0x00005B70 File Offset: 0x00003D70
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				foreach (Card card in base.Battle.EnumerateAllCards())
				{
					card.AuraCost += ManaGroup.Anys(other.Level);
				}
			}
			return flag;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x00005BE4 File Offset: 0x00003DE4
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				card.AuraCost -= this.Mana;
			}
		}

		// Token: 0x060001DE RID: 478 RVA: 0x00005C44 File Offset: 0x00003E44
		private void OnAddCard(CardsEventArgs args)
		{
			Card[] cards = args.Cards;
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].AuraCost += this.Mana;
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00005C80 File Offset: 0x00003E80
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			Card[] cards = args.Cards;
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].AuraCost += this.Mana;
			}
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00005CBB File Offset: 0x00003EBB
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			args.DestinationCard.AuraCost += this.Mana;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00005CD9 File Offset: 0x00003ED9
		private IEnumerable<BattleAction> OnAllEnemyTurnStarting(GameEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
