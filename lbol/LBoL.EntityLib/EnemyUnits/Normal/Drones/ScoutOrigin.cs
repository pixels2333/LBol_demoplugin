using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	// Token: 0x02000207 RID: 519
	[UsedImplicitly]
	public abstract class ScoutOrigin : Drone
	{
		// Token: 0x06000829 RID: 2089 RVA: 0x000120F4 File Offset: 0x000102F4
		protected override void EnterBattle()
		{
			this.Next = (Enumerable.All<ScoutOrigin>(Enumerable.OfType<ScoutOrigin>(base.AllAliveEnemies), (ScoutOrigin enemy) => enemy.RootIndex <= base.RootIndex) ? ScoutOrigin.MoveType.LockOn : ScoutOrigin.MoveType.Shoot);
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x0001212B File Offset: 0x0001032B
		protected override void Stun()
		{
			this.Next = ScoutOrigin.MoveType.Stun;
			base.UpdateTurnMoves();
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x0001213A File Offset: 0x0001033A
		private IEnumerable<BattleAction> LockOn()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return PerformAction.Effect(this, "CameraFlash", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			foreach (BattleAction battleAction in base.NegativeActions(null, typeof(LockedOn), new int?(base.Power), default(int?), false, null))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x0001214A File Offset: 0x0001034A
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case ScoutOrigin.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case ScoutOrigin.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(1), base.Defend, base.Defend, 0, true, null);
				break;
			case ScoutOrigin.MoveType.LockOn:
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.LockOn());
				yield return base.AddCardMove(null, typeof(Xuanguang), 1, EnemyUnit.AddCardZone.Discard, null, false);
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				break;
			case ScoutOrigin.MoveType.Stun:
				yield return new SimpleEnemyMove(Intention.Stun(), base.PerformActions(base.GetMove(3), PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1)));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x0001215C File Offset: 0x0001035C
		protected override void UpdateMoveCounters()
		{
			if (base.HasStatusEffect<Emi>())
			{
				this.Next = ScoutOrigin.MoveType.Stun;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Last = this.Next;
			if (base.CountDown <= 0)
			{
				this.Next = ScoutOrigin.MoveType.LockOn;
				return;
			}
			if (Enumerable.Any<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Terminator) && this._forceDefendWithTerminator)
			{
				this.Next = ScoutOrigin.MoveType.Defend;
				this._forceDefendWithTerminator = false;
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x0600082E RID: 2094 RVA: 0x0001220B File Offset: 0x0001040B
		// (set) Token: 0x0600082F RID: 2095 RVA: 0x00012213 File Offset: 0x00010413
		private ScoutOrigin.MoveType Last { get; set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000830 RID: 2096 RVA: 0x0001221C File Offset: 0x0001041C
		// (set) Token: 0x06000831 RID: 2097 RVA: 0x00012224 File Offset: 0x00010424
		private ScoutOrigin.MoveType Next { get; set; }

		// Token: 0x04000089 RID: 137
		private bool _forceDefendWithTerminator = true;

		// Token: 0x0400008C RID: 140
		private readonly RepeatableRandomPool<ScoutOrigin.MoveType> _pool = new RepeatableRandomPool<ScoutOrigin.MoveType>
		{
			{
				ScoutOrigin.MoveType.Shoot,
				3f
			},
			{
				ScoutOrigin.MoveType.Defend,
				1f
			}
		};

		// Token: 0x0200070E RID: 1806
		private enum MoveType
		{
			// Token: 0x040009C2 RID: 2498
			Shoot,
			// Token: 0x040009C3 RID: 2499
			Defend,
			// Token: 0x040009C4 RID: 2500
			LockOn,
			// Token: 0x040009C5 RID: 2501
			Stun
		}
	}
}
