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

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001BB RID: 443
	[UsedImplicitly]
	public sealed class Dahuoji : Exhibit
	{
		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000662 RID: 1634 RVA: 0x0000EB47 File Offset: 0x0000CD47
		public override string OverrideIconName
		{
			get
			{
				if (base.Counter != 0)
				{
					return base.Id + "Inactive";
				}
				return base.Id;
			}
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0000EB68 File Offset: 0x0000CD68
		protected override void OnEnterBattle()
		{
			base.Counter = 0;
			base.ReactBattleEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
			base.Active = true;
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0000EB9A File Offset: 0x0000CD9A
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
						Unit unit3 = unit2;
						int? num = new int?(base.Value1);
						yield return new ApplyStatusEffectAction<Vulnerable>(unit3, default(int?), num, default(int?), default(int?), 0f, true);
						base.Counter = 1;
						base.Blackout = true;
						base.Active = false;
					}
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0000EBB1 File Offset: 0x0000CDB1
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
