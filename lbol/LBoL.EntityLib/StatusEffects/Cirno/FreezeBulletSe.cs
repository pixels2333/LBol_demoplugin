using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	public sealed class FreezeBulletSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageReceived));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnStatisticalDamageReceived(StatisticalDamageEventArgs args)
		{
			if (args.DamageSource != base.Owner && args.DamageSource.IsAlive)
			{
				foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
				{
					Unit unit;
					IReadOnlyList<DamageEventArgs> readOnlyList;
					keyValuePair.Deconstruct(ref unit, ref readOnlyList);
					Unit unit2 = unit;
					IReadOnlyList<DamageEventArgs> readOnlyList2 = readOnlyList;
					if (unit2 == base.Owner)
					{
						if (Enumerable.Any<DamageEventArgs>(readOnlyList2, (DamageEventArgs damage) => damage.DamageInfo.DamageType == DamageType.Attack))
						{
							base.NotifyActivating();
							yield return PerformAction.Gun(base.Owner, args.DamageSource, "冻结弹幕", 0f);
							yield return new ApplyStatusEffectAction<Cold>(args.DamageSource, default(int?), default(int?), default(int?), default(int?), 0f, true);
							int num = base.Level - 1;
							base.Level = num;
							if (base.Level == 0)
							{
								yield return new RemoveStatusEffectAction(this, true, 0.1f);
							}
						}
					}
				}
				IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			}
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
