using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
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
	// Token: 0x020001CE RID: 462
	[UsedImplicitly]
	public sealed class Koishi : EnemyUnit
	{
		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x0000F760 File Offset: 0x0000D960
		// (set) Token: 0x060006C7 RID: 1735 RVA: 0x0000F768 File Offset: 0x0000D968
		private Koishi.MoveType Next { get; set; }

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060006C8 RID: 1736 RVA: 0x0000F771 File Offset: 0x0000D971
		// (set) Token: 0x060006C9 RID: 1737 RVA: 0x0000F779 File Offset: 0x0000D979
		private Koishi.MoveType Last { get; set; }

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060006CA RID: 1738 RVA: 0x0000F782 File Offset: 0x0000D982
		// (set) Token: 0x060006CB RID: 1739 RVA: 0x0000F78A File Offset: 0x0000D98A
		private Koishi.InspirationType PreviousInspiration { get; set; }

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x0000F793 File Offset: 0x0000D993
		// (set) Token: 0x060006CD RID: 1741 RVA: 0x0000F79B File Offset: 0x0000D99B
		private Koishi.InspirationType NextInspiration { get; set; }

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060006CE RID: 1742 RVA: 0x0000F7A4 File Offset: 0x0000D9A4
		// (set) Token: 0x060006CF RID: 1743 RVA: 0x0000F7AC File Offset: 0x0000D9AC
		private RandomGen InspirationRng { get; set; }

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060006D0 RID: 1744 RVA: 0x0000F7B5 File Offset: 0x0000D9B5
		private string SpellCardName
		{
			get
			{
				return base.GetSpellCardName(new int?(9), 10);
			}
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0000F7C8 File Offset: 0x0000D9C8
		protected override void OnEnterBattle(BattleController battle)
		{
			base.CountDown = 3;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			this.InspirationRng = base.EnemyMoveRng;
			this.NextInspiration = this._insPool.Sample(this.InspirationRng);
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x0000F81C File Offset: 0x0000DA1C
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				yield return new ApplyStatusEffectAction<KoishiUnknown>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
				this.SpellLevel = 1;
			}
			else
			{
				this.SpellLevel = 0;
			}
			yield break;
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x0000F82C File Offset: 0x0000DA2C
		public override void OnSpawn(EnemyUnit spawner)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				this.React(new ApplyStatusEffectAction<KoishiUnknown>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
				this.SpellLevel = 1;
			}
			else
			{
				this.SpellLevel = 0;
			}
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0000F8C5 File Offset: 0x0000DAC5
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Koishi.MoveType.TripleShoot:
				yield return base.AttackMove(base.GetMove(6), base.Gun1, base.Damage1, 3, false, "Instant", true);
				break;
			case Koishi.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(7), base.Gun2, base.Damage2, 2, false, "Instant", true);
				break;
			case Koishi.MoveType.ShootDefend:
				yield return base.AttackMove(base.GetMove(8), base.Gun3, base.Damage1, 1, false, "Instant", true);
				yield return base.DefendMove(this, null, base.Defend, 0, 0, true, null);
				break;
			case Koishi.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCardName, new int?(base.Damage4), true), this.SpellActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (this.Next == Koishi.MoveType.Spell)
			{
				yield break;
			}
			switch (this.NextInspiration)
			{
			case Koishi.InspirationType.White:
				yield return base.DefendMove(this, base.GetMove(1), 0, this.ShieldCount, 0, true, PerformAction.Effect(this, "InspirationW", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f)).AsHiddenIntention(this.HiddenIntention);
				break;
			case Koishi.InspirationType.Blue:
				yield return base.AddCardMove(base.GetMove(2), Library.CreateCards<KoishiDiscard>(this.AddCardsCount, false), EnemyUnit.AddCardZone.Draw, PerformAction.Effect(this, "InspirationU", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f), false).AsHiddenIntention(this.HiddenIntention);
				break;
			case Koishi.InspirationType.Black:
			{
				string move = base.GetMove(3);
				Type typeFromHandle = typeof(Weak);
				int? num = new int?(this.DebuffDuration);
				PerformAction performAction = PerformAction.Effect(this, "InspirationB", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.NegativeMove(move, typeFromHandle, default(int?), num, false, false, performAction).AsHiddenIntention(this.HiddenIntention);
				break;
			}
			case Koishi.InspirationType.Red:
			{
				string move2 = base.GetMove(4);
				Type typeFromHandle2 = typeof(Firepower);
				int? num2 = new int?(this.PowerCount);
				PerformAction performAction = PerformAction.Effect(this, "InspirationR", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				int? num = default(int?);
				yield return base.PositiveMove(move2, typeFromHandle2, num2, num, false, performAction).AsHiddenIntention(this.HiddenIntention);
				break;
			}
			case Koishi.InspirationType.Green:
				yield return base.DefendMove(this, base.GetMove(5), 0, 0, this.GrazeCount, true, PerformAction.Effect(this, "InspirationG", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f)).AsHiddenIntention(this.HiddenIntention);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (base.CountDown == 1)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x0000F8D5 File Offset: 0x0000DAD5
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun4, base.Damage4, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			int num = this.SpellLevel + 1;
			this.SpellLevel = num;
			yield return new ApplyStatusEffectAction<KoishiUnknown>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x0000F8E5 File Offset: 0x0000DAE5
		// (set) Token: 0x060006D7 RID: 1751 RVA: 0x0000F8ED File Offset: 0x0000DAED
		private int SpellLevel { get; set; }

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060006D8 RID: 1752 RVA: 0x0000F8F6 File Offset: 0x0000DAF6
		private bool HiddenIntention
		{
			get
			{
				return this.SpellLevel > 0;
			}
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060006D9 RID: 1753 RVA: 0x0000F901 File Offset: 0x0000DB01
		private int ShieldCount
		{
			get
			{
				return base.Defend / 2 + this.SpellLevel * 2;
			}
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060006DA RID: 1754 RVA: 0x0000F914 File Offset: 0x0000DB14
		private int AddCardsCount
		{
			get
			{
				if (this.SpellLevel <= 1)
				{
					return 1;
				}
				return 2;
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060006DB RID: 1755 RVA: 0x0000F922 File Offset: 0x0000DB22
		private int DebuffDuration
		{
			get
			{
				if (this.SpellLevel <= 2)
				{
					return 1;
				}
				return 2;
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060006DC RID: 1756 RVA: 0x0000F930 File Offset: 0x0000DB30
		private int PowerCount
		{
			get
			{
				if (this.SpellLevel <= 3)
				{
					return 1;
				}
				return 2;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060006DD RID: 1757 RVA: 0x0000F93E File Offset: 0x0000DB3E
		private int GrazeCount
		{
			get
			{
				if (this.SpellLevel <= 2)
				{
					return 1;
				}
				return 2;
			}
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x0000F94C File Offset: 0x0000DB4C
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Koishi.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
			}
			else
			{
				this.Last = this.Next;
				this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
			}
			this.PreviousInspiration = this.NextInspiration;
			this.NextInspiration = this._insPool.Without(this.PreviousInspiration).Sample(this.InspirationRng);
		}

		// Token: 0x04000054 RID: 84
		private readonly RepeatableRandomPool<Koishi.MoveType> _pool = new RepeatableRandomPool<Koishi.MoveType>
		{
			{
				Koishi.MoveType.TripleShoot,
				1f
			},
			{
				Koishi.MoveType.DoubleShoot,
				1f
			},
			{
				Koishi.MoveType.ShootDefend,
				1f
			}
		};

		// Token: 0x04000055 RID: 85
		private readonly RepeatableRandomPool<Koishi.InspirationType> _insPool = new RepeatableRandomPool<Koishi.InspirationType>
		{
			{
				Koishi.InspirationType.White,
				1f
			},
			{
				Koishi.InspirationType.Blue,
				1f
			},
			{
				Koishi.InspirationType.Black,
				1f
			},
			{
				Koishi.InspirationType.Red,
				1.5f
			},
			{
				Koishi.InspirationType.Green,
				1f
			}
		};

		// Token: 0x02000689 RID: 1673
		private enum MoveType
		{
			// Token: 0x04000797 RID: 1943
			TripleShoot,
			// Token: 0x04000798 RID: 1944
			DoubleShoot,
			// Token: 0x04000799 RID: 1945
			ShootDefend,
			// Token: 0x0400079A RID: 1946
			Spell
		}

		// Token: 0x0200068A RID: 1674
		private enum InspirationType
		{
			// Token: 0x0400079C RID: 1948
			White,
			// Token: 0x0400079D RID: 1949
			Blue,
			// Token: 0x0400079E RID: 1950
			Black,
			// Token: 0x0400079F RID: 1951
			Red,
			// Token: 0x040007A0 RID: 1952
			Green
		}
	}
}
