using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x0200025B RID: 603
	[UsedImplicitly]
	public sealed class ToolAmulet : Card
	{
		// Token: 0x060009C2 RID: 2498 RVA: 0x00014E84 File Offset: 0x00013084
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Amulet>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
