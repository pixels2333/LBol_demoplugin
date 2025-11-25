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
	public sealed class BailianBlackSe : StatusEffect
	{
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
		protected override void OnAdded(Unit unit)
		{
			this.SetManaAndExile(base.Battle.EnumerateAllCards());
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnCardsAddedToDrawZone));
		}
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetManaAndExile(args.Cards);
		}
		private void OnCardsAddedToDrawZone(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetManaAndExile(args.Cards);
		}
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
