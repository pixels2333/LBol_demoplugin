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
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Cirno.Friend;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000E8 RID: 232
	[UsedImplicitly]
	public sealed class StarFriendSe : StatusEffect
	{
		// Token: 0x0600033F RID: 831 RVA: 0x000089E3 File Offset: 0x00006BE3
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}

		// Token: 0x06000340 RID: 832 RVA: 0x00008A07 File Offset: 0x00006C07
		private IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Unit unit;
			IReadOnlyList<DamageEventArgs> readOnlyList;
			if (args.ActionSource is StarFriend)
			{
				bool activated = false;
				foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
				{
					keyValuePair.Deconstruct(ref unit, ref readOnlyList);
					Unit unit2 = unit;
					IReadOnlyList<DamageEventArgs> readOnlyList2 = readOnlyList;
					if (unit2.IsAlive)
					{
						int num = Enumerable.Count<DamageEventArgs>(readOnlyList2, delegate(DamageEventArgs damageAgs)
						{
							DamageInfo damageInfo = damageAgs.DamageInfo;
							return damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f;
						});
						if (num > 0)
						{
							if (!activated)
							{
								base.NotifyActivating();
								activated = true;
							}
							yield return new ApplyStatusEffectAction<LockedOn>(unit2, new int?(base.Level * num), default(int?), default(int?), default(int?), 0f, true);
						}
					}
				}
				IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			}
			if (args.ActionSource is FairyAllOut)
			{
				bool activated = false;
				foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
				{
					keyValuePair.Deconstruct(ref unit, ref readOnlyList);
					Unit unit3 = unit;
					IReadOnlyList<DamageEventArgs> readOnlyList3 = readOnlyList;
					if (unit3.IsAlive)
					{
						int num2 = Enumerable.Count<DamageEventArgs>(readOnlyList3, delegate(DamageEventArgs damageAgs)
						{
							if (damageAgs.GunName == StarFriend.PassiveGunName)
							{
								DamageInfo damageInfo2 = damageAgs.DamageInfo;
								return damageInfo2.DamageType == DamageType.Attack && damageInfo2.Amount > 0f;
							}
							return false;
						});
						if (num2 > 0)
						{
							if (!activated)
							{
								base.NotifyActivating();
								activated = true;
							}
							yield return new ApplyStatusEffectAction<LockedOn>(unit3, new int?(base.Level * num2), default(int?), default(int?), default(int?), 0f, true);
						}
					}
				}
				IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
