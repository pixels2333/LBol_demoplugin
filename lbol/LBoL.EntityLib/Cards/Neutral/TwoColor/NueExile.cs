using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200029D RID: 669
	[UsedImplicitly]
	public sealed class NueExile : Card
	{
		// Token: 0x06000A6C RID: 2668 RVA: 0x00015B0C File Offset: 0x00013D0C
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count == 1)
			{
				this.oneTargetHand = list[0];
			}
			if (list.Count <= 1)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00015B64 File Offset: 0x00013D64
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new ExileCardAction(card);
					Card card2 = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), null);
					card2.SetTurnCost(base.Mana);
					yield return new AddCardsToHandAction(new Card[] { card2 });
				}
			}
			else if (this.oneTargetHand != null)
			{
				yield return new ExileCardAction(this.oneTargetHand);
				Card card3 = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), null);
				card3.SetTurnCost(base.Mana);
				yield return new AddCardsToHandAction(new Card[] { card3 });
				this.oneTargetHand = null;
			}
			yield break;
		}

		// Token: 0x040000F0 RID: 240
		private Card oneTargetHand;
	}
}
