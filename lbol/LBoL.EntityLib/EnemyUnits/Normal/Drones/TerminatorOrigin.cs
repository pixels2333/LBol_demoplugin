using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	[UsedImplicitly]
	public abstract class TerminatorOrigin : Drone
	{
		protected override void EnterBattle()
		{
			this.Next = TerminatorOrigin.MoveType.ShootAccuracy;
			base.CountDown = 4;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<DroneBlock>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Appliance>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<DroneBlock>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override void Stun()
		{
			this.Next = TerminatorOrigin.MoveType.Stun;
			base.UpdateTurnMoves();
		}
		private IEnumerable<BattleAction> RepairActions()
		{
			base.CountDown = 4;
			yield return PerformAction.Chat(this, "Chat.Terminator0".Localize(true), 1.3f, 0.2f, 0f, true);
			yield return PerformAction.Animation(this, "defend", 1.5f, null, 0f, -1);
			int num = base.MaxHp - base.Hp;
			if (num == 0)
			{
				yield return PerformAction.Chat(this, "Chat.Terminator1".Localize(true), 2.5f, 0.2f, 0f, true);
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(2), default(int?), default(int?), default(int?), 1f, true);
			}
			else if (num < base.Count1 / 2)
			{
				yield return PerformAction.Chat(this, "Chat.Terminator2".Localize(true), 2.5f, 0.2f, 0f, true);
				yield return new HealAction(this, this, base.Count1 / 2, HealType.Normal, 1f);
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return PerformAction.Chat(this, "Chat.Terminator3".Localize(true), 2.5f, 0.2f, 0f, true);
				yield return new HealAction(this, this, base.Count1, HealType.Normal, 1f);
			}
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case TerminatorOrigin.MoveType.TripleShoot:
				enemyMove = base.AttackMove(base.GetMove(1), base.Gun2, base.Damage1, 3, false, "Instant", false);
				break;
			case TerminatorOrigin.MoveType.ShootAccuracy:
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage2, 1, true, "Instant", false);
				break;
			case TerminatorOrigin.MoveType.Repair:
				enemyMove = new SimpleEnemyMove(Intention.Repair(), this.RepairActions());
				break;
			case TerminatorOrigin.MoveType.Stun:
				enemyMove = new SimpleEnemyMove(Intention.Stun(), base.PerformActions(base.GetMove(3), PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1)));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			if (base.HasStatusEffect<Emi>())
			{
				this.Next = TerminatorOrigin.MoveType.Stun;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Last = this.Next;
			if (base.CountDown <= 0)
			{
				this.Next = TerminatorOrigin.MoveType.Repair;
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
		}
		private TerminatorOrigin.MoveType Last { get; set; }
		private TerminatorOrigin.MoveType Next { get; set; }
		private const int RepairInterval = 4;
		private const float ResultChatTime = 2.5f;
		private readonly RepeatableRandomPool<TerminatorOrigin.MoveType> _pool = new RepeatableRandomPool<TerminatorOrigin.MoveType>
		{
			{
				TerminatorOrigin.MoveType.TripleShoot,
				1f
			},
			{
				TerminatorOrigin.MoveType.ShootAccuracy,
				1f
			}
		};
		private enum MoveType
		{
			TripleShoot,
			ShootAccuracy,
			Repair,
			Stun
		}
	}
}
