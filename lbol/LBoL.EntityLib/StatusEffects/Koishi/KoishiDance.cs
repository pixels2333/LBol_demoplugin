using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000076 RID: 118
	[UsedImplicitly]
	public sealed class KoishiDance : Card
	{
		// Token: 0x0600019A RID: 410 RVA: 0x0000532E File Offset: 0x0000352E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new FollowAttackAction(selector, false);
			yield break;
		}
	}
}
