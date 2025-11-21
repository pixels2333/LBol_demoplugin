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
	// Token: 0x0200026A RID: 618
	[UsedImplicitly]
	public sealed class ToolWeak : Card
	{
		// Token: 0x060009E2 RID: 2530 RVA: 0x0001504A File Offset: 0x0001324A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<Weak>(selector.GetEnemy(base.Battle), 0, base.Value1, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
