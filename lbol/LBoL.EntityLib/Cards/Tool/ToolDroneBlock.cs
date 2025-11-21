using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x0200025F RID: 607
	[UsedImplicitly]
	public sealed class ToolDroneBlock : Card
	{
		// Token: 0x060009CA RID: 2506 RVA: 0x00014EEB File Offset: 0x000130EB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<DroneBlock>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
