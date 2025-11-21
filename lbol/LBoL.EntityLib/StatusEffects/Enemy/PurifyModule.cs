using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000BF RID: 191
	public sealed class PurifyModule : StatusEffect
	{
		// Token: 0x0600029E RID: 670 RVA: 0x000073F5 File Offset: 0x000055F5
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}

		// Token: 0x0600029F RID: 671 RVA: 0x00007414 File Offset: 0x00005614
		private IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
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
						return damageInfo.DamageType == DamageType.Attack && !damageInfo.IsGrazed;
					});
					if (num > 0)
					{
						base.NotifyActivating();
						yield return new ApplyStatusEffectAction<TurnStartPurify>(unit2, new int?(num * base.Level), default(int?), default(int?), default(int?), 0f, true);
					}
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
	}
}
