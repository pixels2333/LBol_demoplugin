using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000339 RID: 825
	[UsedImplicitly]
	public sealed class RemiliaFate : Card
	{
		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000C08 RID: 3080 RVA: 0x00017AEF File Offset: 0x00015CEF
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x00017AF4 File Offset: 0x00015CF4
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value1, list);
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x00017B3B File Offset: 0x00015D3B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				yield return new DiscardManyAction(readOnlyList);
			}
			if (base.Battle.DrawZone.Count > 0)
			{
				int max = (this.IsUpgraded ? 5 : 3);
				RemiliaFate.<>c__DisplayClass3_0 CS$<>8__locals1 = new RemiliaFate.<>c__DisplayClass3_0();
				CS$<>8__locals1.i = 1;
				while (CS$<>8__locals1.i <= max && base.Battle.DrawZone.Count != 0 && !base.Battle.HandIsFull)
				{
					List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => !card.IsForbidden && card.ConfigCost.Amount == CS$<>8__locals1.i));
					if (list.Count > 0)
					{
						Card card2 = list.Sample(base.BattleRng);
						yield return new MoveCardAction(card2, CardZone.Hand);
					}
					int i = CS$<>8__locals1.i;
					CS$<>8__locals1.i = i + 1;
				}
				CS$<>8__locals1 = null;
			}
			yield break;
		}
	}
}
