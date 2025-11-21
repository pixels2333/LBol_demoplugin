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
	// Token: 0x02000378 RID: 888
	[UsedImplicitly]
	public sealed class DebugExileDrawDiscard : Card
	{
		// Token: 0x06000CB5 RID: 3253 RVA: 0x0001884B File Offset: 0x00016A4B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			IEnumerable<Card> enumerable = Enumerable.Concat<Card>(base.Battle.DrawZone, base.Battle.DiscardZone);
			yield return new ExileManyCardAction(enumerable);
			yield break;
		}
	}
}
