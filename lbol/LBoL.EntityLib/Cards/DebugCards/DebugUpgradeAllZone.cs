using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.DebugCards
{
	// Token: 0x0200037C RID: 892
	[UsedImplicitly]
	public sealed class DebugUpgradeAllZone : Card
	{
		// Token: 0x06000CC1 RID: 3265 RVA: 0x00018943 File Offset: 0x00016B43
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Take<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => card.CanUpgrade), base.Value1));
			if (Enumerable.Any<Card>(list))
			{
				yield return new UpgradeCardsAction(list);
			}
			list = Enumerable.ToList<Card>(Enumerable.Take<Card>(Enumerable.Where<Card>(base.Battle.DiscardZone, (Card card) => card.CanUpgrade), base.Value1));
			if (Enumerable.Any<Card>(list))
			{
				yield return new UpgradeCardsAction(list);
			}
			list = Enumerable.ToList<Card>(Enumerable.Take<Card>(Enumerable.Where<Card>(base.Battle.ExileZone, (Card card) => card.CanUpgrade), base.Value1));
			if (Enumerable.Any<Card>(list))
			{
				yield return new UpgradeCardsAction(list);
			}
			yield break;
		}
	}
}
