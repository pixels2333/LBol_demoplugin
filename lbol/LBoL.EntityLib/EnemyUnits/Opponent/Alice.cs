using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	[UsedImplicitly]
	public sealed class Alice : EnemyUnit
	{
		private Alice.MoveType Next { get; set; }
		private int DefendCountDown { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.DefendCountDown = 2;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Alice.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case Alice.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, false, "Instant", false);
				break;
			case Alice.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(2), base.Defend, 0, 0, true, null);
				yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = this.DefendCountDown - 1;
			this.DefendCountDown = num;
			if (this.DefendCountDown < 1)
			{
				this.Next = Alice.MoveType.Defend;
				this.DefendCountDown = 3;
				return;
			}
			switch (this.Next)
			{
			case Alice.MoveType.DoubleShoot:
				this.Next = Alice.MoveType.Shoot;
				return;
			case Alice.MoveType.Shoot:
				this.Next = Alice.MoveType.DoubleShoot;
				return;
			case Alice.MoveType.Defend:
				this.Next = this._pool.Sample(base.EnemyMoveRng);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		private const int DefendTurnAmount = 3;
		private readonly RepeatableRandomPool<Alice.MoveType> _pool = new RepeatableRandomPool<Alice.MoveType>
		{
			{
				Alice.MoveType.Shoot,
				1f
			},
			{
				Alice.MoveType.DoubleShoot,
				1f
			}
		};
		private enum MoveType
		{
			DoubleShoot,
			Shoot,
			Defend
		}
	}
}
