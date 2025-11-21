using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	// Token: 0x020001CC RID: 460
	[UsedImplicitly]
	public sealed class Alice : EnemyUnit
	{
		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060006A7 RID: 1703 RVA: 0x0000F21A File Offset: 0x0000D41A
		// (set) Token: 0x060006A8 RID: 1704 RVA: 0x0000F222 File Offset: 0x0000D422
		private Alice.MoveType Next { get; set; }

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060006A9 RID: 1705 RVA: 0x0000F22B File Offset: 0x0000D42B
		// (set) Token: 0x060006AA RID: 1706 RVA: 0x0000F233 File Offset: 0x0000D433
		private int DefendCountDown { get; set; }

		// Token: 0x060006AB RID: 1707 RVA: 0x0000F23C File Offset: 0x0000D43C
		protected override void OnEnterBattle(BattleController battle)
		{
			this.DefendCountDown = 2;
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x0000F248 File Offset: 0x0000D448
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0000F290 File Offset: 0x0000D490
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Alice.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case Alice.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, false, "Instant", false);
				break;
			case Alice.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(2), base.Defend, 0, 0, true, null);
				yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x0000F2A0 File Offset: 0x0000D4A0
		protected override void UpdateMoveCounters()
		{
			int num = this.DefendCountDown - 1;
			this.DefendCountDown = num;
			if (this.DefendCountDown < 1)
			{
				this.Next = Alice.MoveType.Defend;
				this.DefendCountDown = 3;
				return;
			}
			switch (this.Next)
			{
			case Alice.MoveType.DoubleShoot:
				this.Next = Alice.MoveType.Shoot;
				return;
			case Alice.MoveType.Shoot:
				this.Next = Alice.MoveType.DoubleShoot;
				return;
			case Alice.MoveType.Defend:
				this.Next = this._pool.Sample(base.EnemyMoveRng);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x04000044 RID: 68
		private const int DefendTurnAmount = 3;

		// Token: 0x04000046 RID: 70
		private readonly RepeatableRandomPool<Alice.MoveType> _pool = new RepeatableRandomPool<Alice.MoveType>
		{
			{
				Alice.MoveType.Shoot,
				1f
			},
			{
				Alice.MoveType.DoubleShoot,
				1f
			}
		};

		// Token: 0x02000681 RID: 1665
		private enum MoveType
		{
			// Token: 0x04000774 RID: 1908
			DoubleShoot,
			// Token: 0x04000775 RID: 1909
			Shoot,
			// Token: 0x04000776 RID: 1910
			Defend
		}
	}
}
