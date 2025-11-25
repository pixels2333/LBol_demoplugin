using System;
using System.Collections.Generic;
using System.Linq;
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
	[UsedImplicitly]
	public sealed class JiangshenSe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(1);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new EventSequencedReactor<CardsAddingToDrawZoneEventArgs>(this.OnCardsAddedToDrawZone));
		}
		private IEnumerable<BattleAction> OnAddCard(CardsEventArgs args)
		{
			yield return this.Upgrade(args.Cards);
			yield break;
		}
		private IEnumerable<BattleAction> OnCardsAddedToDrawZone(CardsAddingToDrawZoneEventArgs args)
		{
			yield return this.Upgrade(args.Cards);
			yield break;
		}
		private BattleAction Upgrade(IEnumerable<Card> cards)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(cards, (Card card) => card.CanUpgradeAndPositive));
			if (list.Count == 0)
			{
				return null;
			}
			base.NotifyActivating();
			return new UpgradeCardsAction(list);
		}
	}
}
