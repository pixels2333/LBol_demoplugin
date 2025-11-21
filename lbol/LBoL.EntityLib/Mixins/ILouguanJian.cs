using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Mixins
{
	// Token: 0x0200010C RID: 268
	internal interface ILouguanJian : INotifyActivating
	{
		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060003AB RID: 939
		BattleController Battle { get; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060003AC RID: 940
		Unit LouguanJianOwner { get; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060003AD RID: 941
		int Multiplier { get; }

		// Token: 0x060003AE RID: 942 RVA: 0x0000A529 File Offset: 0x00008729
		void OnTriggered()
		{
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0000A52B File Offset: 0x0000872B
		IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
		{
			if (this.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				keyValuePair.Deconstruct(ref unit, ref readOnlyList);
				Unit unit2 = unit;
				IReadOnlyList<DamageEventArgs> readOnlyList2 = readOnlyList;
				if (unit2.IsAlive)
				{
					int num = Enumerable.Count<DamageEventArgs>(readOnlyList2, delegate(DamageEventArgs damageAgs)
					{
						DamageInfo damageInfo = damageAgs.DamageInfo;
						return damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f;
					});
					if (num > 0)
					{
						this.NotifyActivating();
						EnemyUnit enemyUnit = this.LouguanJianOwner as EnemyUnit;
						if (enemyUnit != null && enemyUnit != this.Battle.LastAliveEnemy)
						{
							yield return new ApplyStatusEffectAction<EnemyLockedOn>(enemyUnit, new int?(this.Multiplier * num), default(int?), default(int?), default(int?), 0f, true);
						}
						else
						{
							yield return new ApplyStatusEffectAction<LockedOn>(unit2, new int?(this.Multiplier * num), default(int?), default(int?), default(int?), 0f, true);
						}
						this.OnTriggered();
					}
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
	}
}
