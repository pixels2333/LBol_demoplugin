using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000475 RID: 1141
	[UsedImplicitly]
	public sealed class KoishiKiss : Card
	{
		// Token: 0x06000F52 RID: 3922 RVA: 0x0001B7E1 File Offset: 0x000199E1
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Value1 > 0)
			{
				EnemyUnit selectedEnemy = selector.SelectedEnemy;
				if (selectedEnemy.IsAlive)
				{
					yield return base.DebuffAction<Weak>(selectedEnemy, 0, base.Value1, 0, 0, true, 0.2f);
				}
			}
			yield return new FollowAttackAction(selector, false);
			yield break;
		}
	}
}
