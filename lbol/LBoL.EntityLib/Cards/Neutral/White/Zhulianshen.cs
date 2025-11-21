using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000285 RID: 645
	[UsedImplicitly]
	public sealed class Zhulianshen : Card
	{
		// Token: 0x06000A2E RID: 2606 RVA: 0x0001568E File Offset: 0x0001388E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<AmuletForCard>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
