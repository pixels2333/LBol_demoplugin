using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;

namespace LBoL.Core.Cards
{
	// Token: 0x02000139 RID: 313
	[UsedImplicitly]
	public sealed class Xiaozhuo : Card
	{
		// Token: 0x06000C01 RID: 3073 RVA: 0x000215AC File Offset: 0x0001F7AC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
