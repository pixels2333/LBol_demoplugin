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
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003FF RID: 1023
	[UsedImplicitly]
	public sealed class SuikaOut : Card
	{
		// Token: 0x06000E2C RID: 3628 RVA: 0x0001A314 File Offset: 0x00018514
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

		// Token: 0x06000E2D RID: 3629 RVA: 0x0001A36C File Offset: 0x0001856C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new ExileCardAction(card);
				}
			}
			else if (this.oneTargetHand != null)
			{
				yield return new ExileCardAction(this.oneTargetHand);
				this.oneTargetHand = null;
			}
			yield return base.BuffAction<AmuletForCard>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}

		// Token: 0x04000108 RID: 264
		private Card oneTargetHand;
	}
}
