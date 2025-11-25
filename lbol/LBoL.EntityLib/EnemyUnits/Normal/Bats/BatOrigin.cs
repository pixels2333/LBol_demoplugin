using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.EnemyUnits.Normal.Bats
{
	[UsedImplicitly]
	public abstract class BatOrigin : EnemyUnit
	{
		private BatOrigin.MoveType Last { get; set; }
		private BatOrigin.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			if (Enumerable.All<BatOrigin>(Enumerable.OfType<BatOrigin>(base.AllAliveEnemies), (BatOrigin enemy) => enemy.RootIndex <= base.RootIndex))
			{
				this.Next = BatOrigin.MoveType.LockOn;
				this.Last = BatOrigin.MoveType.Defend;
			}
			else
			{
				this.Next = ((base.RootIndex % 2 == 0) ? BatOrigin.MoveType.Shoot : BatOrigin.MoveType.DoubleShoot);
				this.Last = this.Next;
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		protected virtual IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Vampire>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<Graze>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Vampire>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<Graze>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case BatOrigin.MoveType.Shoot:
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
				break;
			case BatOrigin.MoveType.DoubleShoot:
				enemyMove = base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", false);
				break;
			case BatOrigin.MoveType.Defend:
				enemyMove = base.DefendMove(this, base.GetMove(2), 0, 0, base.Defend, true, PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1));
				break;
			case BatOrigin.MoveType.LockOn:
			{
				string move = base.GetMove(3);
				Type typeFromHandle = typeof(LockedOn);
				int? num = new int?(base.Power);
				PerformAction performAction = PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
				enemyMove = base.NegativeMove(move, typeFromHandle, num, default(int?), false, false, performAction);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			if (Enumerable.All<BatOrigin>(Enumerable.OfType<BatOrigin>(base.AllAliveEnemies), (BatOrigin enemy) => enemy.RootIndex <= base.RootIndex && base.EnemyMoveRng.NextFloat(0f, 1f) * (float)(Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) - 1) > 0.4f))
			{
				this.Next = BatOrigin.MoveType.LockOn;
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
			this.Last = this.Next;
		}
		private readonly RepeatableRandomPool<BatOrigin.MoveType> _pool = new RepeatableRandomPool<BatOrigin.MoveType>
		{
			{
				BatOrigin.MoveType.Shoot,
				1f
			},
			{
				BatOrigin.MoveType.DoubleShoot,
				1f
			},
			{
				BatOrigin.MoveType.Defend,
				0.5f
			}
		};
		private enum MoveType
		{
			Shoot,
			DoubleShoot,
			Defend,
			LockOn
		}
	}
}
