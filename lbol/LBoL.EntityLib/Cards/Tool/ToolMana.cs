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
	// Token: 0x02000264 RID: 612
	[UsedImplicitly]
	public sealed class ToolMana : Card
	{
		// Token: 0x060009D6 RID: 2518 RVA: 0x00014FB3 File Offset: 0x000131B3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
