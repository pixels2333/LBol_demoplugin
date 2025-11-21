using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000169 RID: 361
	[UsedImplicitly]
	public sealed class Guangjian : Exhibit
	{
		// Token: 0x060004FF RID: 1279 RVA: 0x0000CA08 File Offset: 0x0000AC08
		protected override void OnEnterBattle()
		{
			base.Counter = 0;
			base.ReactBattleEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
			base.Active = true;
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0000CA3A File Offset: 0x0000AC3A
		private IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || base.Counter == 1)
			{
				yield break;
			}
			bool activated = false;
			foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				keyValuePair.Deconstruct(ref unit, ref readOnlyList);
				Unit unit2 = unit;
				IReadOnlyList<DamageEventArgs> readOnlyList2 = readOnlyList;
				if (unit2.IsAlive)
				{
					if (Enumerable.Count<DamageEventArgs>(readOnlyList2, delegate(DamageEventArgs damageAgs)
					{
						DamageInfo damageInfo = damageAgs.DamageInfo;
						return damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f;
					}) > 0)
					{
						if (!activated)
						{
							base.NotifyActivating();
							activated = true;
						}
						yield return new ApplyStatusEffectAction<LockedOn>(unit2, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
						base.Counter = 1;
						base.Active = false;
						base.Blackout = true;
					}
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x0000CA51 File Offset: 0x0000AC51
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
