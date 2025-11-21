using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003E3 RID: 995
	[UsedImplicitly]
	public sealed class MomentPower : Card
	{
		// Token: 0x06000DEF RID: 3567 RVA: 0x00019EE2 File Offset: 0x000180E2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MomentPowerSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
