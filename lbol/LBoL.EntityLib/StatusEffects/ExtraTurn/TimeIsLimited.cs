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
	[UsedImplicitly]
	public sealed class TimeIsLimited : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(base.Level);
			}
		}
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
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				card.AuraCost -= this.Mana;
			}
		}
		private void OnAddCard(CardsEventArgs args)
		{
			Card[] cards = args.Cards;
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].AuraCost += this.Mana;
			}
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			Card[] cards = args.Cards;
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].AuraCost += this.Mana;
			}
		}
		private void OnCardTransformed(CardTransformEventArgs args)
		{
			args.DestinationCard.AuraCost += this.Mana;
		}
		private IEnumerable<BattleAction> OnAllEnemyTurnStarting(GameEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
