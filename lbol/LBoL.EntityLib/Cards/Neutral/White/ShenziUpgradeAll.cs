using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x0200027C RID: 636
	[UsedImplicitly]
	public sealed class ShenziUpgradeAll : Card
	{
		// Token: 0x06000A12 RID: 2578 RVA: 0x000153E6 File Offset: 0x000135E6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.EnumerateAllCards(), (Card c) => c.CanUpgradeAndPositive && c != this));
			if (list.Count > 0)
			{
				yield return new UpgradeCardsAction(list);
			}
			yield break;
		}
	}
}
