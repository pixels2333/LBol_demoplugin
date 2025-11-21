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
	// Token: 0x020001E3 RID: 483
	[UsedImplicitly]
	public sealed class SickGirl : EnemyUnit
	{
		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000788 RID: 1928 RVA: 0x00010DEB File Offset: 0x0000EFEB
		// (set) Token: 0x06000789 RID: 1929 RVA: 0x00010DF3 File Offset: 0x0000EFF3
		private SickGirl.MoveType Next { get; set; }

		// Token: 0x0600078A RID: 1930 RVA: 0x00010DFC File Offset: 0x0000EFFC
		protected override void OnEnterBattle(BattleController battle)
		{
			base.CountDown = base.EnemyMoveRng.NextInt(1, 3);
			this.UpdateMoveCounters();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x00010E34 File Offset: 0x0000F034
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.SickGirl".Localize(true), 2f, 0f, 0f, true);
			yield return new ApplyStatusEffectAction<DeathVulnerable>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x00010E44 File Offset: 0x0000F044
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x00010E54 File Offset: 0x0000F054
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			SickGirl.MoveType next = this.Next;
			if (next != SickGirl.MoveType.Attack)
			{
				if (next != SickGirl.MoveType.Buff)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield break;
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x00010E64 File Offset: 0x0000F064
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = SickGirl.MoveType.Buff;
				base.CountDown = ((base.EnemyMoveRng.NextFloat(0f, 1f) < 0.8f) ? 3 : 2);
				return;
			}
			this.Next = SickGirl.MoveType.Attack;
		}

		// Token: 0x020006DA RID: 1754
		private enum MoveType
		{
			// Token: 0x040008EE RID: 2286
			Attack,
			// Token: 0x040008EF RID: 2287
			Buff
		}
	}
}
