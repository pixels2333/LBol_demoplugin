using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001D3 RID: 467
	public abstract class DollBase : EnemyUnit
	{
		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000716 RID: 1814 RVA: 0x00010273 File Offset: 0x0000E473
		// (set) Token: 0x06000717 RID: 1815 RVA: 0x0001027B File Offset: 0x0000E47B
		private DollBase.MoveType Next { get; set; }

		// Token: 0x06000718 RID: 1816 RVA: 0x00010284 File Offset: 0x0000E484
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.RootIndex % 2 == 0) ? DollBase.MoveType.Shoot : DollBase.MoveType.PoisonShoot);
			base.CountDown = base.EnemyMoveRng.NextInt(1, 3);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x000102D5 File Offset: 0x0000E4D5
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<DeathPoison>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x000102E8 File Offset: 0x0000E4E8
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<DeathPoison>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00010332 File Offset: 0x0000E532
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case DollBase.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case DollBase.MoveType.PoisonShoot:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2 + base.EnemyBattleRng.NextInt(0, 2), 1, true, "Instant", false);
				yield return base.NegativeMove(null, typeof(Poison), new int?(base.Power), default(int?), true, false, null);
				break;
			case DollBase.MoveType.Buff:
			{
				string move = base.GetMove(2);
				Type typeFromHandle = typeof(DeathPoison);
				int? num = new int?(base.Count1);
				PerformAction performAction = PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
				yield return base.PositiveMove(move, typeFromHandle, num, default(int?), false, performAction);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x00010344 File Offset: 0x0000E544
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = DollBase.MoveType.Buff;
				base.CountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			DollBase.MoveType moveType;
			switch (this.Next)
			{
			case DollBase.MoveType.Shoot:
				moveType = DollBase.MoveType.PoisonShoot;
				break;
			case DollBase.MoveType.PoisonShoot:
				moveType = DollBase.MoveType.Shoot;
				break;
			case DollBase.MoveType.Buff:
				moveType = DollBase.MoveType.Shoot;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}

		// Token: 0x020006A1 RID: 1697
		private enum MoveType
		{
			// Token: 0x04000804 RID: 2052
			Shoot,
			// Token: 0x04000805 RID: 2053
			PoisonShoot,
			// Token: 0x04000806 RID: 2054
			Buff
		}
	}
}
