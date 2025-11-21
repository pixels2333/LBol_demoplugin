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
	// Token: 0x0200020D RID: 525
	[UsedImplicitly]
	public abstract class BatOrigin : EnemyUnit
	{
		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000846 RID: 2118 RVA: 0x0001245D File Offset: 0x0001065D
		// (set) Token: 0x06000847 RID: 2119 RVA: 0x00012465 File Offset: 0x00010665
		private BatOrigin.MoveType Last { get; set; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000848 RID: 2120 RVA: 0x0001246E File Offset: 0x0001066E
		// (set) Token: 0x06000849 RID: 2121 RVA: 0x00012476 File Offset: 0x00010676
		private BatOrigin.MoveType Next { get; set; }

		// Token: 0x0600084A RID: 2122 RVA: 0x00012480 File Offset: 0x00010680
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

		// Token: 0x0600084B RID: 2123 RVA: 0x000124F9 File Offset: 0x000106F9
		protected virtual IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Vampire>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<Graze>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x0001250C File Offset: 0x0001070C
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Vampire>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<Graze>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x00012591 File Offset: 0x00010791
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

		// Token: 0x0600084E RID: 2126 RVA: 0x000125A4 File Offset: 0x000107A4
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

		// Token: 0x04000094 RID: 148
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

		// Token: 0x02000717 RID: 1815
		private enum MoveType
		{
			// Token: 0x040009EA RID: 2538
			Shoot,
			// Token: 0x040009EB RID: 2539
			DoubleShoot,
			// Token: 0x040009EC RID: 2540
			Defend,
			// Token: 0x040009ED RID: 2541
			LockOn
		}
	}
}
