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
	internal interface ILouguanJian : INotifyActivating
	{
		BattleController Battle { get; }
		Unit LouguanJianOwner { get; }
		int Multiplier { get; }
		void OnTriggered()
		{
		}
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
