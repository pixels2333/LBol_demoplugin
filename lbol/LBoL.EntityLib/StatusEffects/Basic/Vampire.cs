using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000F8 RID: 248
	[UsedImplicitly]
	public sealed class Vampire : StatusEffect
	{
		// Token: 0x0600037A RID: 890 RVA: 0x00008FC7 File Offset: 0x000071C7
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}

		// Token: 0x0600037B RID: 891 RVA: 0x00008FE6 File Offset: 0x000071E6
		private IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			bool activated = false;
			int totalHeal = 0;
			foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				keyValuePair.Deconstruct(ref unit, ref readOnlyList);
				Unit unit2 = unit;
				foreach (DamageEventArgs damageEventArgs in Enumerable.Where<DamageEventArgs>(Enumerable.Where<DamageEventArgs>(readOnlyList, (DamageEventArgs ags) => ags.DamageInfo.DamageType == DamageType.Attack), (DamageEventArgs amount) => amount.DamageInfo.Damage > 0f))
				{
					totalHeal += damageEventArgs.DamageInfo.Damage.ToInt();
				}
				if (totalHeal > 0 && !activated)
				{
					base.NotifyActivating();
					activated = true;
					yield return new HealAction(unit2, base.Owner, totalHeal, HealType.Vampire, 0f);
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
	}
}
