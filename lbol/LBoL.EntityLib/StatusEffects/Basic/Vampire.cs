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
	[UsedImplicitly]
	public sealed class Vampire : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}
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
