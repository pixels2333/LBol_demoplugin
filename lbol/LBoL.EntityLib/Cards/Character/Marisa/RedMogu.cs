using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200043F RID: 1087
	[UsedImplicitly]
	public sealed class RedMogu : Card
	{
		// Token: 0x06000ED6 RID: 3798 RVA: 0x0001B00D File Offset: 0x0001920D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<Poison>(selector.SelectedEnemy, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
