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

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003DC RID: 988
	[UsedImplicitly]
	public sealed class JingshenYishi : Card
	{
		// Token: 0x06000DDB RID: 3547 RVA: 0x00019CA4 File Offset: 0x00017EA4
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

		// Token: 0x06000DDC RID: 3548 RVA: 0x00019CFC File Offset: 0x00017EFC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
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

		// Token: 0x04000106 RID: 262
		private Card _oneTargetHand;
	}
}
