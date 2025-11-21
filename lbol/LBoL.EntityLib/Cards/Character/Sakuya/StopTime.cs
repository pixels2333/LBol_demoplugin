using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003BD RID: 957
	[UsedImplicitly]
	public sealed class StopTime : Card
	{
		// Token: 0x06000D84 RID: 3460 RVA: 0x0001964B File Offset: 0x0001784B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<StopTimeSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
