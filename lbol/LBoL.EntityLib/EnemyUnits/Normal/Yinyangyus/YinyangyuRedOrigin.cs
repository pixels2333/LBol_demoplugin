using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus
{
	public abstract class YinyangyuRedOrigin : EnemyUnit
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.RootIndex % 2 == 0)
			{
				this.Next = YinyangyuRedOrigin.MoveType.Shoot;
				base.CountDown = 0;
			}
			else
			{
				this.Next = YinyangyuRedOrigin.MoveType.DoubleShoot;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<AbsorbPower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<AbsorbPower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			YinyangyuRedOrigin.MoveType next = this.Next;
			IEnemyMove enemyMove;
			if (next != YinyangyuRedOrigin.MoveType.Shoot)
			{
				if (next != YinyangyuRedOrigin.MoveType.DoubleShoot)
				{
					throw new ArgumentOutOfRangeException();
				}
				enemyMove = base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", false);
			}
			else
			{
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield return enemyMove;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = YinyangyuRedOrigin.MoveType.DoubleShoot;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				return;
			}
			this.Next = YinyangyuRedOrigin.MoveType.Shoot;
		}
		private YinyangyuRedOrigin.MoveType Next { get; set; }
		private enum MoveType
		{
			Shoot,
			DoubleShoot
		}
	}
}
