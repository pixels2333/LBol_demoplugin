using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000467 RID: 1127
	[UsedImplicitly]
	public sealed class InspirationDefense : Card
	{
		// Token: 0x06000F37 RID: 3895 RVA: 0x0001B5E6 File Offset: 0x000197E6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Inspiration>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
