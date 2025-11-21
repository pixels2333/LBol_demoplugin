using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	// Token: 0x02000204 RID: 516
	[UsedImplicitly]
	public abstract class PurifierOrigin : Drone
	{
		// Token: 0x0600081E RID: 2078 RVA: 0x00011FF9 File Offset: 0x000101F9
		protected override void EnterBattle()
		{
			this.Next = ((base.RootIndex % 2 == 0) ? PurifierOrigin.MoveType.Shoot : PurifierOrigin.MoveType.Purify);
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x0001200F File Offset: 0x0001020F
		protected override void Stun()
		{
			this.Next = PurifierOrigin.MoveType.Stun;
			base.UpdateTurnMoves();
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x0001201E File Offset: 0x0001021E
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case PurifierOrigin.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case PurifierOrigin.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(1), base.Defend, base.Defend, 0, true, null);
				break;
			case PurifierOrigin.MoveType.Purify:
				yield return base.AttackMove(base.GetMove(2), base.Gun2, base.Damage2, 1, false, "Instant", false);
				yield return base.NegativeMove(null, typeof(TurnStartPurify), new int?(base.Power), default(int?), true, false, null);
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				break;
			case PurifierOrigin.MoveType.Stun:
				yield return new SimpleEnemyMove(Intention.Stun(), base.PerformActions(base.GetMove(3), PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1)));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x00012030 File Offset: 0x00010230
		protected override void UpdateMoveCounters()
		{
			if (base.HasStatusEffect<Emi>())
			{
				this.Next = PurifierOrigin.MoveType.Stun;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Last = this.Next;
			this.Next = ((base.CountDown <= 0) ? PurifierOrigin.MoveType.Purify : this._pool.Without(this.Last).Sample(base.EnemyMoveRng));
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000822 RID: 2082 RVA: 0x00012097 File Offset: 0x00010297
		// (set) Token: 0x06000823 RID: 2083 RVA: 0x0001209F File Offset: 0x0001029F
		private PurifierOrigin.MoveType Last { get; set; }

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000824 RID: 2084 RVA: 0x000120A8 File Offset: 0x000102A8
		// (set) Token: 0x06000825 RID: 2085 RVA: 0x000120B0 File Offset: 0x000102B0
		private PurifierOrigin.MoveType Next { get; set; }

		// Token: 0x04000088 RID: 136
		private readonly RepeatableRandomPool<PurifierOrigin.MoveType> _pool = new RepeatableRandomPool<PurifierOrigin.MoveType>
		{
			{
				PurifierOrigin.MoveType.Shoot,
				2f
			},
			{
				PurifierOrigin.MoveType.Defend,
				1f
			}
		};

		// Token: 0x0200070C RID: 1804
		private enum MoveType
		{
			// Token: 0x040009B9 RID: 2489
			Shoot,
			// Token: 0x040009BA RID: 2490
			Defend,
			// Token: 0x040009BB RID: 2491
			Purify,
			// Token: 0x040009BC RID: 2492
			Stun
		}
	}
}
