using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000247 RID: 583
	[UsedImplicitly]
	public sealed class Seija : EnemyUnit
	{
		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06000936 RID: 2358 RVA: 0x00013E71 File Offset: 0x00012071
		// (set) Token: 0x06000937 RID: 2359 RVA: 0x00013E79 File Offset: 0x00012079
		private Seija.MoveType Next { get; set; }

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06000938 RID: 2360 RVA: 0x00013E82 File Offset: 0x00012082
		// (set) Token: 0x06000939 RID: 2361 RVA: 0x00013E8A File Offset: 0x0001208A
		public RandomGen SeijaRng { get; set; }

		// Token: 0x0600093A RID: 2362 RVA: 0x00013E94 File Offset: 0x00012094
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.Difficulty == GameDifficulty.Lunatic) ? Seija.MoveType.MultiShoot : Seija.MoveType.Start);
			this.BigRoundCount = 0;
			this.SeijaRng = new RandomGen(base.GameRun.FinalBossSeed);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x00013EEE File Offset: 0x000120EE
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Spell(this, "道具的救世主");
			yield return PerformAction.Animation(this, "spell", 0f, null, 0f, -1);
			yield return PerformAction.Effect(this, "GuirenAura", 0f, "GuirenAura", 0f, PerformAction.EffectBehavior.Add, 0f);
			int? num = new int?(300);
			yield return new ApplyStatusEffectAction<LimitedDamage>(this, default(int?), default(int?), default(int?), num, 1f, true);
			yield return PerformAction.Effect(this, "SeijaExhibitManager", 0f, null, 0f, PerformAction.EffectBehavior.Add, 0f);
			GameDifficulty difficulty = base.Difficulty;
			int? num2 = new int?((difficulty == GameDifficulty.Lunatic || difficulty == GameDifficulty.Hard) ? 2 : 1);
			num = default(int?);
			int? num3 = num;
			num = default(int?);
			int? num4 = num;
			num = default(int?);
			yield return new ApplyStatusEffectAction<SingleJiandaoSe>(this, num2, num3, num4, num, 0f, true);
			this.ItemCount = 1;
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				yield return this.RandomBuff();
				this.ItemCount = 2;
			}
			if (base.GameRun.FinalBossInitialDamage > 0)
			{
				Card card3 = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone, (Card card) => card is SakuyaDetective);
				if (card3 != null)
				{
					yield return PerformAction.ViewCard(card3);
				}
				string text = "追查真凶" + ((int)MathF.Min((float)(base.GameRun.FinalBossInitialDamage / 12), 5f)).ToString();
				yield return new DamageAction(base.Battle.Player, this, DamageInfo.Reaction((float)base.GameRun.FinalBossInitialDamage, false), text, GunType.Single);
			}
			List<Card> list = new List<Card>();
			foreach (Card card2 in base.Battle.EnumerateAllCardsButExile())
			{
				if (card2 is SakuyaDetective)
				{
					list.Add(card2);
				}
			}
			if (list.Count > 0)
			{
				yield return new ExileManyCardAction(list);
			}
			yield break;
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x00013EFE File Offset: 0x000120FE
		private IEnumerable<BattleAction> StartActions()
		{
			yield return PerformAction.Spell(this, "天下翻覆");
			yield return PerformAction.Animation(this, "spell", 1f, null, 0f, -1);
			yield return this.RandomBuff();
			this.ItemCount = 2;
			yield break;
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600093D RID: 2365 RVA: 0x00013F0E File Offset: 0x0001210E
		// (set) Token: 0x0600093E RID: 2366 RVA: 0x00013F16 File Offset: 0x00012116
		public int ItemCount { get; private set; }

		// Token: 0x0600093F RID: 2367 RVA: 0x00013F1F File Offset: 0x0001211F
		private IEnumerable<BattleAction> BuffAndClear()
		{
			yield return PerformAction.Spell(this, "逆转攻势");
			yield return PerformAction.Animation(this, "spell", 1f, null, 0f, -1);
			yield return new CastBlockShieldAction(this, 0, base.Defend, BlockShieldType.Normal, false);
			switch (this.ItemCount)
			{
			case 2:
				this._pool.Add(typeof(HolyGrailSe));
				this._pool.Add(typeof(QiannianShenqiSe));
				yield return this.RandomBuff();
				break;
			case 3:
				this._pool.Add(typeof(InfinityGemsSe));
				this._pool.Add(typeof(SakuraWandSe));
				yield return this.RandomBuff();
				break;
			case 4:
				yield return this.RandomBuff();
				break;
			case 5:
				yield return this.RandomBuff();
				break;
			case 6:
				yield return new ApplyStatusEffectAction<DragonBallSe>(this, default(int?), default(int?), default(int?), default(int?), 1f, true);
				break;
			}
			int num = this.ItemCount + 1;
			this.ItemCount = num;
			num = this.BigRoundCount + 1;
			this.BigRoundCount = num;
			yield break;
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06000940 RID: 2368 RVA: 0x00013F2F File Offset: 0x0001212F
		// (set) Token: 0x06000941 RID: 2369 RVA: 0x00013F37 File Offset: 0x00012137
		private string LastType { get; set; }

		// Token: 0x06000942 RID: 2370 RVA: 0x00013F40 File Offset: 0x00012140
		private BattleAction RandomBuff()
		{
			Type type = this._pool.Sample(this.SeijaRng);
			this._pool.Remove(type);
			this.LastType = type.Name;
			int? num2;
			if (type == typeof(ShendengSe))
			{
				int? num = new int?(3);
				num2 = default(int?);
				int? num3 = num2;
				num2 = default(int?);
				int? num4 = num2;
				num2 = default(int?);
				return new ApplyStatusEffectAction<ShendengSe>(this, num, num3, num4, num2, 1f, true);
			}
			if (type == typeof(MadokaBowSe))
			{
				int? num5 = new int?(2);
				num2 = default(int?);
				int? num6 = num2;
				num2 = default(int?);
				int? num7 = num2;
				num2 = default(int?);
				return new ApplyStatusEffectAction<MadokaBowSe>(this, num5, num6, num7, num2, 1f, true);
			}
			if (type == typeof(QiannianShenqiSe))
			{
				int? num8 = new int?(2);
				num2 = new int?(10);
				return new ApplyStatusEffectAction<QiannianShenqiSe>(this, num8, default(int?), default(int?), num2, 1f, true);
			}
			Type type2 = type;
			num2 = default(int?);
			int? num9 = num2;
			num2 = default(int?);
			int? num10 = num2;
			num2 = default(int?);
			int? num11 = num2;
			num2 = default(int?);
			return new ApplyStatusEffectAction(type2, this, num9, num10, num11, num2, 1f, true);
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x0001406A File Offset: 0x0001226A
		private IEnumerable<BattleAction> PlayerLose()
		{
			yield return PerformAction.Spell(this, "实现愿望");
			yield return PerformAction.Animation(this, "skill1", 0.8f, null, 0f, -1);
			yield return new ForceKillAction(this, base.Battle.Player);
			yield break;
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000944 RID: 2372 RVA: 0x0001407A File Offset: 0x0001227A
		private int MultiShootDamage
		{
			get
			{
				return 5;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000945 RID: 2373 RVA: 0x0001407D File Offset: 0x0001227D
		private int ShootAccuracyDamage
		{
			get
			{
				return 24;
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000946 RID: 2374 RVA: 0x00014081 File Offset: 0x00012281
		// (set) Token: 0x06000947 RID: 2375 RVA: 0x00014089 File Offset: 0x00012289
		private int BigRoundCount { get; set; }

		// Token: 0x06000948 RID: 2376 RVA: 0x00014092 File Offset: 0x00012292
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if (base.HasStatusEffect<DragonBallSe>())
			{
				yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(default(int?), 5), default(int?), default(int?), false), this.PlayerLose());
			}
			else
			{
				int attackTimes = (base.HasStatusEffect<SakuraWandSe>() ? 2 : 1);
				switch (this.Next)
				{
				case Seija.MoveType.Start:
					yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(default(int?), 1), default(int?), default(int?), false), this.StartActions());
					break;
				case Seija.MoveType.MultiShoot:
				{
					int num;
					for (int i = 0; i < attackTimes; i = num + 1)
					{
						yield return base.AttackMove(base.GetMove(2), this.GetGunName(2), this.MultiShootDamage + this.BigRoundCount, 8, false, "Instant", true);
						num = i;
					}
					break;
				}
				case Seija.MoveType.ShootAccuracy:
				{
					int num;
					for (int i = 0; i < attackTimes; i = num + 1)
					{
						yield return base.AttackMove(base.GetMove(3), this.GetGunName(1), this.ShootAccuracyDamage + this.BigRoundCount * 2, 2, true, "Instant", true);
						num = i;
					}
					break;
				}
				case Seija.MoveType.BuffAndClear:
					yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(default(int?), 4), default(int?), default(int?), false), this.BuffAndClear());
					yield return base.ClearMove(0f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			yield break;
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x000140A4 File Offset: 0x000122A4
		private string GetGunName(int gunIndex)
		{
			string text = this.LastType + gunIndex.ToString();
			if (GunConfig.FromName(text) != null)
			{
				return text;
			}
			return "SingleJiandaoSe" + gunIndex.ToString();
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x000140E0 File Offset: 0x000122E0
		protected override void UpdateMoveCounters()
		{
			Seija.MoveType moveType;
			switch (this.Next)
			{
			case Seija.MoveType.Start:
				moveType = Seija.MoveType.MultiShoot;
				break;
			case Seija.MoveType.MultiShoot:
				moveType = Seija.MoveType.ShootAccuracy;
				break;
			case Seija.MoveType.ShootAccuracy:
				moveType = Seija.MoveType.BuffAndClear;
				break;
			case Seija.MoveType.BuffAndClear:
				moveType = Seija.MoveType.MultiShoot;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0001412C File Offset: 0x0001232C
		public Seija()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(ShendengSe));
			list.Add(typeof(SihunYuSe));
			list.Add(typeof(MadokaBowSe));
			this._pool = list;
			base..ctor();
		}

		// Token: 0x040000D2 RID: 210
		private readonly List<Type> _pool;

		// Token: 0x040000D4 RID: 212
		private const float BuffTime = 1f;

		// Token: 0x0200076E RID: 1902
		private enum MoveType
		{
			// Token: 0x04000B7B RID: 2939
			Start,
			// Token: 0x04000B7C RID: 2940
			MultiShoot,
			// Token: 0x04000B7D RID: 2941
			ShootAccuracy,
			// Token: 0x04000B7E RID: 2942
			BuffAndClear
		}
	}
}
