using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	// Token: 0x0200020A RID: 522
	[UsedImplicitly]
	public abstract class TerminatorOrigin : Drone
	{
		// Token: 0x06000836 RID: 2102 RVA: 0x00012282 File Offset: 0x00010482
		protected override void EnterBattle()
		{
			this.Next = TerminatorOrigin.MoveType.ShootAccuracy;
			base.CountDown = 4;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x000122AF File Offset: 0x000104AF
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<DroneBlock>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x000122C0 File Offset: 0x000104C0
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Appliance>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<DroneBlock>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x00012345 File Offset: 0x00010545
		protected override void Stun()
		{
			this.Next = TerminatorOrigin.MoveType.Stun;
			base.UpdateTurnMoves();
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00012354 File Offset: 0x00010554
		private IEnumerable<BattleAction> RepairActions()
		{
			base.CountDown = 4;
			yield return PerformAction.Chat(this, "Chat.Terminator0".Localize(true), 1.3f, 0.2f, 0f, true);
			yield return PerformAction.Animation(this, "defend", 1.5f, null, 0f, -1);
			int num = base.MaxHp - base.Hp;
			if (num == 0)
			{
				yield return PerformAction.Chat(this, "Chat.Terminator1".Localize(true), 2.5f, 0.2f, 0f, true);
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(2), default(int?), default(int?), default(int?), 1f, true);
			}
			else if (num < base.Count1 / 2)
			{
				yield return PerformAction.Chat(this, "Chat.Terminator2".Localize(true), 2.5f, 0.2f, 0f, true);
				yield return new HealAction(this, this, base.Count1 / 2, HealType.Normal, 1f);
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return PerformAction.Chat(this, "Chat.Terminator3".Localize(true), 2.5f, 0.2f, 0f, true);
				yield return new HealAction(this, this, base.Count1, HealType.Normal, 1f);
			}
			yield break;
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x00012364 File Offset: 0x00010564
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case TerminatorOrigin.MoveType.TripleShoot:
				enemyMove = base.AttackMove(base.GetMove(1), base.Gun2, base.Damage1, 3, false, "Instant", false);
				break;
			case TerminatorOrigin.MoveType.ShootAccuracy:
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage2, 1, true, "Instant", false);
				break;
			case TerminatorOrigin.MoveType.Repair:
				enemyMove = new SimpleEnemyMove(Intention.Repair(), this.RepairActions());
				break;
			case TerminatorOrigin.MoveType.Stun:
				enemyMove = new SimpleEnemyMove(Intention.Stun(), base.PerformActions(base.GetMove(3), PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1)));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			yield break;
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x00012374 File Offset: 0x00010574
		protected override void UpdateMoveCounters()
		{
			if (base.HasStatusEffect<Emi>())
			{
				this.Next = TerminatorOrigin.MoveType.Stun;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Last = this.Next;
			if (base.CountDown <= 0)
			{
				this.Next = TerminatorOrigin.MoveType.Repair;
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x0600083D RID: 2109 RVA: 0x000123E0 File Offset: 0x000105E0
		// (set) Token: 0x0600083E RID: 2110 RVA: 0x000123E8 File Offset: 0x000105E8
		private TerminatorOrigin.MoveType Last { get; set; }

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600083F RID: 2111 RVA: 0x000123F1 File Offset: 0x000105F1
		// (set) Token: 0x06000840 RID: 2112 RVA: 0x000123F9 File Offset: 0x000105F9
		private TerminatorOrigin.MoveType Next { get; set; }

		// Token: 0x0400008D RID: 141
		private const int RepairInterval = 4;

		// Token: 0x0400008E RID: 142
		private const float ResultChatTime = 2.5f;

		// Token: 0x04000091 RID: 145
		private readonly RepeatableRandomPool<TerminatorOrigin.MoveType> _pool = new RepeatableRandomPool<TerminatorOrigin.MoveType>
		{
			{
				TerminatorOrigin.MoveType.TripleShoot,
				1f
			},
			{
				TerminatorOrigin.MoveType.ShootAccuracy,
				1f
			}
		};

		// Token: 0x02000712 RID: 1810
		private enum MoveType
		{
			// Token: 0x040009D2 RID: 2514
			TripleShoot,
			// Token: 0x040009D3 RID: 2515
			ShootAccuracy,
			// Token: 0x040009D4 RID: 2516
			Repair,
			// Token: 0x040009D5 RID: 2517
			Stun
		}
	}
}
