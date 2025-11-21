using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200045E RID: 1118
	[UsedImplicitly]
	public sealed class DefenseFollowAttack : Card
	{
		// Token: 0x06000F1C RID: 3868 RVA: 0x0001B43B File Offset: 0x0001963B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new FollowAttackAction(selector, false);
			yield break;
		}
	}
}
