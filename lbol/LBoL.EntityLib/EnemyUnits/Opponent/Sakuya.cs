using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	// Token: 0x020001D1 RID: 465
	[UsedImplicitly]
	public sealed class Sakuya : EnemyUnit
	{
		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x060006FC RID: 1788 RVA: 0x0000FEFC File Offset: 0x0000E0FC
		// (set) Token: 0x060006FD RID: 1789 RVA: 0x0000FF04 File Offset: 0x0000E104
		private Sakuya.MoveType Next { get; set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x060006FE RID: 1790 RVA: 0x0000FF0D File Offset: 0x0000E10D
		// (set) Token: 0x060006FF RID: 1791 RVA: 0x0000FF15 File Offset: 0x0000E115
		private Sakuya.MoveType Last { get; set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000700 RID: 1792 RVA: 0x0000FF1E File Offset: 0x0000E11E
		private string SpellCardName
		{
			get
			{
				return base.GetSpellCardName(new int?(4), 5);
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000701 RID: 1793 RVA: 0x0000FF2D File Offset: 0x0000E12D
		// (set) Token: 0x06000702 RID: 1794 RVA: 0x0000FF35 File Offset: 0x0000E135
		private int AddCardsCountDown { get; set; }

		// Token: 0x06000703 RID: 1795 RVA: 0x0000FF3E File Offset: 0x0000E13E
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Sakuya.MoveType.MultiShoot;
			base.CountDown = 5;
			this.AddCardsCountDown = 1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x0000FF72 File Offset: 0x0000E172
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?((base.GameRun.Difficulty == GameDifficulty.Lunatic) ? 10 : 5);
			int? num2 = new int?(8);
			yield return new ApplyStatusEffectAction<PrivateSquare>(this, num, default(int?), default(int?), num2, 0f, true);
			yield break;
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x0000FF84 File Offset: 0x0000E184
		public override void OnSpawn(EnemyUnit spawner)
		{
			int? num = new int?((base.GameRun.Difficulty == GameDifficulty.Lunatic) ? 10 : 5);
			int? num2 = new int?(8);
			this.React(new ApplyStatusEffectAction<PrivateSquare>(this, num, default(int?), default(int?), num2, 0f, true));
			num2 = default(int?);
			int? num3 = num2;
			num2 = default(int?);
			int? num4 = num2;
			num2 = default(int?);
			int? num5 = num2;
			num2 = default(int?);
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, num3, num4, num5, num2, 0f, true));
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x00010016 File Offset: 0x0000E216
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun4, base.Damage4, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			int? num = new int?(5);
			int? num2 = new int?(8);
			yield return new ApplyStatusEffectAction<PrivateSquare>(this, num, default(int?), default(int?), num2, 0f, true);
			yield break;
			yield break;
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x00010026 File Offset: 0x0000E226
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Sakuya.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 4, false, "Instant", true);
				break;
			case Sakuya.MoveType.ShootDebuff:
			{
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Fragil);
				int? num = new int?(1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case Sakuya.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(2), 0, base.Defend, base.Count1, true, null);
				break;
			case Sakuya.MoveType.AddCards:
				yield return base.AddCardMove(base.GetMove(3), typeof(SakuyaLock), 2, EnemyUnit.AddCardZone.Draw, PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1), true);
				break;
			case Sakuya.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCardName, new int?(base.Damage4), true), this.SpellActions());
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

		// Token: 0x06000708 RID: 1800 RVA: 0x00010038 File Offset: 0x0000E238
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Sakuya.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			num = this.AddCardsCountDown - 1;
			this.AddCardsCountDown = num;
			if (this.AddCardsCountDown <= 0)
			{
				this.Next = Sakuya.MoveType.AddCards;
				this.AddCardsCountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			this.Last = this.Next;
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
		}

		// Token: 0x04000063 RID: 99
		private readonly RepeatableRandomPool<Sakuya.MoveType> _pool = new RepeatableRandomPool<Sakuya.MoveType>
		{
			{
				Sakuya.MoveType.MultiShoot,
				2f
			},
			{
				Sakuya.MoveType.ShootDebuff,
				2f
			},
			{
				Sakuya.MoveType.Defend,
				1f
			}
		};

		// Token: 0x02000699 RID: 1689
		private enum MoveType
		{
			// Token: 0x040007E1 RID: 2017
			MultiShoot,
			// Token: 0x040007E2 RID: 2018
			ShootDebuff,
			// Token: 0x040007E3 RID: 2019
			Defend,
			// Token: 0x040007E4 RID: 2020
			AddCards,
			// Token: 0x040007E5 RID: 2021
			Spell
		}
	}
}
