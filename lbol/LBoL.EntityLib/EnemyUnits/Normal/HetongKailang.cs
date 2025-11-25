using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class HetongKailang : EnemyUnit
	{
		private HetongKailang.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = HetongKailang.MoveType.Buff;
			base.CountDown = 5;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(base.Count2);
			yield return new ApplyStatusEffectAction<GuangxueMicai>(this, default(int?), num, default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> RepairActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Drone && enemy.MaxHp - enemy.Hp >= base.Count1 / 2));
			if (list.Count >= 1)
			{
				EnemyUnit target = list.Sample(base.EnemyMoveRng);
				yield return PerformAction.Animation(this, "shoot3", 0.8f, "MetalHit3", 0.6f, -1);
				yield return new HealAction(this, target, base.Count1, HealType.Normal, 0.2f);
				if (target.Hp >= target.MaxHp - base.Count1 / 2)
				{
					yield return PerformAction.Chat(this, "Chat.HetongKailang1".Localize(true), 3f, 0.5f, 0f, true);
				}
				else
				{
					yield return PerformAction.Chat(this, "Chat.HetongKailang2".Localize(true), 3f, 0.5f, 0f, true);
				}
				target = null;
			}
			else
			{
				yield return new CastBlockShieldAction(this, this, base.Defend, 0, BlockShieldType.Normal, true);
			}
			yield break;
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Chat(this, "Chat.HetongKailang3".Localize(true), 3f, 0f, 0f, true);
			yield return PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				yield return new ApplyStatusEffectAction<Firepower>(enemyUnit, new int?(base.Power), default(int?), default(int?), default(int?), 0.2f, true);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case HetongKailang.MoveType.Shoot:
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
				break;
			case HetongKailang.MoveType.Repair:
				enemyMove = new SimpleEnemyMove(Intention.Repair(), this.RepairActions());
				break;
			case HetongKailang.MoveType.Buff:
				enemyMove = new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0 && Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) >= 2)
			{
				this.Next = HetongKailang.MoveType.Buff;
				return;
			}
			if (Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Drone && enemy.MaxHp - enemy.Hp >= base.Count1 / 2)).Count >= 1)
			{
				this.Next = HetongKailang.MoveType.Repair;
				return;
			}
			this.Next = HetongKailang.MoveType.Shoot;
		}
		private enum MoveType
		{
			Shoot,
			Repair,
			Buff
		}
	}
}
