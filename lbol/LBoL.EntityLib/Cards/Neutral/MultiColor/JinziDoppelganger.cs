using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.MultiColor;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002ED RID: 749
	[UsedImplicitly]
	public sealed class JinziDoppelganger : Card
	{
		// Token: 0x06000B31 RID: 2865 RVA: 0x000169C6 File Offset: 0x00014BC6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<JinziDoppelgangerSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
