using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	[UsedImplicitly]
	public abstract class PurifierOrigin : Drone
	{
		protected override void EnterBattle()
		{
			this.Next = ((base.RootIndex % 2 == 0) ? PurifierOrigin.MoveType.Shoot : PurifierOrigin.MoveType.Purify);
		}
		protected override void Stun()
		{
			this.Next = PurifierOrigin.MoveType.Stun;
			base.UpdateTurnMoves();
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case PurifierOrigin.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case PurifierOrigin.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(1), base.Defend, base.Defend, 0, true, null);
				break;
			case PurifierOrigin.MoveType.Purify:
				yield return base.AttackMove(base.GetMove(2), base.Gun2, base.Damage2, 1, false, "Instant", false);
				yield return base.NegativeMove(null, typeof(TurnStartPurify), new int?(base.Power), default(int?), true, false, null);
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				break;
			case PurifierOrigin.MoveType.Stun:
				yield return new SimpleEnemyMove(Intention.Stun(), base.PerformActions(base.GetMove(3), PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1)));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			if (base.HasStatusEffect<Emi>())
			{
				this.Next = PurifierOrigin.MoveType.Stun;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Last = this.Next;
			this.Next = ((base.CountDown <= 0) ? PurifierOrigin.MoveType.Purify : this._pool.Without(this.Last).Sample(base.EnemyMoveRng));
		}
		private PurifierOrigin.MoveType Last { get; set; }
		private PurifierOrigin.MoveType Next { get; set; }
		private readonly RepeatableRandomPool<PurifierOrigin.MoveType> _pool = new RepeatableRandomPool<PurifierOrigin.MoveType>
		{
			{
				PurifierOrigin.MoveType.Shoot,
				2f
			},
			{
				PurifierOrigin.MoveType.Defend,
				1f
			}
		};
		private enum MoveType
		{
			Shoot,
			Defend,
			Purify,
			Stun
		}
	}
}
