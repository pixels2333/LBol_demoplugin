using System;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200036D RID: 877
	public sealed class SakuyaLock : Card
	{
		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000C99 RID: 3225 RVA: 0x00018634 File Offset: 0x00016834
		protected override int AdditionalValue1
		{
			get
			{
				BattleController battle = base.Battle;
				EnemyUnit enemyUnit;
				if (battle == null)
				{
					enemyUnit = null;
				}
				else
				{
					enemyUnit = Enumerable.FirstOrDefault<EnemyUnit>(battle.EnemyGroup, (EnemyUnit u) => u is Sakuya);
				}
				EnemyUnit enemyUnit2 = enemyUnit;
				StatusEffect statusEffect;
				if (enemyUnit2 == null)
				{
					statusEffect = null;
				}
				else
				{
					statusEffect = Enumerable.FirstOrDefault<StatusEffect>(enemyUnit2.StatusEffects, (StatusEffect se) => se is PrivateSquare);
				}
				StatusEffect statusEffect2 = statusEffect;
				if (statusEffect2 == null)
				{
					return 0;
				}
				DamageInfo damageInfo = DamageInfo.Attack((float)statusEffect2.Level, false);
				return base.Battle.CalculateDamage(base.Battle.Player, enemyUnit2, base.Battle.Player, damageInfo);
			}
		}
	}
}
