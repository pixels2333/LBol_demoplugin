using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x02000068 RID: 104
	[UsedImplicitly]
	public sealed class ManaFreezed : StatusEffect
	{
		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000166 RID: 358 RVA: 0x00004B53 File Offset: 0x00002D53
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000167 RID: 359 RVA: 0x00004B60 File Offset: 0x00002D60
		// (set) Token: 0x06000168 RID: 360 RVA: 0x00004B68 File Offset: 0x00002D68
		private bool Active { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000169 RID: 361 RVA: 0x00004B71 File Offset: 0x00002D71
		// (set) Token: 0x0600016A RID: 362 RVA: 0x00004B79 File Offset: 0x00002D79
		private int ActiveLevel { get; set; }

		// Token: 0x0600016B RID: 363 RVA: 0x00004B84 File Offset: 0x00002D84
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

		// Token: 0x0600016C RID: 364 RVA: 0x00004C64 File Offset: 0x00002E64
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

		// Token: 0x0600016D RID: 365 RVA: 0x00004CA8 File Offset: 0x00002EA8
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

		// Token: 0x0600016E RID: 366 RVA: 0x00004CEB File Offset: 0x00002EEB
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			if (this.Active)
			{
				args.DestinationCard.AuraCost += this.Mana;
			}
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00004D14 File Offset: 0x00002F14
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

		// Token: 0x06000170 RID: 368 RVA: 0x00004D9C File Offset: 0x00002F9C
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

		// Token: 0x06000171 RID: 369 RVA: 0x00004E18 File Offset: 0x00003018
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
