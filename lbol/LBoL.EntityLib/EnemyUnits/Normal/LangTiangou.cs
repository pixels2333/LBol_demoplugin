using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001DB RID: 475
	[UsedImplicitly]
	public sealed class LangTiangou : EnemyUnit
	{
		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600075A RID: 1882 RVA: 0x00010940 File Offset: 0x0000EB40
		// (set) Token: 0x0600075B RID: 1883 RVA: 0x00010948 File Offset: 0x0000EB48
		private LangTiangou.MoveType Next { get; set; }

		// Token: 0x0600075C RID: 1884 RVA: 0x00010951 File Offset: 0x0000EB51
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = LangTiangou.MoveType.AttackDefend;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x00010977 File Offset: 0x0000EB77
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<PowerByDefense>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x00010988 File Offset: 0x0000EB88
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<PowerByDefense>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x000109D2 File Offset: 0x0000EBD2
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case LangTiangou.MoveType.AttackDefend:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage1, 1, false, "Instant", true);
				yield return base.DefendMove(this, null, base.Defend, 0, 0, true, null);
				break;
			case LangTiangou.MoveType.AttackDebuff:
			{
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage2, 1, false, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Vulnerable);
				int? num = new int?(base.Count1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case LangTiangou.MoveType.DoubleAttack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, base.Gun1, true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x000109E4 File Offset: 0x0000EBE4
		protected override void UpdateMoveCounters()
		{
			switch (this.Next)
			{
			case LangTiangou.MoveType.AttackDefend:
				this.Next = LangTiangou.MoveType.AttackDebuff;
				return;
			case LangTiangou.MoveType.AttackDebuff:
				this.Next = ((base.EnemyMoveRng.NextInt(0, 1) == 0) ? LangTiangou.MoveType.DoubleAttack : LangTiangou.MoveType.AttackDefend);
				return;
			case LangTiangou.MoveType.DoubleAttack:
				this.Next = LangTiangou.MoveType.AttackDefend;
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x020006C5 RID: 1733
		private enum MoveType
		{
			// Token: 0x04000898 RID: 2200
			AttackDefend,
			// Token: 0x04000899 RID: 2201
			AttackDebuff,
			// Token: 0x0400089A RID: 2202
			DoubleAttack
		}
	}
}
