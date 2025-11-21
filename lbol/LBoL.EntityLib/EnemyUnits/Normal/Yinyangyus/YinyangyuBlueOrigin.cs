using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus
{
	// Token: 0x020001EA RID: 490
	public abstract class YinyangyuBlueOrigin : EnemyUnit
	{
		// Token: 0x060007B9 RID: 1977 RVA: 0x00011404 File Offset: 0x0000F604
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.RootIndex % 2 == 0)
			{
				this.Next = YinyangyuBlueOrigin.MoveType.Shoot;
				base.CountDown = 0;
			}
			else
			{
				this.Next = YinyangyuBlueOrigin.MoveType.Defend;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x00011462 File Offset: 0x0000F662
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<AbsorbSpirit>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x00011474 File Offset: 0x0000F674
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<AbsorbSpirit>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x000114BE File Offset: 0x0000F6BE
		private IEnumerable<BattleAction> DefendActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy != this));
			EnemyUnit enemyUnit = this;
			if (list.Count > 0)
			{
				enemyUnit = list.Sample(base.EnemyMoveRng);
			}
			yield return new CastBlockShieldAction(this, enemyUnit, 0, base.Defend, BlockShieldType.Normal, true);
			yield break;
		}

		// Token: 0x060007BD RID: 1981 RVA: 0x000114CE File Offset: 0x0000F6CE
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			YinyangyuBlueOrigin.MoveType next = this.Next;
			IEnemyMove enemyMove;
			if (next != YinyangyuBlueOrigin.MoveType.Shoot)
			{
				if (next != YinyangyuBlueOrigin.MoveType.Defend)
				{
					throw new ArgumentOutOfRangeException();
				}
				enemyMove = new SimpleEnemyMove(Intention.Defend(), this.DefendActions());
			}
			else
			{
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield return enemyMove;
			yield break;
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x000114E0 File Offset: 0x0000F6E0
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = YinyangyuBlueOrigin.MoveType.Defend;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				return;
			}
			this.Next = YinyangyuBlueOrigin.MoveType.Shoot;
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060007BF RID: 1983 RVA: 0x00011528 File Offset: 0x0000F728
		// (set) Token: 0x060007C0 RID: 1984 RVA: 0x00011530 File Offset: 0x0000F730
		private YinyangyuBlueOrigin.MoveType Next { get; set; }

		// Token: 0x020006EF RID: 1775
		private enum MoveType
		{
			// Token: 0x04000945 RID: 2373
			Shoot,
			// Token: 0x04000946 RID: 2374
			Defend
		}
	}
}
