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

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000343 RID: 835
	[UsedImplicitly]
	public sealed class XiaoemoBook : Card
	{
		// Token: 0x06000C22 RID: 3106 RVA: 0x00017CB9 File Offset: 0x00015EB9
		protected override string GetBaseDescription()
		{
			if (!base.DebutActive)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x06000C23 RID: 3107 RVA: 0x00017CD0 File Offset: 0x00015ED0
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

		// Token: 0x06000C24 RID: 3108 RVA: 0x00017D28 File Offset: 0x00015F28
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
			else if (this._oneTargetHand != null)
			{
				yield return new ExileCardAction(this._oneTargetHand);
				this._oneTargetHand = null;
			}
			yield return new DrawManyCardAction(base.Value1);
			if (base.TriggeredAnyhow)
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<BManaCard>() });
			}
			yield break;
		}

		// Token: 0x040000FC RID: 252
		private Card _oneTargetHand;
	}
}
