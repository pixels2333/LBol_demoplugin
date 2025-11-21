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
	// Token: 0x020000DD RID: 221
	public sealed class FreezeBulletSe : StatusEffect
	{
		// Token: 0x06000319 RID: 793 RVA: 0x000085AE File Offset: 0x000067AE
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageReceived));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600031A RID: 794 RVA: 0x000085EA File Offset: 0x000067EA
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

		// Token: 0x0600031B RID: 795 RVA: 0x00008601 File Offset: 0x00006801
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
