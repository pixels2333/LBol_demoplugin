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

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x0200027E RID: 638
	[UsedImplicitly]
	public sealed class YoumuAttack : Card
	{
		// Token: 0x06000A17 RID: 2583 RVA: 0x0001542C File Offset: 0x0001362C
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

		// Token: 0x06000A18 RID: 2584 RVA: 0x00015484 File Offset: 0x00013684
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new ExileCardAction(card);
				}
			}
			else if (this._oneTargetHand != null)
			{
				yield return new ExileCardAction(this._oneTargetHand);
				this._oneTargetHand = null;
			}
			yield break;
		}

		// Token: 0x040000ED RID: 237
		private Card _oneTargetHand;
	}
}
