using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.Core.Units
{
	// Token: 0x0200007E RID: 126
	[Localizable]
	public abstract class EnemyUnit : Unit
	{
		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x060005CD RID: 1485 RVA: 0x00012A78 File Offset: 0x00010C78
		// (set) Token: 0x060005CE RID: 1486 RVA: 0x00012A80 File Offset: 0x00010C80
		public EnemyUnitConfig Config { get; private set; }

		// Token: 0x060005CF RID: 1487 RVA: 0x00012A89 File Offset: 0x00010C89
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<EnemyUnit>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00012A99 File Offset: 0x00010C99
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<EnemyUnit>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x060005D1 RID: 1489 RVA: 0x00012AA8 File Offset: 0x00010CA8
		public string ModelName
		{
			get
			{
				if (!this.Config.ModleName.IsNullOrEmpty())
				{
					return this.Config.ModleName;
				}
				return base.Id;
			}
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x00012ACE File Offset: 0x00010CCE
		public override UnitName GetName()
		{
			return UnitNameTable.GetName(base.Id, this.Config.NarrativeColor);
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x060005D3 RID: 1491 RVA: 0x00012AE6 File Offset: 0x00010CE6
		public string Title
		{
			get
			{
				return this.LocalizeProperty("Title", false, true);
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x060005D4 RID: 1492 RVA: 0x00012AF5 File Offset: 0x00010CF5
		private IReadOnlyList<string> Moves
		{
			get
			{
				return this.LocalizeListProperty("Move", true);
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x060005D5 RID: 1493 RVA: 0x00012B03 File Offset: 0x00010D03
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x060005D6 RID: 1494 RVA: 0x00012B10 File Offset: 0x00010D10
		// (set) Token: 0x060005D7 RID: 1495 RVA: 0x00012B18 File Offset: 0x00010D18
		internal int Index { get; set; }

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x060005D8 RID: 1496 RVA: 0x00012B21 File Offset: 0x00010D21
		// (set) Token: 0x060005D9 RID: 1497 RVA: 0x00012B29 File Offset: 0x00010D29
		public int RootIndex { get; internal set; }

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x00012B32 File Offset: 0x00010D32
		public bool IsServant
		{
			get
			{
				return base.HasStatusEffect<Servant>();
			}
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x060005DB RID: 1499 RVA: 0x00012B3A File Offset: 0x00010D3A
		protected string Gun1
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun1);
			}
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x00012B4C File Offset: 0x00010D4C
		protected string Gun2
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun2);
			}
		}

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x060005DD RID: 1501 RVA: 0x00012B5E File Offset: 0x00010D5E
		protected string Gun3
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun3);
			}
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x00012B70 File Offset: 0x00010D70
		protected string Gun4
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun4);
			}
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x00012B84 File Offset: 0x00010D84
		private static string GetRandomGun(IReadOnlyList<string> guns)
		{
			int count = guns.Count;
			if (count > 1)
			{
				return guns[Random.Range(0, count)];
			}
			return Enumerable.FirstOrDefault<string>(guns);
		}

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x060005E0 RID: 1504 RVA: 0x00012BB0 File Offset: 0x00010DB0
		protected int Damage1
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage1, this.Config.Damage1Hard, this.Config.Damage1Lunatic, "Damage1");
			}
		}

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x060005E1 RID: 1505 RVA: 0x00012BDE File Offset: 0x00010DDE
		protected int Damage2
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage2, this.Config.Damage2Hard, this.Config.Damage2Lunatic, "Damage2");
			}
		}

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x060005E2 RID: 1506 RVA: 0x00012C0C File Offset: 0x00010E0C
		protected int Damage3
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage3, this.Config.Damage3Hard, this.Config.Damage3Lunatic, "Damage3");
			}
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x060005E3 RID: 1507 RVA: 0x00012C3A File Offset: 0x00010E3A
		protected int Damage4
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage4, this.Config.Damage4Hard, this.Config.Damage4Lunatic, "Damage4");
			}
		}

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x060005E4 RID: 1508 RVA: 0x00012C68 File Offset: 0x00010E68
		protected int Power
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Power, this.Config.PowerHard, this.Config.PowerLunatic, "Power");
			}
		}

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x060005E5 RID: 1509 RVA: 0x00012C96 File Offset: 0x00010E96
		protected int Defend
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Defend, this.Config.DefendHard, this.Config.DefendLunatic, "Defend");
			}
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x00012CC4 File Offset: 0x00010EC4
		protected int Count1
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Count1, this.Config.Count1Hard, this.Config.Count1Lunatic, "Count1");
			}
		}

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x060005E7 RID: 1511 RVA: 0x00012CF2 File Offset: 0x00010EF2
		protected int Count2
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Count2, this.Config.Count2Hard, this.Config.Count2Lunatic, "Count2");
			}
		}

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x060005E8 RID: 1512 RVA: 0x00012D20 File Offset: 0x00010F20
		// (set) Token: 0x060005E9 RID: 1513 RVA: 0x00012D28 File Offset: 0x00010F28
		protected int CountDown { get; set; }

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x060005EA RID: 1514 RVA: 0x00012D31 File Offset: 0x00010F31
		protected IEnumerable<EnemyUnit> AllAliveEnemies
		{
			get
			{
				return base.Battle.AllAliveEnemies;
			}
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x060005EB RID: 1515 RVA: 0x00012D3E File Offset: 0x00010F3E
		protected RandomGen EnemyMoveRng
		{
			get
			{
				return base.Battle.GameRun.EnemyMoveRng;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x060005EC RID: 1516 RVA: 0x00012D50 File Offset: 0x00010F50
		protected RandomGen EnemyBattleRng
		{
			get
			{
				return base.Battle.GameRun.EnemyBattleRng;
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x060005ED RID: 1517 RVA: 0x00012D62 File Offset: 0x00010F62
		// (set) Token: 0x060005EE RID: 1518 RVA: 0x00012D6A File Offset: 0x00010F6A
		private protected GameDifficulty Difficulty { protected get; private set; }

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x060005EF RID: 1519 RVA: 0x00012D73 File Offset: 0x00010F73
		public virtual int MovePriority
		{
			get
			{
				return 10;
			}
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x060005F0 RID: 1520 RVA: 0x00012D77 File Offset: 0x00010F77
		// (set) Token: 0x060005F1 RID: 1521 RVA: 0x00012D7F File Offset: 0x00010F7F
		public List<Intention> Intentions { get; protected set; } = new List<Intention>();

		// Token: 0x060005F2 RID: 1522 RVA: 0x00012D88 File Offset: 0x00010F88
		private int? GetAdjustedConfigValue(int? easyAndNormal, int? hard, int? lunatic)
		{
			GameDifficulty difficulty = this.Difficulty;
			int? num;
			if (difficulty != GameDifficulty.Hard)
			{
				if (difficulty != GameDifficulty.Lunatic)
				{
					num = easyAndNormal;
				}
				else
				{
					int? num2 = lunatic;
					int? num4;
					if (num2 == null)
					{
						int? num3 = hard;
						num4 = ((num3 != null) ? num3 : easyAndNormal);
					}
					else
					{
						num4 = num2;
					}
					num = num4;
				}
			}
			else
			{
				int? num2 = hard;
				num = ((num2 != null) ? num2 : easyAndNormal);
			}
			return num;
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00012DDC File Offset: 0x00010FDC
		private int GuardedGetAdjustedConfigValue(int? easyAndNormal, int? hard, int? lunatic, [CallerMemberName] string memberName = "")
		{
			int? adjustedConfigValue = this.GetAdjustedConfigValue(easyAndNormal, hard, lunatic);
			if (adjustedConfigValue == null)
			{
				throw new InvalidDataException(this.DebugName + " has no '" + memberName + "' in config");
			}
			return adjustedConfigValue.GetValueOrDefault();
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00012E20 File Offset: 0x00011020
		public string GetMove(int index)
		{
			IReadOnlyList<string> moves = this.Moves;
			string text = ((moves != null) ? moves.TryGetValue(index) : null);
			if (text != null)
			{
				return text;
			}
			Debug.LogError(string.Format("Move {0} of {1} not found", index, this.DebugName));
			return string.Format("<Move {0}>", index);
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x00012E74 File Offset: 0x00011074
		protected string GetSpellCardName(int? title, int name)
		{
			if (this.Moves == null)
			{
				return null;
			}
			if (title != null)
			{
				return this.Moves.TryGetValue(title.Value) + "「" + this.Moves.TryGetValue(name) + "」";
			}
			return "「" + this.Moves.TryGetValue(name) + "」";
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00012EE0 File Offset: 0x000110E0
		public override void Initialize()
		{
			base.Initialize();
			this.Config = EnemyUnitConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find enemy-unit config for " + base.Id);
			}
			base.SetMaxHp(this.Config.MaxHp, this.Config.MaxHp);
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00012F3E File Offset: 0x0001113E
		protected void SetMaxHpInBattle(int hp, int maxHp)
		{
			base.SetMaxHp(hp, maxHp);
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00012F48 File Offset: 0x00011148
		internal override void EnterGameRun(GameRunController gameRun)
		{
			this.Difficulty = gameRun.Difficulty;
			int num = this.GuardedGetAdjustedConfigValue(new int?(this.Config.MaxHp), this.Config.MaxHpHard, this.Config.MaxHpLunatic, "MaxHp");
			int? maxHpAdd = this.Config.MaxHpAdd;
			int num2;
			if (maxHpAdd != null)
			{
				int valueOrDefault = maxHpAdd.GetValueOrDefault();
				num2 = num + gameRun.EnemyBattleRng.NextInt(0, valueOrDefault);
			}
			else
			{
				num2 = num;
			}
			int num3 = num2;
			base.SetMaxHp(num3, num3);
			base.EnterGameRun(gameRun);
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00012FD4 File Offset: 0x000111D4
		public virtual void OnSpawn(EnemyUnit spawner)
		{
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x00012FD6 File Offset: 0x000111D6
		internal override void Die()
		{
			base.Die();
			this.Intentions.Clear();
			this.NotifyIntentionsChanged();
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x00012FEF File Offset: 0x000111EF
		internal void Escape()
		{
			this.Intentions.Clear();
			this.NotifyIntentionsChanged();
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x00013002 File Offset: 0x00011202
		internal IEnumerable<BattleAction> GetActions()
		{
			return this.Actions();
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x0001300A File Offset: 0x0001120A
		protected virtual IEnumerable<BattleAction> Actions()
		{
			foreach (IEnemyMove enemyMove in Enumerable.ToList<IEnemyMove>(this._turnMoves))
			{
				if (base.IsAlive && enemyMove.Actions != null)
				{
					Intention intention = enemyMove.Intention;
					if (intention != null)
					{
						intention.NotifyActivating();
					}
					foreach (BattleAction battleAction in enemyMove.Actions)
					{
						yield return battleAction;
					}
					IEnumerator<BattleAction> enumerator2 = null;
				}
			}
			List<IEnemyMove>.Enumerator enumerator = default(List<IEnemyMove>.Enumerator);
			this.UpdateMoveCounters();
			yield break;
			yield break;
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x0001301A File Offset: 0x0001121A
		protected virtual IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield break;
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x00013023 File Offset: 0x00011223
		protected virtual void UpdateMoveCounters()
		{
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x00013025 File Offset: 0x00011225
		protected virtual IEnumerable<BattleAction> AttackActions(string move, string gunName, int damage, int times = 1, bool isAccuracy = false, string followGunName = "Instant")
		{
			List<DamageAction> damageActions = new List<DamageAction>();
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			if (times < 2)
			{
				if (base.Battle.BattleShouldEnd || base.IsNotAlive)
				{
					yield break;
				}
				DamageAction damageAction = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)damage, isAccuracy), gunName, GunType.Single);
				damageActions.Add(damageAction);
				yield return damageAction;
			}
			else
			{
				if (base.Battle.BattleShouldEnd || base.IsNotAlive)
				{
					yield break;
				}
				DamageAction damageAction2 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)damage, isAccuracy), gunName, GunType.First);
				damageActions.Add(damageAction2);
				yield return damageAction2;
				if (times > 2)
				{
					int num;
					for (int i = 0; i < times - 2; i = num + 1)
					{
						if (base.Battle.BattleShouldEnd || base.IsNotAlive)
						{
							yield break;
						}
						DamageAction damageAction3 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)damage, isAccuracy), followGunName.IsNullOrEmpty() ? gunName : followGunName, GunType.Middle);
						damageActions.Add(damageAction3);
						yield return damageAction3;
						num = i;
					}
				}
				if (base.Battle.BattleShouldEnd || base.IsNotAlive)
				{
					yield break;
				}
				DamageAction damageAction4 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)damage, isAccuracy), followGunName.IsNullOrEmpty() ? gunName : followGunName, GunType.Last);
				damageActions.Add(damageAction4);
				yield return damageAction4;
			}
			if (!damageActions.Empty<DamageAction>())
			{
				yield return new StatisticalTotalDamageAction(damageActions);
			}
			yield break;
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00013064 File Offset: 0x00011264
		protected IEnemyMove AttackMove(string move, [CanBeNull] string gunName, int damage, int times = 1, bool isAccuracy = false, string followGunName = "Instant", bool withSpell = false)
		{
			return new SimpleEnemyMove(withSpell ? ((times > 1) ? Intention.Attack(damage, times, isAccuracy).WithMoveName(move) : Intention.Attack(damage, isAccuracy).WithMoveName(move)) : ((times > 1) ? Intention.Attack(damage, times, isAccuracy) : Intention.Attack(damage, isAccuracy)), this.AttackActions(move, gunName, damage, times, isAccuracy, followGunName));
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x000130CC File Offset: 0x000112CC
		protected IEnumerable<BattleAction> DefendActions(Unit target, [CanBeNull] string move, int block = 0, int shield = 0, int graze = 0, bool cast = true, PerformAction performAction = null)
		{
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			if (block > 0 || shield > 0)
			{
				yield return new CastBlockShieldAction(this, target ?? this, block, shield, BlockShieldType.Normal, cast);
			}
			if (performAction != null)
			{
				yield return performAction;
			}
			if (graze > 0)
			{
				yield return new ApplyStatusEffectAction<Graze>(target ?? this, new int?(graze), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x06000603 RID: 1539 RVA: 0x0001311C File Offset: 0x0001131C
		protected IEnemyMove DefendMove(Unit target, [CanBeNull] string move, int block = 0, int shield = 0, int graze = 0, bool cast = true, PerformAction performAction = null)
		{
			if (block == 0 && shield == 0 && graze == 0)
			{
				throw new ArgumentException("Cannot create defend move with block = 0 AND shield = 0 AND graze = 0");
			}
			return new SimpleEnemyMove((block > 0 || shield > 0) ? Intention.Defend() : Intention.Graze(), this.DefendActions(target, move, block, shield, graze, cast, performAction));
		}

		// Token: 0x06000604 RID: 1540 RVA: 0x0001316A File Offset: 0x0001136A
		protected IEnumerable<BattleAction> AddCardActions([CanBeNull] string move, IEnumerable<Card> cards, EnemyUnit.AddCardZone zone = EnemyUnit.AddCardZone.Discard, PerformAction perform = null)
		{
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			if (perform != null)
			{
				yield return perform;
			}
			BattleAction battleAction;
			switch (zone)
			{
			case EnemyUnit.AddCardZone.Discard:
				battleAction = new AddCardsToDiscardAction(cards, AddCardsType.Normal);
				break;
			case EnemyUnit.AddCardZone.Draw:
				battleAction = new AddCardsToDrawZoneAction(cards, DrawZoneTarget.Random, AddCardsType.Normal);
				break;
			case EnemyUnit.AddCardZone.Hand:
				battleAction = new AddCardsToHandAction(cards, AddCardsType.Normal);
				break;
			default:
				throw new ArgumentOutOfRangeException("zone", zone, null);
			}
			yield return battleAction;
			yield break;
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00013197 File Offset: 0x00011397
		protected IEnemyMove AddCardMove([CanBeNull] string move, IEnumerable<Card> cards, EnemyUnit.AddCardZone zone = EnemyUnit.AddCardZone.Discard, PerformAction perform = null, bool withSpell = false)
		{
			if (!withSpell)
			{
				return new SimpleEnemyMove(Intention.AddCard(), this.AddCardActions(move, cards, zone, perform));
			}
			return new SimpleEnemyMove(Intention.AddCard().WithMoveName(move), this.AddCardActions(move, cards, zone, perform));
		}

		// Token: 0x06000606 RID: 1542 RVA: 0x000131D0 File Offset: 0x000113D0
		protected IEnemyMove AddCardMove([CanBeNull] string move, Type cardType, int amount = 1, EnemyUnit.AddCardZone zone = EnemyUnit.AddCardZone.Discard, PerformAction perform = null, bool withSpell = false)
		{
			List<Card> list = new List<Card>();
			if (amount > 1)
			{
				for (int i = 0; i < amount; i++)
				{
					list.Add(Library.CreateCard(cardType));
				}
			}
			else
			{
				list.Add(Library.CreateCard(cardType));
			}
			if (!withSpell)
			{
				return new SimpleEnemyMove(Intention.AddCard(), this.AddCardActions(move, list, zone, perform));
			}
			return new SimpleEnemyMove(Intention.AddCard().WithMoveName(move), this.AddCardActions(move, list, zone, perform));
		}

		// Token: 0x06000607 RID: 1543 RVA: 0x00013244 File Offset: 0x00011444
		protected IEnumerable<BattleAction> PositiveActions([CanBeNull] string move, Type type, int? level = null, int? duration = null, float occupationTime = 0f, PerformAction performAction = null)
		{
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			if (performAction != null)
			{
				yield return performAction;
			}
			yield return new ApplyStatusEffectAction(type, this, level, duration, default(int?), default(int?), occupationTime, true);
			yield break;
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x00013281 File Offset: 0x00011481
		protected IEnemyMove PositiveMove([CanBeNull] string move, Type type, int? level = null, int? duration = null, bool withSpell = false, PerformAction performAction = null)
		{
			return new SimpleEnemyMove(withSpell ? Intention.PositiveEffect().WithMoveName(move) : Intention.PositiveEffect(), this.PositiveActions(move, type, level, duration, 0f, performAction));
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x000132B0 File Offset: 0x000114B0
		protected IEnumerable<BattleAction> NegativeActions([CanBeNull] string move, Type type, int? level = null, int? duration = null, bool startAutoDecreasing = true, PerformAction performAction = null)
		{
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			if (performAction != null)
			{
				yield return performAction;
			}
			int? num;
			if (type == typeof(Vulnerable) || type == typeof(LockedOn))
			{
				EnemyUnit lastAliveEnemy = base.Battle.LastAliveEnemy;
				if (lastAliveEnemy != null && lastAliveEnemy != this)
				{
					if (type == typeof(Vulnerable))
					{
						num = new int?(startAutoDecreasing ? 1 : 0);
						yield return new ApplyStatusEffectAction<EnemyVulnerable>(this, duration, default(int?), default(int?), num, 0f, true);
						yield break;
					}
					if (type == typeof(LockedOn))
					{
						num = new int?(startAutoDecreasing ? 1 : 0);
						yield return new ApplyStatusEffectAction<EnemyLockedOn>(this, level, default(int?), default(int?), num, 0f, true);
						yield break;
					}
				}
			}
			Unit player = base.Battle.Player;
			num = default(int?);
			int? num2 = num;
			num = default(int?);
			yield return new ApplyStatusEffectAction(type, player, level, duration, num2, num, 0f, startAutoDecreasing);
			yield break;
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x000132ED File Offset: 0x000114ED
		protected IEnemyMove NegativeMove([CanBeNull] string move, Type type, int? level = null, int? duration = null, bool startAutoDecreasing = true, bool withSpell = false, PerformAction performAction = null)
		{
			return new SimpleEnemyMove(withSpell ? Intention.NegativeEffect(null).WithMoveName(move) : Intention.NegativeEffect(null), this.NegativeActions(move, type, level, duration, startAutoDecreasing, performAction));
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x0001331B File Offset: 0x0001151B
		protected IEnumerable<BattleAction> PerformActions([CanBeNull] string move, PerformAction performAction)
		{
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			yield return performAction;
			yield break;
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00013339 File Offset: 0x00011539
		protected IEnemyMove ClearMove(float occupationTime = 0f)
		{
			return new SimpleEnemyMove(Intention.Clear(), new RemoveAllNegativeStatusEffectAction(this, occupationTime));
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x0001334C File Offset: 0x0001154C
		public void UpdateTurnMoves()
		{
			if (base.IsInTurn)
			{
				Debug.LogError("[EnemyUnit: " + this.DebugName + "] UpdateTurnMoves() is invoked in turn, behaviour maybe undefined");
			}
			this._turnMoves.Clear();
			foreach (IEnemyMove enemyMove in this.GetTurnMoves())
			{
				this._turnMoves.Add(enemyMove);
			}
			this.Intentions = Enumerable.ToList<Intention>(Enumerable.Select<Intention, Intention>(this.GetIntentions(), (Intention i) => i.SetSource(this)));
			this.NotifyIntentionsChanged();
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x000133F4 File Offset: 0x000115F4
		public void ClearIntentions()
		{
			this.Intentions.Clear();
			this.NotifyIntentionsChanged();
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00013407 File Offset: 0x00011607
		protected virtual IEnumerable<Intention> GetIntentions()
		{
			foreach (IEnemyMove enemyMove in this._turnMoves)
			{
				yield return enemyMove.Intention;
			}
			List<IEnemyMove>.Enumerator enumerator = default(List<IEnemyMove>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06000610 RID: 1552 RVA: 0x00013418 File Offset: 0x00011618
		// (remove) Token: 0x06000611 RID: 1553 RVA: 0x00013450 File Offset: 0x00011650
		public event Action<EnemyUnit> IntentionsChanged;

		// Token: 0x06000612 RID: 1554 RVA: 0x00013485 File Offset: 0x00011685
		public void NotifyIntentionsChanged()
		{
			Action<EnemyUnit> intentionsChanged = this.IntentionsChanged;
			if (intentionsChanged == null)
			{
				return;
			}
			intentionsChanged.Invoke(this);
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x00013498 File Offset: 0x00011698
		public override void SetView(IUnitView view)
		{
			base.SetView(view);
			IEnemyUnitView enemyUnitView = view as IEnemyUnitView;
			if (enemyUnitView != null)
			{
				this.View = enemyUnitView;
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06000614 RID: 1556 RVA: 0x000134BD File Offset: 0x000116BD
		// (set) Token: 0x06000615 RID: 1557 RVA: 0x000134C5 File Offset: 0x000116C5
		public new IEnemyUnitView View { get; private set; }

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000616 RID: 1558 RVA: 0x000134CE File Offset: 0x000116CE
		public GameEvent<DieEventArgs> EnemyPointGenerating { get; } = new GameEvent<DieEventArgs>();

		// Token: 0x040002DA RID: 730
		private readonly List<IEnemyMove> _turnMoves = new List<IEnemyMove>();

		// Token: 0x02000226 RID: 550
		public enum AddCardZone
		{
			// Token: 0x0400084F RID: 2127
			Discard,
			// Token: 0x04000850 RID: 2128
			Draw,
			// Token: 0x04000851 RID: 2129
			Hand
		}
	}
}
