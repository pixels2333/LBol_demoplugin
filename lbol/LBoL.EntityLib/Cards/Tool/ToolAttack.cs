using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x0200025C RID: 604
	[UsedImplicitly]
	public sealed class ToolAttack : Card
	{
		// Token: 0x060009C4 RID: 2500 RVA: 0x00014E9C File Offset: 0x0001309C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield break;
		}
	}
}
