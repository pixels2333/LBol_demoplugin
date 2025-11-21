using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000BB RID: 187
	public sealed class MaoyuBlock : StatusEffect
	{
		// Token: 0x06000291 RID: 657 RVA: 0x000072A1 File Offset: 0x000054A1
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnOwnerStatisticalTotalDamageReceived));
		}

		// Token: 0x06000292 RID: 658 RVA: 0x000072C0 File Offset: 0x000054C0
		private IEnumerable<BattleAction> OnOwnerStatisticalTotalDamageReceived(StatisticalDamageEventArgs totalArgs)
		{
			if (!base.Owner.IsAlive)
			{
				yield break;
			}
			foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in totalArgs.ArgsTable)
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				keyValuePair.Deconstruct(ref unit, ref readOnlyList);
				IReadOnlyList<DamageEventArgs> readOnlyList2 = readOnlyList;
				foreach (DamageEventArgs damageEventArgs in readOnlyList2)
				{
					DamageInfo damageInfo = damageEventArgs.DamageInfo;
					if (damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f)
					{
						base.NotifyActivating();
						yield return new CastBlockShieldAction(base.Owner, new BlockInfo(base.Level, BlockShieldType.Direct), false);
						yield return new RemoveStatusEffectAction(this, true, 0.1f);
						yield break;
					}
				}
				IEnumerator<DamageEventArgs> enumerator2 = null;
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
	}
}
