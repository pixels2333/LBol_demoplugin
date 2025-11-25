using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	public abstract class DollBase : EnemyUnit
	{
		private DollBase.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.RootIndex % 2 == 0) ? DollBase.MoveType.Shoot : DollBase.MoveType.PoisonShoot);
			base.CountDown = base.EnemyMoveRng.NextInt(1, 3);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<DeathPoison>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<DeathPoison>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case DollBase.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case DollBase.MoveType.PoisonShoot:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2 + base.EnemyBattleRng.NextInt(0, 2), 1, true, "Instant", false);
				yield return base.NegativeMove(null, typeof(Poison), new int?(base.Power), default(int?), true, false, null);
				break;
			case DollBase.MoveType.Buff:
			{
				string move = base.GetMove(2);
				Type typeFromHandle = typeof(DeathPoison);
				int? num = new int?(base.Count1);
				PerformAction performAction = PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
				yield return base.PositiveMove(move, typeFromHandle, num, default(int?), false, performAction);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = DollBase.MoveType.Buff;
				base.CountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			DollBase.MoveType moveType;
			switch (this.Next)
			{
			case DollBase.MoveType.Shoot:
				moveType = DollBase.MoveType.PoisonShoot;
				break;
			case DollBase.MoveType.PoisonShoot:
				moveType = DollBase.MoveType.Shoot;
				break;
			case DollBase.MoveType.Buff:
				moveType = DollBase.MoveType.Shoot;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}
		private enum MoveType
		{
			Shoot,
			PoisonShoot,
			Buff
		}
	}
}
