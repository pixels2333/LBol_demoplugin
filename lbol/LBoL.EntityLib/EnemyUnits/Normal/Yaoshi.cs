using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class Yaoshi : EnemyUnit
	{
		private Yaoshi.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.RootIndex % 2 == 0) ? Yaoshi.MoveType.Shoot : Yaoshi.MoveType.Buff);
			base.CountDown = base.EnemyMoveRng.NextInt(1, 2);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<RockHard>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				if (!(enemyUnit is Yaoshi) && !enemyUnit.HasStatusEffect<RockHardAura>())
				{
					yield return new ApplyStatusEffectAction<RockHardAura>(enemyUnit, new int?(base.Count2), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			base.React(this.SpawnIntoBattle());
		}
		private IEnumerable<BattleAction> SpawnIntoBattle()
		{
			yield return new ApplyStatusEffectAction<RockHard>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				if (!(enemyUnit is Yaoshi) && !enemyUnit.HasStatusEffect<RockHardAura>())
				{
					yield return new ApplyStatusEffectAction<RockHardAura>(enemyUnit, new int?(base.Count2), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
		protected override void OnDie()
		{
			base.React(this.DieReactor());
		}
		private IEnumerable<BattleAction> DieReactor()
		{
			if (Enumerable.Any<EnemyUnit>(base.AllAliveEnemies) && !Enumerable.Any<Yaoshi>(Enumerable.OfType<Yaoshi>(base.AllAliveEnemies)))
			{
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					if (enemyUnit.HasStatusEffect<RockHardAura>())
					{
						yield return new RemoveStatusEffectAction(enemyUnit.GetStatusEffect<RockHardAura>(), true, 0.1f);
					}
				}
				IEnumerator<EnemyUnit> enumerator = null;
			}
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Yaoshi.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
				break;
			case Yaoshi.MoveType.Buff:
			{
				string move = base.GetMove(1);
				Type typeFromHandle = typeof(Electric);
				int? num = new int?(base.Power);
				PerformAction performAction = PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
				yield return base.PositiveMove(move, typeFromHandle, num, default(int?), false, performAction);
				break;
			}
			case Yaoshi.MoveType.Lightning:
				yield return base.AttackMove(base.GetMove(2), base.Gun2, base.Damage2, 1, false, "Instant", false);
				yield return base.AddCardMove(null, Library.CreateCards<Xuanguang>(2, false), EnemyUnit.AddCardZone.Discard, null, false);
				break;
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
				this.Next = Yaoshi.MoveType.Lightning;
				base.CountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			Yaoshi.MoveType moveType;
			switch (this.Next)
			{
			case Yaoshi.MoveType.Shoot:
				moveType = Yaoshi.MoveType.Buff;
				break;
			case Yaoshi.MoveType.Buff:
				moveType = Yaoshi.MoveType.Shoot;
				break;
			case Yaoshi.MoveType.Lightning:
				moveType = Yaoshi.MoveType.Shoot;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}
		private enum MoveType
		{
			Shoot,
			Buff,
			Lightning
		}
	}
}
