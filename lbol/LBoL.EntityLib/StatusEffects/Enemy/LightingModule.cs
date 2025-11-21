using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000B2 RID: 178
	public sealed class LightingModule : StatusEffect
	{
		// Token: 0x0600026D RID: 621 RVA: 0x00006EDE File Offset: 0x000050DE
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00006EFD File Offset: 0x000050FD
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
						yield return new AddCardsToDrawZoneAction(Library.CreateCards<Xingguang>(num * base.Level, false), DrawZoneTarget.Random, AddCardsType.Normal);
					}
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
	}
}
