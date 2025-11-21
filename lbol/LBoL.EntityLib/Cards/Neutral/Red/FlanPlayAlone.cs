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

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C5 RID: 709
	[UsedImplicitly]
	public sealed class FlanPlayAlone : Card
	{
		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000AD5 RID: 2773 RVA: 0x00016319 File Offset: 0x00014519
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x0001631C File Offset: 0x0001451C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CardType != CardType.Attack));
			yield return new DiscardManyAction(list);
			yield break;
		}
	}
}
