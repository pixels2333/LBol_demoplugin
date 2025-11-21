using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200009E RID: 158
	public sealed class EnemyLarva : StatusEffect
	{
		// Token: 0x06000236 RID: 566 RVA: 0x000068AC File Offset: 0x00004AAC
		protected override void OnAdded(Unit unit)
		{
			this.React(PerformAction.Sfx("FairySupport", 0f));
			this.React(PerformAction.Effect(base.Owner, "LarvaFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
			this.React(new AddCardsToHandAction(new Card[] { Library.CreateCard<LarvaCure>() }));
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnOwnerStatisticalTotalDamageReceived));
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000693A File Offset: 0x00004B3A
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
				Unit target = unit;
				int num = Enumerable.Count<DamageEventArgs>(readOnlyList, delegate(DamageEventArgs args)
				{
					if (target == this.Owner && args.Source == this.Battle.Player)
					{
						DamageInfo damageInfo = args.DamageInfo;
						return damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f;
					}
					return false;
				});
				if (num > 0)
				{
					base.NotifyActivating();
					yield return new ApplyStatusEffectAction<Poison>(base.Battle.Player, new int?(num * base.Level), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
	}
}
