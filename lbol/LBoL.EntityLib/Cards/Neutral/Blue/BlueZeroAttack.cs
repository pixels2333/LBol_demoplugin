using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000312 RID: 786
	[UsedImplicitly]
	public sealed class BlueZeroAttack : Card
	{
		// Token: 0x06000BA1 RID: 2977 RVA: 0x00017400 File Offset: 0x00015600
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd || base.Value1 <= 0)
			{
				yield break;
			}
			EnemyUnit selectedEnemy = selector.SelectedEnemy;
			if (selectedEnemy.IsAlive)
			{
				yield return base.DebuffAction<Weak>(selectedEnemy, 0, base.Value1, 0, 0, true, 0.2f);
			}
			yield break;
		}
	}
}
