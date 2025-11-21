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
	// Token: 0x02000262 RID: 610
	[UsedImplicitly]
	public sealed class ToolHeal : Card
	{
		// Token: 0x060009D2 RID: 2514 RVA: 0x00014F83 File Offset: 0x00013183
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new HealAction(base.Battle.Player, base.Battle.Player, base.Value1, HealType.Normal, 0.2f);
			yield break;
		}
	}
}
