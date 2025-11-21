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
	// Token: 0x020001E7 RID: 487
	[UsedImplicitly]
	public sealed class Yaoshi : EnemyUnit
	{
		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060007A3 RID: 1955 RVA: 0x00011140 File Offset: 0x0000F340
		// (set) Token: 0x060007A4 RID: 1956 RVA: 0x00011148 File Offset: 0x0000F348
		private Yaoshi.MoveType Next { get; set; }

		// Token: 0x060007A5 RID: 1957 RVA: 0x00011154 File Offset: 0x0000F354
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.RootIndex % 2 == 0) ? Yaoshi.MoveType.Shoot : Yaoshi.MoveType.Buff);
			base.CountDown = base.EnemyMoveRng.NextInt(1, 2);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x000111A5 File Offset: 0x0000F3A5
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

		// Token: 0x060007A7 RID: 1959 RVA: 0x000111B5 File Offset: 0x0000F3B5
		public override void OnSpawn(EnemyUnit spawner)
		{
			base.React(this.SpawnIntoBattle());
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x000111C3 File Offset: 0x0000F3C3
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

		// Token: 0x060007A9 RID: 1961 RVA: 0x000111D3 File Offset: 0x0000F3D3
		protected override void OnDie()
		{
			base.React(this.DieReactor());
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x000111E1 File Offset: 0x0000F3E1
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

		// Token: 0x060007AB RID: 1963 RVA: 0x000111F1 File Offset: 0x0000F3F1
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

		// Token: 0x060007AC RID: 1964 RVA: 0x00011204 File Offset: 0x0000F404
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

		// Token: 0x020006E5 RID: 1765
		private enum MoveType
		{
			// Token: 0x04000919 RID: 2329
			Shoot,
			// Token: 0x0400091A RID: 2330
			Buff,
			// Token: 0x0400091B RID: 2331
			Lightning
		}
	}
}
