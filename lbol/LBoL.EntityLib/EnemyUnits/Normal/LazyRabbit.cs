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
	// Token: 0x020001DC RID: 476
	[UsedImplicitly]
	public sealed class LazyRabbit : EnemyUnit
	{
		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000762 RID: 1890 RVA: 0x00010A43 File Offset: 0x0000EC43
		// (set) Token: 0x06000763 RID: 1891 RVA: 0x00010A4B File Offset: 0x0000EC4B
		private LazyRabbit.MoveType Next { get; set; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000764 RID: 1892 RVA: 0x00010A54 File Offset: 0x0000EC54
		private bool HardworkRabbitAlive
		{
			get
			{
				EnemyUnit hardworkRabbit = this._hardworkRabbit;
				return hardworkRabbit != null && hardworkRabbit.IsAlive;
			}
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00010A74 File Offset: 0x0000EC74
		protected override void OnEnterBattle(BattleController battle)
		{
			this._hardworkRabbit = Enumerable.FirstOrDefault<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is HardworkRabbit);
			this.Next = (this.HardworkRabbitAlive ? LazyRabbit.MoveType.DoNothing : LazyRabbit.MoveType.Attack);
			this.BaseDamage = base.Damage1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnEnemyDied));
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00010B09 File Offset: 0x0000ED09
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<InvincibleEternal>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x00010B19 File Offset: 0x0000ED19
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

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000768 RID: 1896 RVA: 0x00010B30 File Offset: 0x0000ED30
		// (set) Token: 0x06000769 RID: 1897 RVA: 0x00010B38 File Offset: 0x0000ED38
		private int BaseDamage { get; set; }

		// Token: 0x0600076A RID: 1898 RVA: 0x00010B41 File Offset: 0x0000ED41
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

		// Token: 0x0600076B RID: 1899 RVA: 0x00010B51 File Offset: 0x0000ED51
		private IEnumerable<BattleAction> MoonRabbitMove()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield break;
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00010B61 File Offset: 0x0000ED61
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

		// Token: 0x0600076D RID: 1901 RVA: 0x00010B71 File Offset: 0x0000ED71
		private IEnumerable<BattleAction> DoNothingActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield return PerformAction.Animation(this, "shoot2", 1f, null, 0f, -1);
			yield break;
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00010B81 File Offset: 0x0000ED81
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

		// Token: 0x0600076F RID: 1903 RVA: 0x00010B94 File Offset: 0x0000ED94
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

		// Token: 0x04000074 RID: 116
		private EnemyUnit _hardworkRabbit;

		// Token: 0x020006C8 RID: 1736
		private enum MoveType
		{
			// Token: 0x040008A4 RID: 2212
			DoNothing,
			// Token: 0x040008A5 RID: 2213
			Attack,
			// Token: 0x040008A6 RID: 2214
			Buff
		}
	}
}
