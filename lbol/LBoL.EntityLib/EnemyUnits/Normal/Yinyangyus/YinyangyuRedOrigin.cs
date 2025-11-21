using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus
{
	// Token: 0x020001ED RID: 493
	public abstract class YinyangyuRedOrigin : EnemyUnit
	{
		// Token: 0x060007C5 RID: 1989 RVA: 0x0001155C File Offset: 0x0000F75C
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.RootIndex % 2 == 0)
			{
				this.Next = YinyangyuRedOrigin.MoveType.Shoot;
				base.CountDown = 0;
			}
			else
			{
				this.Next = YinyangyuRedOrigin.MoveType.DoubleShoot;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x000115BA File Offset: 0x0000F7BA
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<AbsorbPower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x000115CC File Offset: 0x0000F7CC
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<AbsorbPower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00011616 File Offset: 0x0000F816
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			YinyangyuRedOrigin.MoveType next = this.Next;
			IEnemyMove enemyMove;
			if (next != YinyangyuRedOrigin.MoveType.Shoot)
			{
				if (next != YinyangyuRedOrigin.MoveType.DoubleShoot)
				{
					throw new ArgumentOutOfRangeException();
				}
				enemyMove = base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", false);
			}
			else
			{
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield return enemyMove;
			yield break;
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x00011628 File Offset: 0x0000F828
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = YinyangyuRedOrigin.MoveType.DoubleShoot;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				return;
			}
			this.Next = YinyangyuRedOrigin.MoveType.Shoot;
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060007CA RID: 1994 RVA: 0x00011670 File Offset: 0x0000F870
		// (set) Token: 0x060007CB RID: 1995 RVA: 0x00011678 File Offset: 0x0000F878
		private YinyangyuRedOrigin.MoveType Next { get; set; }

		// Token: 0x020006F3 RID: 1779
		private enum MoveType
		{
			// Token: 0x04000954 RID: 2388
			Shoot,
			// Token: 0x04000955 RID: 2389
			DoubleShoot
		}
	}
}
