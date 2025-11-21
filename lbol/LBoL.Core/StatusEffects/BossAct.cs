using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x0200008C RID: 140
	[UsedImplicitly]
	public sealed class BossAct : StatusEffect
	{
		// Token: 0x1700026A RID: 618
		// (get) Token: 0x0600072A RID: 1834 RVA: 0x000155FF File Offset: 0x000137FF
		// (set) Token: 0x0600072B RID: 1835 RVA: 0x00015607 File Offset: 0x00013807
		private int CriticalLife { get; set; }

		// Token: 0x0600072C RID: 1836 RVA: 0x00015610 File Offset: 0x00013810
		protected override void OnAdded(Unit unit)
		{
			this.CriticalLife = base.Owner.MaxHp / 4;
			base.Count = this.CriticalLife;
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnDamageReceived));
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(unit.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalTotalDamageReceived));
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x00015684 File Offset: 0x00013884
		private void OnDamageTaking(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt() - base.Count - base.Owner.Shield - base.Owner.Block;
			if (num > 0)
			{
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
				base.NotifyActivating();
				args.AddModifier(this);
			}
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x000156EC File Offset: 0x000138EC
		private void OnDamageReceived(DamageEventArgs args)
		{
			base.Count -= args.DamageInfo.Damage.RoundToInt();
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x00015719 File Offset: 0x00013919
		private IEnumerable<BattleAction> OnStatisticalTotalDamageReceived(StatisticalDamageEventArgs args)
		{
			if (base.Count == 0)
			{
				Unit owner = base.Owner;
				EnemyUnit enemy = owner as EnemyUnit;
				if (enemy != null)
				{
					yield return PerformAction.Wait(1f, false);
					EnemyUnit enemyUnit = enemy;
					int num = enemyUnit.TurnCounter + 1;
					enemyUnit.TurnCounter = num;
					yield return new EnemyTurnAction(enemy);
					enemy.UpdateTurnMoves();
				}
				base.Count = this.CriticalLife;
				enemy = null;
			}
			yield break;
		}
	}
}
