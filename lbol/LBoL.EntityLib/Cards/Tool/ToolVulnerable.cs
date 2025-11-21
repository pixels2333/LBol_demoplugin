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
	// Token: 0x02000269 RID: 617
	[UsedImplicitly]
	public sealed class ToolVulnerable : Card
	{
		// Token: 0x060009E0 RID: 2528 RVA: 0x0001502B File Offset: 0x0001322B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<Vulnerable>(selector.GetEnemy(base.Battle), 0, base.Value1, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
