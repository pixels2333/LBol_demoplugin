using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.EnemyUnits.Normal.Shenlings
{
	// Token: 0x020001EF RID: 495
	[UsedImplicitly]
	public abstract class Shenling : EnemyUnit
	{
		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060007CE RID: 1998 RVA: 0x00011691 File Offset: 0x0000F891
		// (set) Token: 0x060007CF RID: 1999 RVA: 0x00011699 File Offset: 0x0000F899
		private Shenling.MoveType Next { get; set; }

		// Token: 0x060007D0 RID: 2000 RVA: 0x000116A2 File Offset: 0x0000F8A2
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.RootIndex % 2 == 0) ? Shenling.MoveType.DoubleShoot : Shenling.MoveType.ShootAndBuff);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x000116D6 File Offset: 0x0000F8D6
		protected virtual IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield return new CastBlockShieldAction(this, this, 0, base.MaxHp, BlockShieldType.Normal, false);
			yield break;
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x000116E8 File Offset: 0x0000F8E8
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
			this.React(new CastBlockShieldAction(this, this, 0, base.MaxHp, BlockShieldType.Normal, false));
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x00011748 File Offset: 0x0000F948
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			Shenling.MoveType next = this.Next;
			if (next != Shenling.MoveType.DoubleShoot)
			{
				if (next != Shenling.MoveType.ShootAndBuff)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, false, "Instant", false);
				yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
			}
			yield break;
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x00011758 File Offset: 0x0000F958
		protected override void UpdateMoveCounters()
		{
			this.Next = ((this.Next == Shenling.MoveType.DoubleShoot) ? Shenling.MoveType.ShootAndBuff : Shenling.MoveType.DoubleShoot);
		}

		// Token: 0x020006F6 RID: 1782
		private enum MoveType
		{
			// Token: 0x0400095F RID: 2399
			DoubleShoot,
			// Token: 0x04000960 RID: 2400
			ShootAndBuff
		}
	}
}
