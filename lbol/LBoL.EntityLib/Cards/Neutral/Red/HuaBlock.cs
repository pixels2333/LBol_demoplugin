using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C7 RID: 711
	[UsedImplicitly]
	public sealed class HuaBlock : Card
	{
		// Token: 0x06000ADA RID: 2778 RVA: 0x0001634C File Offset: 0x0001454C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
