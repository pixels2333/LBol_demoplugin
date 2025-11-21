using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200043C RID: 1084
	[UsedImplicitly]
	public sealed class ReadyForBurst : Card
	{
		// Token: 0x06000ECE RID: 3790 RVA: 0x0001AF5D File Offset: 0x0001915D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Charging>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
