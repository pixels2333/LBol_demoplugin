using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Red;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002CA RID: 714
	[UsedImplicitly]
	public sealed class MeihongFire : Card
	{
		// Token: 0x06000AE2 RID: 2786 RVA: 0x00016403 File Offset: 0x00014603
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MeihongFireSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
