using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001E6 RID: 486
	[UsedImplicitly]
	public sealed class WhiteFairy : EnemyUnit
	{
		// Token: 0x170000BF RID: 191
		// (get) Token: 0x0600079B RID: 1947 RVA: 0x00010FE4 File Offset: 0x0000F1E4
		// (set) Token: 0x0600079C RID: 1948 RVA: 0x00010FEC File Offset: 0x0000F1EC
		private WhiteFairy.MoveType Next { get; set; }

		// Token: 0x0600079D RID: 1949 RVA: 0x00010FF5 File Offset: 0x0000F1F5
		protected override void OnEnterBattle(BattleController battle)
		{
			this.EnterBattle();
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x00011000 File Offset: 0x0000F200
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.EnterBattle();
			if (spawner is Clownpiece)
			{
				this.React(new ApplyStatusEffectAction<Lunatic>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			}
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x00011056 File Offset: 0x0000F256
		private void EnterBattle()
		{
			this.Next = ((base.RootIndex % 2 == 0) ? WhiteFairy.MoveType.DoubleShoot : WhiteFairy.MoveType.ShootAndDefend);
			base.CountDown = ((base.RootIndex % 2 == 0) ? 3 : 2);
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x00011080 File Offset: 0x0000F280
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if (!base.HasStatusEffect<Lunatic>())
			{
				switch (this.Next)
				{
				case WhiteFairy.MoveType.ShootAndDefend:
					yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
					yield return base.DefendMove(this, null, base.Defend, 0, 0, false, null);
					break;
				case WhiteFairy.MoveType.DoubleShoot:
					yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", false);
					break;
				case WhiteFairy.MoveType.DefendAndBuff:
					yield return base.DefendMove(this, base.GetMove(2), base.Defend, 0, 0, true, null);
					yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				switch (this.Next)
				{
				case WhiteFairy.MoveType.ShootAndDefend:
					yield return base.AttackMove(base.GetMove(0), base.Gun3, base.Damage1, 1, false, "Instant", false);
					yield return base.DefendMove(this, null, 0, base.Defend, 0, false, null);
					break;
				case WhiteFairy.MoveType.DoubleShoot:
					yield return base.AttackMove(base.GetMove(1), base.Gun4, base.Damage2, 2, false, "Instant", false);
					break;
				case WhiteFairy.MoveType.DefendAndBuff:
					yield return base.DefendMove(this, base.GetMove(2), 0, base.Defend, 0, true, null);
					yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			yield break;
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x00011090 File Offset: 0x0000F290
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = WhiteFairy.MoveType.DefendAndBuff;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			WhiteFairy.MoveType moveType;
			switch (this.Next)
			{
			case WhiteFairy.MoveType.ShootAndDefend:
				moveType = WhiteFairy.MoveType.DoubleShoot;
				break;
			case WhiteFairy.MoveType.DoubleShoot:
				moveType = WhiteFairy.MoveType.ShootAndDefend;
				break;
			case WhiteFairy.MoveType.DefendAndBuff:
				moveType = this._pool.Sample(base.EnemyMoveRng);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}

		// Token: 0x0400007C RID: 124
		private readonly RepeatableRandomPool<WhiteFairy.MoveType> _pool = new RepeatableRandomPool<WhiteFairy.MoveType>
		{
			{
				WhiteFairy.MoveType.ShootAndDefend,
				1f
			},
			{
				WhiteFairy.MoveType.DoubleShoot,
				1f
			}
		};

		// Token: 0x020006E3 RID: 1763
		private enum MoveType
		{
			// Token: 0x04000911 RID: 2321
			ShootAndDefend,
			// Token: 0x04000912 RID: 2322
			DoubleShoot,
			// Token: 0x04000913 RID: 2323
			DefendAndBuff
		}
	}
}
