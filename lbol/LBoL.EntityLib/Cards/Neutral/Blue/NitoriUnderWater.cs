using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x0200031B RID: 795
	[UsedImplicitly]
	public sealed class NitoriUnderWater : Card
	{
		// Token: 0x06000BBD RID: 3005 RVA: 0x0001762D File Offset: 0x0001582D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
