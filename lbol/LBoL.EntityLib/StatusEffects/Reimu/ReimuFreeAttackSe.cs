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

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000031 RID: 49
	[UsedImplicitly]
	public sealed class ReimuFreeAttackSe : StatusEffect
	{
		// Token: 0x0600008B RID: 139 RVA: 0x00002EA8 File Offset: 0x000010A8
		protected override void OnAdded(Unit unit)
		{
			this.CardToFree(base.Battle.EnumerateAllCards());
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00002F57 File Offset: 0x00001157
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.CardToFree(args.Cards);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00002F65 File Offset: 0x00001165
		private void OnAddCard(CardsEventArgs args)
		{
			this.CardToFree(args.Cards);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00002F74 File Offset: 0x00001174
		private void CardToFree(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
			{
				if (card.CardType == CardType.Attack)
				{
					card.FreeCost = true;
				}
			}
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00002FC8 File Offset: 0x000011C8
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				if (card.CardType == CardType.Attack)
				{
					card.FreeCost = false;
				}
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003024 File Offset: 0x00001224
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack)
			{
				int num = base.Level - 1;
				base.Level = num;
				if (base.Level <= 0)
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}

		// Token: 0x04000004 RID: 4
		[UsedImplicitly]
		public ManaGroup Mana = ManaGroup.Empty;
	}
}
