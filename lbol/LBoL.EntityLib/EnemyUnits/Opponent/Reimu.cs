using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	// Token: 0x020001D0 RID: 464
	[UsedImplicitly]
	public sealed class Reimu : EnemyUnit
	{
		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060006EE RID: 1774 RVA: 0x0000FC9C File Offset: 0x0000DE9C
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(5), 6);
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x0000FCAB File Offset: 0x0000DEAB
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x0000FCB3 File Offset: 0x0000DEB3
		private Reimu.MoveType LastAttack { get; set; }

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x0000FCBC File Offset: 0x0000DEBC
		// (set) Token: 0x060006F2 RID: 1778 RVA: 0x0000FCC4 File Offset: 0x0000DEC4
		private Reimu.MoveType Next { get; set; }

		// Token: 0x060006F3 RID: 1779 RVA: 0x0000FCCD File Offset: 0x0000DECD
		protected override void OnEnterBattle(BattleController battle)
		{
			this.LastAttack = Reimu.MoveType.ShootAccuracy;
			this.Next = Reimu.MoveType.Summon;
			base.CountDown = 5;
			this._defendCount = 2;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0000FD08 File Offset: 0x0000DF08
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new CastBlockShieldAction(this, 0, base.Defend, BlockShieldType.Normal, false);
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x0000FD18 File Offset: 0x0000DF18
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new CastBlockShieldAction(this, 0, base.Defend, BlockShieldType.Normal, false));
			this.React(new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x0000FDB2 File Offset: 0x0000DFB2
		private IEnumerable<BattleAction> HakureiDefend()
		{
			yield return new EnemyMoveAction(this, base.GetMove(3), true);
			yield return new CastBlockShieldAction(this, 0, base.Defend + base.EnemyBattleRng.NextInt(0, 2), BlockShieldType.Normal, true);
			if (!base.HasStatusEffect<Amulet>())
			{
				yield return new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0000FDC2 File Offset: 0x0000DFC2
		private IEnumerable<BattleAction> SummonActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(4), true);
			yield return PerformAction.Animation(this, "shoot2", 0.5f, null, 0f, -1);
			if (Enumerable.All<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex != 0))
			{
				yield return new SpawnEnemyAction<YinyangyuRedReimu>(this, 0, 0f, 0.3f, true);
			}
			if (Enumerable.All<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex != 1))
			{
				yield return new SpawnEnemyAction<YinyangyuBlueReimu>(this, 1, 0f, 0.3f, true);
			}
			this._vacancy = 7;
			yield break;
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x0000FDD2 File Offset: 0x0000DFD2
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(this.SpellCard, base.Gun4, base.Damage4, 3, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x0000FDE2 File Offset: 0x0000DFE2
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Reimu.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 4, false, "Instant", true);
				break;
			case Reimu.MoveType.ShootAccuracy:
			{
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, true, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Weak);
				int? num = new int?(1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case Reimu.MoveType.ShootDebuff:
			{
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage3, 1, false, "Instant", true);
				string text2 = null;
				Type typeFromHandle2 = typeof(Fengyin);
				int? num = new int?(1);
				yield return base.NegativeMove(text2, typeFromHandle2, default(int?), num, false, false, null);
				break;
			}
			case Reimu.MoveType.Defend:
				yield return new SimpleEnemyMove(Intention.Defend().WithMoveName(base.GetMove(3)), this.HakureiDefend());
				break;
			case Reimu.MoveType.Summon:
				yield return new SimpleEnemyMove(Intention.Spawn().WithMoveName(base.GetMove(4)), this.SummonActions());
				break;
			case Reimu.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCard, new int?(base.Damage4), new int?(3), true), this.SpellActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int countDown = base.CountDown;
			if (countDown == 1 || countDown == 2)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x0000FDF4 File Offset: 0x0000DFF4
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			int num2 = Enumerable.Count<EnemyUnit>(base.Battle.AllAliveEnemies);
			int num3 = 3 - num2;
			this._vacancy -= num3;
			if (base.CountDown <= 0)
			{
				this.Next = Reimu.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			if (this._vacancy <= 0)
			{
				this.Next = Reimu.MoveType.Summon;
				return;
			}
			this._defendCount--;
			if (this._defendCount <= 0 && base.Shield == 0)
			{
				this.Next = Reimu.MoveType.Defend;
				this._defendCount = 4;
				return;
			}
			this.Next = this._pool.Without(this.LastAttack).Sample(base.EnemyMoveRng);
			this.LastAttack = this.Next;
		}

		// Token: 0x0400005D RID: 93
		private readonly RepeatableRandomPool<Reimu.MoveType> _pool = new RepeatableRandomPool<Reimu.MoveType>
		{
			{
				Reimu.MoveType.MultiShoot,
				2f
			},
			{
				Reimu.MoveType.ShootAccuracy,
				2f
			},
			{
				Reimu.MoveType.ShootDebuff,
				1f
			}
		};

		// Token: 0x0400005E RID: 94
		private int _defendCount;

		// Token: 0x0400005F RID: 95
		private int _vacancy;

		// Token: 0x02000692 RID: 1682
		private enum MoveType
		{
			// Token: 0x040007C2 RID: 1986
			MultiShoot,
			// Token: 0x040007C3 RID: 1987
			ShootAccuracy,
			// Token: 0x040007C4 RID: 1988
			ShootDebuff,
			// Token: 0x040007C5 RID: 1989
			Defend,
			// Token: 0x040007C6 RID: 1990
			Summon,
			// Token: 0x040007C7 RID: 1991
			Spell
		}
	}
}
