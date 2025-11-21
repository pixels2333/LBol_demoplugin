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
	// Token: 0x0200042D RID: 1069
	[UsedImplicitly]
	public sealed class MarisaEat : Card
	{
		// Token: 0x06000E9E RID: 3742 RVA: 0x0001AB01 File Offset: 0x00018D01
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Concentration>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.HealAction(base.Value2);
			yield break;
		}
	}
}
