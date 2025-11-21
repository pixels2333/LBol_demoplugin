using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000465 RID: 1125
	[UsedImplicitly]
	public sealed class GrazeFollowAttack : Card
	{
		// Token: 0x06000F2E RID: 3886 RVA: 0x0001B55A File Offset: 0x0001975A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield return new FollowAttackAction(selector, false);
			yield break;
		}
	}
}
