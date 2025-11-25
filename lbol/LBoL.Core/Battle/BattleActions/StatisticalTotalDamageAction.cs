using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class StatisticalTotalDamageAction : EventBattleAction<StatisticalDamageEventArgs>
	{
		private static int UnitOrder(Unit unit)
		{
			int num;
			if (unit != null)
			{
				if (!(unit is PlayerUnit))
				{
					EnemyUnit enemyUnit = unit as EnemyUnit;
					if (enemyUnit == null)
					{
						throw new ArgumentOutOfRangeException("unit", unit, null);
					}
					num = enemyUnit.Index + 100;
				}
				else
				{
					num = 1;
				}
			}
			else
			{
				num = 0;
			}
			return num;
		}
		public StatisticalTotalDamageAction(IEnumerable<DamageAction> allBattleActions)
		{
			Unit unit = null;
			SortedDictionary<Unit, IReadOnlyList<DamageEventArgs>> sortedDictionary = new SortedDictionary<Unit, IReadOnlyList<DamageEventArgs>>(new StatisticalTotalDamageAction.UnitComparer());
			foreach (DamageEventArgs damageEventArgs in Enumerable.SelectMany<DamageAction, DamageEventArgs>(allBattleActions, (DamageAction a) => a.DamageArgs))
			{
				if (!damageEventArgs.IsCanceled)
				{
					if (unit == null)
					{
						unit = damageEventArgs.Source;
					}
					IReadOnlyList<DamageEventArgs> readOnlyList;
					if (!sortedDictionary.TryGetValue(damageEventArgs.Target, ref readOnlyList))
					{
						readOnlyList = new List<DamageEventArgs>();
						sortedDictionary.Add(damageEventArgs.Target, readOnlyList);
					}
					((List<DamageEventArgs>)readOnlyList).Add(damageEventArgs);
				}
			}
			base.Args = new StatisticalDamageEventArgs
			{
				DamageSource = unit,
				ArgsTable = sortedDictionary,
				CanCancel = false
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.StatisticalTotalDamage(base.Args.DamageSource, base.Args.ArgsTable);
			}, true);
			yield return base.CreatePhase("Dealt", delegate
			{
				base.Args.DamageSource.StatisticalTotalDamageDealt.Execute(base.Args);
			}, false);
			foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in base.Args.ArgsTable)
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				keyValuePair.Deconstruct(ref unit, ref readOnlyList);
				Unit target = unit;
				yield return base.CreatePhase("Received", delegate
				{
					target.StatisticalTotalDamageReceived.Execute(this.Args);
				}, false);
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
		internal static IEnumerable<BattleAction> WrapReactorWithStats(IEnumerable<BattleAction> reactor, List<DamageAction> list)
		{
			foreach (BattleAction battleAction in reactor)
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					list.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		private class UnitComparer : IComparer<Unit>
		{
			public int Compare(Unit a, Unit b)
			{
				return StatisticalTotalDamageAction.UnitOrder(a).CompareTo(StatisticalTotalDamageAction.UnitOrder(b));
			}
		}
	}
}
