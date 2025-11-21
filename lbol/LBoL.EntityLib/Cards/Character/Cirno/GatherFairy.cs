using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004BD RID: 1213
	[UsedImplicitly]
	public sealed class GatherFairy : Card
	{
		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06001017 RID: 4119 RVA: 0x0001C87C File Offset: 0x0001AA7C
		[UsedImplicitly]
		public int MaxHand
		{
			get
			{
				return 12;
			}
		}

		// Token: 0x06001018 RID: 4120 RVA: 0x0001C880 File Offset: 0x0001AA80
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MaxHandSe>(0, 0, 0, 0, 0.2f);
			yield return new DrawManyCardAction(base.Value1);
			if (base.Value2 > 0)
			{
				using (IEnumerator<Card> enumerator = Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card2 = enumerator.Current;
						card2.NotifyActivating();
						card2.Loyalty += base.Value2;
					}
					yield break;
				}
			}
			yield break;
		}
	}
}
