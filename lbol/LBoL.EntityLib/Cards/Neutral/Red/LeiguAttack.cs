using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C9 RID: 713
	[UsedImplicitly]
	public sealed class LeiguAttack : Card
	{
		// Token: 0x06000AE0 RID: 2784 RVA: 0x000163E4 File Offset: 0x000145E4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<EnemyUnit> enemiesInShield = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(selector.GetEnemies(base.Battle), (EnemyUnit enemy) => enemy.Block > 0 || enemy.Shield > 0));
			yield return base.AttackAction(selector, null);
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(enemiesInShield, (EnemyUnit enemy) => enemy.IsAlive));
			if (list.Count > 0)
			{
				yield return base.AttackAction(list, "Instant");
			}
			yield break;
		}
	}
}
