using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x02000265 RID: 613
	[UsedImplicitly]
	public sealed class ToolMaxHp : Card
	{
		// Token: 0x060009D8 RID: 2520 RVA: 0x00014FCB File Offset: 0x000131CB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
			yield break;
		}
	}
}
