using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001E5 RID: 485
	[UsedImplicitly]
	public sealed class WaterGirl : EnemyUnit
	{
		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000794 RID: 1940 RVA: 0x00010F28 File Offset: 0x0000F128
		// (set) Token: 0x06000795 RID: 1941 RVA: 0x00010F30 File Offset: 0x0000F130
		private WaterGirl.MoveType Next { get; set; }

		// Token: 0x06000796 RID: 1942 RVA: 0x00010F39 File Offset: 0x0000F139
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = WaterGirl.MoveType.Debuff;
			base.CountDown = 2;
		}

		// Token: 0x06000797 RID: 1943 RVA: 0x00010F49 File Offset: 0x0000F149
		private IEnumerable<BattleAction> DrownActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<Drowning>(base.Battle.Player, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Chat(this, "Chat.WaterGirl".Localize(true), 3f, 0f, 0f, true);
			yield break;
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x00010F59 File Offset: 0x0000F159
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case WaterGirl.MoveType.Attack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case WaterGirl.MoveType.AttackLarge:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, true, "Instant", false);
				break;
			case WaterGirl.MoveType.Debuff:
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.DrownActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x00010F6C File Offset: 0x0000F16C
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = (base.Battle.Player.HasStatusEffect<Drowning>() ? WaterGirl.MoveType.AttackLarge : WaterGirl.MoveType.Debuff);
				base.CountDown = ((base.EnemyMoveRng.NextFloat(0f, 1f) < 0.8f) ? 3 : 2);
				return;
			}
			this.Next = WaterGirl.MoveType.Attack;
		}

		// Token: 0x020006E0 RID: 1760
		private enum MoveType
		{
			// Token: 0x04000905 RID: 2309
			Attack,
			// Token: 0x04000906 RID: 2310
			AttackLarge,
			// Token: 0x04000907 RID: 2311
			Debuff
		}
	}
}
