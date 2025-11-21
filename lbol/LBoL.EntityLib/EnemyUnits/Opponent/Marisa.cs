using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	// Token: 0x020001CF RID: 463
	[UsedImplicitly]
	public sealed class Marisa : EnemyUnit
	{
		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060006E0 RID: 1760 RVA: 0x0000FA75 File Offset: 0x0000DC75
		// (set) Token: 0x060006E1 RID: 1761 RVA: 0x0000FA7D File Offset: 0x0000DC7D
		private Marisa.MoveType Next { get; set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060006E2 RID: 1762 RVA: 0x0000FA86 File Offset: 0x0000DC86
		// (set) Token: 0x060006E3 RID: 1763 RVA: 0x0000FA8E File Offset: 0x0000DC8E
		private Marisa.MoveType Last { get; set; }

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060006E4 RID: 1764 RVA: 0x0000FA97 File Offset: 0x0000DC97
		private string SpellCardName
		{
			get
			{
				return base.GetSpellCardName(new int?(5), 6);
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060006E5 RID: 1765 RVA: 0x0000FAA6 File Offset: 0x0000DCA6
		// (set) Token: 0x060006E6 RID: 1766 RVA: 0x0000FAAE File Offset: 0x0000DCAE
		private int AddCardsCountDown { get; set; }

		// Token: 0x060006E7 RID: 1767 RVA: 0x0000FAB7 File Offset: 0x0000DCB7
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Marisa.MoveType.ShootAddCard;
			base.CountDown = 5;
			this.AddCardsCountDown = 3;
			this._spellTimes = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0000FAF2 File Offset: 0x0000DCF2
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<FirepowerIsJustice>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x0000FB04 File Offset: 0x0000DD04
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<FirepowerIsJustice>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x0000FB87 File Offset: 0x0000DD87
		private IEnumerable<BattleAction> ChargeActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(4), true);
			yield return PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1);
			yield break;
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x0000FB97 File Offset: 0x0000DD97
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Marisa.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + this._spellTimes, 3, false, "Instant", true);
				break;
			case Marisa.MoveType.ShootAddCard:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2 + this._spellTimes, 1, false, "Instant", true);
				yield return base.AddCardMove(null, typeof(Riguang), 1, EnemyUnit.AddCardZone.Discard, null, false);
				break;
			case Marisa.MoveType.ShootGraze:
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage3 + this._spellTimes, 1, false, "Instant", true);
				yield return base.DefendMove(this, null, 0, 0, base.Defend, true, null);
				break;
			case Marisa.MoveType.AddCards:
			{
				List<Card> list = new List<Card>();
				list.Add(Library.CreateCard<Xingguang>());
				list.Add(Library.CreateCard<Xingguang>());
				list.Add(Library.CreateCard<Xingguang>());
				list.Add(Library.CreateCard<AyaNews>());
				List<Card> list2 = list;
				GameDifficulty difficulty = base.GameRun.Difficulty;
				if (difficulty != GameDifficulty.Hard)
				{
					if (difficulty == GameDifficulty.Lunatic)
					{
						list2.Add(Library.CreateCard<BlackResidue>());
					}
				}
				else
				{
					list2.Add(Library.CreateCard<HatateNews>());
				}
				list2.Shuffle(base.EnemyMoveRng);
				yield return base.AddCardMove(base.GetMove(3), list2, EnemyUnit.AddCardZone.Discard, PerformAction.Animation(this, "defend", 0f, null, 0f, -1), true);
				break;
			}
			case Marisa.MoveType.Charge:
				yield return new SimpleEnemyMove(Intention.Charge(), this.ChargeActions());
				break;
			case Marisa.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCardName, new int?(base.Damage4 + this._spellTimes * base.Count1), true), this.AttackActions(this.SpellCardName, base.Gun4, base.Damage4 + this._spellTimes * base.Count1, 1, true, "Instant"));
				this._spellTimes++;
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

		// Token: 0x060006EC RID: 1772 RVA: 0x0000FBA8 File Offset: 0x0000DDA8
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Marisa.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			if (base.CountDown == 1 && base.Difficulty != GameDifficulty.Lunatic)
			{
				this.Next = Marisa.MoveType.Charge;
				return;
			}
			num = this.AddCardsCountDown - 1;
			this.AddCardsCountDown = num;
			if (this.AddCardsCountDown <= 0)
			{
				this.Next = Marisa.MoveType.AddCards;
				this.AddCardsCountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
			this.Last = this.Next;
		}

		// Token: 0x04000059 RID: 89
		private int _spellTimes;

		// Token: 0x0400005A RID: 90
		private readonly RepeatableRandomPool<Marisa.MoveType> _pool = new RepeatableRandomPool<Marisa.MoveType>
		{
			{
				Marisa.MoveType.MultiShoot,
				1f
			},
			{
				Marisa.MoveType.ShootAddCard,
				1f
			},
			{
				Marisa.MoveType.ShootGraze,
				1f
			}
		};

		// Token: 0x0200068E RID: 1678
		private enum MoveType
		{
			// Token: 0x040007AF RID: 1967
			MultiShoot,
			// Token: 0x040007B0 RID: 1968
			ShootAddCard,
			// Token: 0x040007B1 RID: 1969
			ShootGraze,
			// Token: 0x040007B2 RID: 1970
			AddCards,
			// Token: 0x040007B3 RID: 1971
			Charge,
			// Token: 0x040007B4 RID: 1972
			Spell
		}
	}
}
