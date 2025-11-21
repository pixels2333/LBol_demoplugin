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

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000EC RID: 236
	[UsedImplicitly]
	public sealed class AmuletForCard : StatusEffect
	{
		// Token: 0x06000349 RID: 841 RVA: 0x00008AA4 File Offset: 0x00006CA4
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new EventSequencedReactor<CardsAddingToDrawZoneEventArgs>(this.OnCardsAddedToDrawZone));
		}

		// Token: 0x0600034A RID: 842 RVA: 0x00008B25 File Offset: 0x00006D25
		private IEnumerable<BattleAction> OnAddCard(CardsEventArgs args)
		{
			return this.ExileCard(args.Cards);
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00008B33 File Offset: 0x00006D33
		private IEnumerable<BattleAction> OnCardsAddedToDrawZone(CardsAddingToDrawZoneEventArgs args)
		{
			return this.ExileCard(args.Cards);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00008B41 File Offset: 0x00006D41
		private IEnumerable<BattleAction> ExileCard(IEnumerable<Card> cards)
		{
			List<Card> candidate = Enumerable.ToList<Card>(Enumerable.Where<Card>(cards, (Card card) => card.CardType == CardType.Status));
			if (candidate.Count == 0)
			{
				yield break;
			}
			if (base.Level < candidate.Count)
			{
				candidate.RemoveRange(base.Level, candidate.Count - base.Level);
			}
			base.NotifyActivating();
			yield return new ExileManyCardAction(candidate);
			base.Level -= candidate.Count;
			if (base.Level <= 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
