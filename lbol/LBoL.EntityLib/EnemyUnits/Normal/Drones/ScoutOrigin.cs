using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	[UsedImplicitly]
	public abstract class ScoutOrigin : Drone
	{
		protected override void EnterBattle()
		{
			this.Next = (Enumerable.All<ScoutOrigin>(Enumerable.OfType<ScoutOrigin>(base.AllAliveEnemies), (ScoutOrigin enemy) => enemy.RootIndex <= base.RootIndex) ? ScoutOrigin.MoveType.LockOn : ScoutOrigin.MoveType.Shoot);
		}
		protected override void Stun()
		{
			this.Next = ScoutOrigin.MoveType.Stun;
			base.UpdateTurnMoves();
		}
		private IEnumerable<BattleAction> LockOn()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return PerformAction.Effect(this, "CameraFlash", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			foreach (BattleAction battleAction in base.NegativeActions(null, typeof(LockedOn), new int?(base.Power), default(int?), false, null))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case ScoutOrigin.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case ScoutOrigin.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(1), base.Defend, base.Defend, 0, true, null);
				break;
			case ScoutOrigin.MoveType.LockOn:
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.LockOn());
				yield return base.AddCardMove(null, typeof(Xuanguang), 1, EnemyUnit.AddCardZone.Discard, null, false);
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				break;
			case ScoutOrigin.MoveType.Stun:
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
				this.Next = ScoutOrigin.MoveType.Stun;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Last = this.Next;
			if (base.CountDown <= 0)
			{
				this.Next = ScoutOrigin.MoveType.LockOn;
				return;
			}
			if (Enumerable.Any<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Terminator) && this._forceDefendWithTerminator)
			{
				this.Next = ScoutOrigin.MoveType.Defend;
				this._forceDefendWithTerminator = false;
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
		}
		private ScoutOrigin.MoveType Last { get; set; }
		private ScoutOrigin.MoveType Next { get; set; }
		private bool _forceDefendWithTerminator = true;
		private readonly RepeatableRandomPool<ScoutOrigin.MoveType> _pool = new RepeatableRandomPool<ScoutOrigin.MoveType>
		{
			{
				ScoutOrigin.MoveType.Shoot,
				3f
			},
			{
				ScoutOrigin.MoveType.Defend,
				1f
			}
		};
		private enum MoveType
		{
			Shoot,
			Defend,
			LockOn,
			Stun
		}
	}
}
