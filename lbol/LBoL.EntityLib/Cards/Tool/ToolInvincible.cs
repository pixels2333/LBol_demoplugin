using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x02000263 RID: 611
	[UsedImplicitly]
	public sealed class ToolInvincible : Card
	{
		// Token: 0x060009D4 RID: 2516 RVA: 0x00014F9B File Offset: 0x0001319B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
			yield break;
		}
	}
}
