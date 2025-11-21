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
	// Token: 0x02000266 RID: 614
	[UsedImplicitly]
	public sealed class ToolMicai : Card
	{
		// Token: 0x060009DA RID: 2522 RVA: 0x00014FE3 File Offset: 0x000131E3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<GuangxueMicai>(0, base.Value1, 0, 0, 0.2f);
			yield break;
		}
	}
}
