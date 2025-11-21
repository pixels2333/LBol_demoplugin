using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002EC RID: 748
	[UsedImplicitly]
	public sealed class HuiyeSuperExtraTurn : Card
	{
		// Token: 0x06000B2F RID: 2863 RVA: 0x000169AE File Offset: 0x00014BAE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<SuperExtraTurn>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
