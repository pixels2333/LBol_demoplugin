using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class BossAct : StatusEffect
	{
		private int CriticalLife { get; set; }
		protected override void OnAdded(Unit unit)
		{
			this.CriticalLife = base.Owner.MaxHp / 4;
			base.Count = this.CriticalLife;
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnDamageReceived));
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(unit.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalTotalDamageReceived));
		}
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
		private void OnDamageReceived(DamageEventArgs args)
		{
			base.Count -= args.DamageInfo.Damage.RoundToInt();
		}
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
