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
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	// Token: 0x020001CD RID: 461
	[UsedImplicitly]
	public sealed class Cirno : EnemyUnit
	{
		// Token: 0x1700008B RID: 139
		// (get) Token: 0x060006B0 RID: 1712 RVA: 0x0000F348 File Offset: 0x0000D548
		// (set) Token: 0x060006B1 RID: 1713 RVA: 0x0000F350 File Offset: 0x0000D550
		private Cirno.MoveType Next { get; set; }

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x060006B2 RID: 1714 RVA: 0x0000F359 File Offset: 0x0000D559
		// (set) Token: 0x060006B3 RID: 1715 RVA: 0x0000F361 File Offset: 0x0000D561
		private Cirno.MoveType Last { get; set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060006B4 RID: 1716 RVA: 0x0000F36C File Offset: 0x0000D56C
		private string SpellCardName
		{
			get
			{
				if (this.NextBuff == typeof(EnemyDayaojing))
				{
					return base.GetSpellCardName(new int?(4), 5);
				}
				if (this.NextBuff == typeof(EnemyLarva))
				{
					return base.GetSpellCardName(new int?(4), 6);
				}
				if (this.NextBuff == typeof(EnemyLily))
				{
					return base.GetSpellCardName(new int?(4), 7);
				}
				if (this.NextBuff == typeof(EnemyMaid))
				{
					return base.GetSpellCardName(new int?(4), 8);
				}
				return base.GetSpellCardName(new int?(9), 10);
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x060006B5 RID: 1717 RVA: 0x0000F41C File Offset: 0x0000D61C
		// (set) Token: 0x060006B6 RID: 1718 RVA: 0x0000F424 File Offset: 0x0000D624
		private int AddCardsCountDown { get; set; }

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060006B7 RID: 1719 RVA: 0x0000F42D File Offset: 0x0000D62D
		private bool HasLily
		{
			get
			{
				return base.HasStatusEffect<EnemyLily>();
			}
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x0000F438 File Offset: 0x0000D638
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				this.SetNextBuff();
				this._supportPool.Add(typeof(EnemyLarva), 1);
			}
			else
			{
				this._supportPool.Add(typeof(EnemyLarva), 1);
				this.SetNextBuff();
			}
			this.Next = Cirno.MoveType.MultiShoot;
			this.Last = this.Next;
			base.CountDown = 4;
			this.AddCardsCountDown = 1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x0000F4C6 File Offset: 0x0000D6C6
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				yield return new ApplyStatusEffectAction(this.NextBuff, this, new int?(this.NextLevel), default(int?), default(int?), default(int?), 0f, true);
				this.SetNextBuff();
			}
			yield break;
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x0000F4D8 File Offset: 0x0000D6D8
		public override void OnSpawn(EnemyUnit spawner)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				this.React(new ApplyStatusEffectAction(this.NextBuff, this, new int?(this.NextLevel), default(int?), default(int?), default(int?), 0f, true));
				this.SetNextBuff();
			}
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x0000F572 File Offset: 0x0000D772
		private IEnumerable<BattleAction> CirnoDefend()
		{
			yield return new EnemyMoveAction(this, base.GetMove(3), true);
			yield return PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1);
			int num = base.Defend;
			if (this.HasLily)
			{
				num += 3;
			}
			yield return new ApplyStatusEffectAction<FrostArmor>(this, new int?(num), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x0000F582 File Offset: 0x0000D782
		private IEnumerable<BattleAction> SpellActions()
		{
			IEnumerator<BattleAction> enumerator;
			if (this.NextBuff == typeof(EnemyDayaojing))
			{
				yield return PerformAction.Spell(this, "冰藤滋生");
				foreach (BattleAction battleAction in this.AttackActions(null, "CirnoDayao", base.Damage4, 1, true, "Instant"))
				{
					yield return battleAction;
				}
				enumerator = null;
			}
			else if (this.NextBuff == typeof(EnemyLarva))
			{
				yield return PerformAction.Spell(this, "冰鳞滑行");
				foreach (BattleAction battleAction2 in this.AttackActions(null, "CirnoLarva", base.Damage4, 1, true, "Instant"))
				{
					yield return battleAction2;
				}
				enumerator = null;
			}
			else if (this.NextBuff == typeof(EnemyLily))
			{
				yield return PerformAction.Spell(this, "冰冷的春天");
				foreach (BattleAction battleAction3 in this.AttackActions(null, "CirnoLily", base.Damage4, 1, true, "Instant"))
				{
					yield return battleAction3;
				}
				enumerator = null;
			}
			else if (this.NextBuff == typeof(EnemyMaid))
			{
				yield return PerformAction.Spell(this, "冰冻飞刀");
				foreach (BattleAction battleAction4 in this.AttackActions(null, "CirnoKasumi", base.Damage4, 1, true, "Instant"))
				{
					yield return battleAction4;
				}
				enumerator = null;
			}
			else
			{
				yield return PerformAction.Spell(this, "完美冻结");
				foreach (BattleAction battleAction5 in this.AttackActions(null, "CirnoDayao", base.Damage4, 1, true, "Instant"))
				{
					yield return battleAction5;
				}
				enumerator = null;
			}
			foreach (BattleAction battleAction6 in this.RandomBuff())
			{
				yield return battleAction6;
			}
			enumerator = null;
			this.SetNextBuff();
			yield break;
			yield break;
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x0000F592 File Offset: 0x0000D792
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Cirno.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, this.HasLily ? 4 : 3, false, "Instant", true);
				break;
			case Cirno.MoveType.ShootDebuff:
			{
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, false, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Weak);
				int? num = new int?(this.HasLily ? 2 : 1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case Cirno.MoveType.ShootAddCards:
				yield return base.AttackMove(base.GetMove(2), base.Gun2, base.Damage3, 1, false, "Instant", true);
				yield return base.AddCardMove(null, typeof(CirnoFreeze), this.HasLily ? 3 : 2, EnemyUnit.AddCardZone.Discard, null, false);
				break;
			case Cirno.MoveType.Defend:
				yield return new SimpleEnemyMove(Intention.Defend().WithMoveName(base.GetMove(3)), this.CirnoDefend());
				break;
			case Cirno.MoveType.Spell:
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

		// Token: 0x060006BE RID: 1726 RVA: 0x0000F5A4 File Offset: 0x0000D7A4
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Cirno.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			num = this.AddCardsCountDown - 1;
			this.AddCardsCountDown = num;
			if (this.AddCardsCountDown <= 0)
			{
				this.Next = Cirno.MoveType.ShootAddCards;
				this.AddCardsCountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
			this.Last = this.Next;
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x0000F648 File Offset: 0x0000D848
		private void SetNextBuff()
		{
			if (this._supportPool.NotEmpty<KeyValuePair<Type, int>>())
			{
				Type type;
				int num;
				this._supportPool.Sample(base.EnemyBattleRng).Deconstruct(ref type, ref num);
				this.NextBuff = type;
				this.NextLevel = num;
				this._supportPool.Remove(this.NextBuff);
				return;
			}
			this.NextBuff = null;
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x0000F6AC File Offset: 0x0000D8AC
		private IEnumerable<BattleAction> RandomBuff()
		{
			if (this.NextBuff != null)
			{
				yield return new ApplyStatusEffectAction(this.NextBuff, this, new int?(this.NextLevel), default(int?), default(int?), default(int?), 0f, true);
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(2), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x0000F6BC File Offset: 0x0000D8BC
		// (set) Token: 0x060006C2 RID: 1730 RVA: 0x0000F6C4 File Offset: 0x0000D8C4
		private Type NextBuff { get; set; }

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060006C3 RID: 1731 RVA: 0x0000F6CD File Offset: 0x0000D8CD
		// (set) Token: 0x060006C4 RID: 1732 RVA: 0x0000F6D5 File Offset: 0x0000D8D5
		private int NextLevel { get; set; }

		// Token: 0x060006C5 RID: 1733 RVA: 0x0000F6E0 File Offset: 0x0000D8E0
		public Cirno()
		{
			Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
			dictionary.Add(typeof(EnemyDayaojing), 6);
			dictionary.Add(typeof(EnemyLily), 0);
			dictionary.Add(typeof(EnemyMaid), 6);
			this._supportPool = dictionary;
			base..ctor();
		}

		// Token: 0x0400004A RID: 74
		private readonly RepeatableRandomPool<Cirno.MoveType> _pool = new RepeatableRandomPool<Cirno.MoveType>
		{
			{
				Cirno.MoveType.MultiShoot,
				2f
			},
			{
				Cirno.MoveType.ShootDebuff,
				1f
			},
			{
				Cirno.MoveType.Defend,
				1f
			}
		};

		// Token: 0x0400004D RID: 77
		private readonly Dictionary<Type, int> _supportPool;

		// Token: 0x02000683 RID: 1667
		private enum MoveType
		{
			// Token: 0x0400077C RID: 1916
			MultiShoot,
			// Token: 0x0400077D RID: 1917
			ShootDebuff,
			// Token: 0x0400077E RID: 1918
			ShootAddCards,
			// Token: 0x0400077F RID: 1919
			Defend,
			// Token: 0x04000780 RID: 1920
			Spell
		}
	}
}
