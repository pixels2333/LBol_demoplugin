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
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C8 RID: 712
	[UsedImplicitly]
	public sealed class KongMana : Card
	{
		// Token: 0x06000ADC RID: 2780 RVA: 0x00016364 File Offset: 0x00014564
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count == 1)
			{
				this._oneTargetHand = list[0];
			}
			if (list.Count <= 1)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x000163BC File Offset: 0x000145BC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new ExileCardAction(card);
					yield return new GainManaAction(card.CostToMana(false));
				}
				card = null;
			}
			else if (this._oneTargetHand != null)
			{
				yield return new ExileCardAction(this._oneTargetHand);
				yield return new GainManaAction(this._oneTargetHand.CostToMana(false));
				this._oneTargetHand = null;
			}
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<PManaCard>() });
			yield break;
		}

		// Token: 0x040000F2 RID: 242
		private Card _oneTargetHand;
	}
}
