using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class SickGirl : EnemyUnit
	{
		private SickGirl.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			base.CountDown = base.EnemyMoveRng.NextInt(1, 3);
			this.UpdateMoveCounters();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.SickGirl".Localize(true), 2f, 0f, 0f, true);
			yield return new ApplyStatusEffectAction<DeathVulnerable>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			SickGirl.MoveType next = this.Next;
			if (next != SickGirl.MoveType.Attack)
			{
				if (next != SickGirl.MoveType.Buff)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = SickGirl.MoveType.Buff;
				base.CountDown = ((base.EnemyMoveRng.NextFloat(0f, 1f) < 0.8f) ? 3 : 2);
				return;
			}
			this.Next = SickGirl.MoveType.Attack;
		}
		private enum MoveType
		{
			Attack,
			Buff
		}
	}
}
