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
	[UsedImplicitly]
	public sealed class Seija : EnemyUnit
	{
		private Seija.MoveType Next { get; set; }
		public RandomGen SeijaRng { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.Difficulty == GameDifficulty.Lunatic) ? Seija.MoveType.MultiShoot : Seija.MoveType.Start);
			this.BigRoundCount = 0;
			this.SeijaRng = new RandomGen(base.GameRun.FinalBossSeed);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
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
		private IEnumerable<BattleAction> StartActions()
		{
			yield return PerformAction.Spell(this, "天下翻覆");
			yield return PerformAction.Animation(this, "spell", 1f, null, 0f, -1);
			yield return this.RandomBuff();
			this.ItemCount = 2;
			yield break;
		}
		public int ItemCount { get; private set; }
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
		private string LastType { get; set; }
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
		private IEnumerable<BattleAction> PlayerLose()
		{
			yield return PerformAction.Spell(this, "实现愿望");
			yield return PerformAction.Animation(this, "skill1", 0.8f, null, 0f, -1);
			yield return new ForceKillAction(this, base.Battle.Player);
			yield break;
		}
		private int MultiShootDamage
		{
			get
			{
				return 5;
			}
		}
		private int ShootAccuracyDamage
		{
			get
			{
				return 24;
			}
		}
		private int BigRoundCount { get; set; }
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
		private string GetGunName(int gunIndex)
		{
			string text = this.LastType + gunIndex.ToString();
			if (GunConfig.FromName(text) != null)
			{
				return text;
			}
			return "SingleJiandaoSe" + gunIndex.ToString();
		}
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
		public Seija()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(ShendengSe));
			list.Add(typeof(SihunYuSe));
			list.Add(typeof(MadokaBowSe));
			this._pool = list;
			base..ctor();
		}
		private readonly List<Type> _pool;
		private const float BuffTime = 1f;
		private enum MoveType
		{
			Start,
			MultiShoot,
			ShootAccuracy,
			BuffAndClear
		}
	}
}
