using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004FB RID: 1275
	[UsedImplicitly]
	public sealed class NewsPositive : Card
	{
		// Token: 0x060010C2 RID: 4290 RVA: 0x0001D39F File Offset: 0x0001B59F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainPowerAction(base.Value1);
			yield break;
		}
	}
}
