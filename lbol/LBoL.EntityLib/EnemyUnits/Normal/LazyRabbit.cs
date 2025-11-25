using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class LazyRabbit : EnemyUnit
	{
		private LazyRabbit.MoveType Next { get; set; }
		private bool HardworkRabbitAlive
		{
			get
			{
				EnemyUnit hardworkRabbit = this._hardworkRabbit;
				return hardworkRabbit != null && hardworkRabbit.IsAlive;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this._hardworkRabbit = Enumerable.FirstOrDefault<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is HardworkRabbit);
			this.Next = (this.HardworkRabbitAlive ? LazyRabbit.MoveType.DoNothing : LazyRabbit.MoveType.Attack);
			this.BaseDamage = base.Damage1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnEnemyDied));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<InvincibleEternal>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			if (base.IsAlive && arg.Unit is HardworkRabbit)
			{
				if (!base.IsInTurn)
				{
					this.UpdateMoveCounters();
					base.UpdateTurnMoves();
				}
				yield return PerformAction.Chat(this, "Chat.LazyRabbit".Localize(true), 3f, 0f, 1f, false);
			}
			yield break;
		}
		private int BaseDamage { get; set; }
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case LazyRabbit.MoveType.DoNothing:
				yield return new SimpleEnemyMove(Intention.DoNothing().WithMoveName(base.GetMove(0)), this.DoNothingActions());
				break;
			case LazyRabbit.MoveType.Attack:
				yield return new SimpleEnemyMove(Intention.Attack(this.BaseDamage, 2, false).WithMoveName(base.GetMove(1)), this.MoonRabbitMove());
				yield return new SimpleEnemyMove(Intention.Attack(this.BaseDamage * 2, true), this.MoonRabbitAttack());
				break;
			case LazyRabbit.MoveType.Buff:
				yield return new SimpleEnemyMove(Intention.PositiveEffect().WithMoveName(base.GetMove(2)), this.BuffActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		private IEnumerable<BattleAction> MoonRabbitMove()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield break;
		}
		private IEnumerable<BattleAction> MoonRabbitAttack()
		{
			List<DamageAction> damageActions = new List<DamageAction>();
			DamageAction damageAction = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)this.BaseDamage, false), base.Gun1, GunType.First);
			damageActions.Add(damageAction);
			yield return damageAction;
			if (base.Battle.BattleShouldEnd || base.IsNotAlive)
			{
				yield break;
			}
			DamageAction damageAction2 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)this.BaseDamage, false), "Instant", GunType.Middle);
			damageActions.Add(damageAction2);
			yield return damageAction2;
			if (base.Battle.BattleShouldEnd || base.IsNotAlive)
			{
				yield break;
			}
			DamageAction damageAction3 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)(this.BaseDamage * 2), true), "Instant", GunType.Last);
			damageActions.Add(damageAction3);
			yield return damageAction3;
			yield return new StatisticalTotalDamageAction(damageActions);
			this.BaseDamage++;
			yield break;
		}
		private IEnumerable<BattleAction> DoNothingActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield return PerformAction.Animation(this, "shoot2", 1f, null, 0f, -1);
			yield break;
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				EnemyTuanzi statusEffect = enemyUnit.GetStatusEffect<EnemyTuanzi>();
				if (statusEffect != null && statusEffect.Level >= base.Count1)
				{
					yield return new ApplyStatusEffectAction<Firepower>(enemyUnit, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
				}
				else
				{
					yield return new ApplyStatusEffectAction<EnemyTuanzi>(enemyUnit, new int?(1), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			LazyRabbit.MoveType moveType;
			switch (base.TurnCounter % 3)
			{
			case 0:
				moveType = (this.HardworkRabbitAlive ? LazyRabbit.MoveType.DoNothing : LazyRabbit.MoveType.Attack);
				break;
			case 1:
				moveType = LazyRabbit.MoveType.Attack;
				break;
			case 2:
				moveType = LazyRabbit.MoveType.Buff;
				break;
			default:
				moveType = this.Next;
				break;
			}
			this.Next = moveType;
		}
		private EnemyUnit _hardworkRabbit;
		private enum MoveType
		{
			DoNothing,
			Attack,
			Buff
		}
	}
}
