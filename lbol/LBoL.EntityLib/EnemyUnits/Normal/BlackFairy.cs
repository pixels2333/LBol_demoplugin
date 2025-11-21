using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001D2 RID: 466
	[UsedImplicitly]
	public sealed class BlackFairy : EnemyUnit
	{
		// Token: 0x170000AA RID: 170
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x00010112 File Offset: 0x0000E312
		// (set) Token: 0x0600070B RID: 1803 RVA: 0x0001011A File Offset: 0x0000E31A
		private BlackFairy.MoveType Next { get; set; }

		// Token: 0x0600070C RID: 1804 RVA: 0x00010123 File Offset: 0x0000E323
		protected override void OnEnterBattle(BattleController battle)
		{
			this.EnterBattle();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x00010148 File Offset: 0x0000E348
		private void EnterBattle()
		{
			this.Next = BlackFairy.MoveType.ShootCards;
			base.CountDown = ((base.RootIndex % 2 == 0) ? 2 : 3);
			this.HighDamageTurn = 1;
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x0001016C File Offset: 0x0000E36C
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new CastBlockShieldAction(this, this, 0, base.Count1, BlockShieldType.Normal, false);
			yield break;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0001017C File Offset: 0x0000E37C
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.EnterBattle();
			if (spawner is Clownpiece)
			{
				this.React(new ApplyStatusEffectAction<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			}
			this.React(new CastBlockShieldAction(this, this, 0, base.Count1, BlockShieldType.Normal, false));
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x000101ED File Offset: 0x0000E3ED
		private IEnumerable<BattleAction> BuffActions()
		{
			if (base.HasStatusEffect<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>())
			{
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
			this.HighDamageTurn = base.EnemyMoveRng.NextInt(1, 3);
			yield break;
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x000101FD File Offset: 0x0000E3FD
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if (!base.HasStatusEffect<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>())
			{
				switch (this.Next)
				{
				case BlackFairy.MoveType.ShootCards:
					yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
					yield return base.AddCardMove(null, Library.CreateCards<BlackResidue>(1, false), EnemyUnit.AddCardZone.Discard, null, false);
					break;
				case BlackFairy.MoveType.ShootAccuracy:
					yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, true, "Instant", false);
					break;
				case BlackFairy.MoveType.DefendBuff:
					yield return base.DefendMove(this, base.GetMove(2), base.Defend, 0, 0, true, null);
					yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				switch (this.Next)
				{
				case BlackFairy.MoveType.ShootCards:
					yield return base.AttackMove(base.GetMove(0), base.Gun3, base.Damage1, 1, false, "Instant", false);
					yield return base.AddCardMove(null, Library.CreateCards<BlackResidue>(2, false), EnemyUnit.AddCardZone.Discard, null, false);
					break;
				case BlackFairy.MoveType.ShootAccuracy:
					yield return base.AttackMove(base.GetMove(1), base.Gun4, base.Damage2 + base.Power, 2, true, "Instant", false);
					break;
				case BlackFairy.MoveType.DefendBuff:
					yield return base.DefendMove(this, base.GetMove(2), 0, base.Defend, 0, true, null);
					yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			yield break;
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000712 RID: 1810 RVA: 0x0001020D File Offset: 0x0000E40D
		// (set) Token: 0x06000713 RID: 1811 RVA: 0x00010215 File Offset: 0x0000E415
		private int HighDamageTurn { get; set; }

		// Token: 0x06000714 RID: 1812 RVA: 0x00010220 File Offset: 0x0000E420
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = BlackFairy.MoveType.DefendBuff;
				return;
			}
			if (base.CountDown == this.HighDamageTurn)
			{
				this.Next = BlackFairy.MoveType.ShootAccuracy;
				return;
			}
			this.Next = BlackFairy.MoveType.ShootCards;
		}

		// Token: 0x0200069D RID: 1693
		private enum MoveType
		{
			// Token: 0x040007F4 RID: 2036
			ShootCards,
			// Token: 0x040007F5 RID: 2037
			ShootAccuracy,
			// Token: 0x040007F6 RID: 2038
			DefendBuff
		}
	}
}
