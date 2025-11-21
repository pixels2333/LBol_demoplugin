using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x0200025D RID: 605
	[UsedImplicitly]
	public sealed class ToolBlock : Card
	{
		// Token: 0x060009C6 RID: 2502 RVA: 0x00014EBB File Offset: 0x000130BB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield break;
		}
	}
}
