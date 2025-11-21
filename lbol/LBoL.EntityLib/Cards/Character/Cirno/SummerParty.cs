using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004D2 RID: 1234
	[UsedImplicitly]
	public sealed class SummerParty : Card
	{
		// Token: 0x06001059 RID: 4185 RVA: 0x0001CE5F File Offset: 0x0001B05F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<FrostArmor>(base.Value1, 0, 0, 0, 0.2f);
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
			yield break;
		}
	}
}
