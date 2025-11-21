using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x02000261 RID: 609
	[UsedImplicitly]
	public sealed class ToolFirstAid : Card
	{
		// Token: 0x060009D0 RID: 2512 RVA: 0x00014F6B File Offset: 0x0001316B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new RemoveAllNegativeStatusEffectAction(base.Battle.Player, 0.2f);
			yield return base.HealAction(base.Value1);
			yield break;
		}
	}
}
