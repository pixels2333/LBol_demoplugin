using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004D3 RID: 1235
	[UsedImplicitly]
	public sealed class Tether : Card
	{
		// Token: 0x0600105B RID: 4187 RVA: 0x0001CE77 File Offset: 0x0001B077
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			using (IEnumerator<Card> enumerator = Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card2 = enumerator.Current;
					card2.NotifyActivating();
					card2.Loyalty++;
				}
				yield break;
			}
			yield break;
		}
	}
}
