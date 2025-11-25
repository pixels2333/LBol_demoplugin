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
	[Localizable]
	public abstract class EnemyUnit : Unit
	{
		public EnemyUnitConfig Config { get; private set; }
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<EnemyUnit>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<EnemyUnit>.LocalizeListProperty(base.Id, key, required);
		}
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
		public override UnitName GetName()
		{
			return UnitNameTable.GetName(base.Id, this.Config.NarrativeColor);
		}
		public string Title
		{
			get
			{
				return this.LocalizeProperty("Title", false, true);
			}
		}
		private IReadOnlyList<string> Moves
		{
			get
			{
				return this.LocalizeListProperty("Move", true);
			}
		}
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}
		internal int Index { get; set; }
		public int RootIndex { get; internal set; }
		public bool IsServant
		{
			get
			{
				return base.HasStatusEffect<Servant>();
			}
		}
		protected string Gun1
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun1);
			}
		}
		protected string Gun2
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun2);
			}
		}
		protected string Gun3
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun3);
			}
		}
		protected string Gun4
		{
			get
			{
				return EnemyUnit.GetRandomGun(this.Config.Gun4);
			}
		}
		private static string GetRandomGun(IReadOnlyList<string> guns)
		{
			int count = guns.Count;
			if (count > 1)
			{
				return guns[Random.Range(0, count)];
			}
			return Enumerable.FirstOrDefault<string>(guns);
		}
		protected int Damage1
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage1, this.Config.Damage1Hard, this.Config.Damage1Lunatic, "Damage1");
			}
		}
		protected int Damage2
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage2, this.Config.Damage2Hard, this.Config.Damage2Lunatic, "Damage2");
			}
		}
		protected int Damage3
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage3, this.Config.Damage3Hard, this.Config.Damage3Lunatic, "Damage3");
			}
		}
		protected int Damage4
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Damage4, this.Config.Damage4Hard, this.Config.Damage4Lunatic, "Damage4");
			}
		}
		protected int Power
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Power, this.Config.PowerHard, this.Config.PowerLunatic, "Power");
			}
		}
		protected int Defend
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Defend, this.Config.DefendHard, this.Config.DefendLunatic, "Defend");
			}
		}
		protected int Count1
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Count1, this.Config.Count1Hard, this.Config.Count1Lunatic, "Count1");
			}
		}
		protected int Count2
		{
			get
			{
				return this.GuardedGetAdjustedConfigValue(this.Config.Count2, this.Config.Count2Hard, this.Config.Count2Lunatic, "Count2");
			}
		}
		protected int CountDown { get; set; }
		protected IEnumerable<EnemyUnit> AllAliveEnemies
		{
			get
			{
				return base.Battle.AllAliveEnemies;
			}
		}
		protected RandomGen EnemyMoveRng
		{
			get
			{
				return base.Battle.GameRun.EnemyMoveRng;
			}
		}
		protected RandomGen EnemyBattleRng
		{
			get
			{
				return base.Battle.GameRun.EnemyBattleRng;
			}
		}
		private protected GameDifficulty Difficulty { protected get; private set; }
		public virtual int MovePriority
		{
			get
			{
				return 10;
			}
		}
		public List<Intention> Intentions { get; protected set; } = new List<Intention>();
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
		private int GuardedGetAdjustedConfigValue(int? easyAndNormal, int? hard, int? lunatic, [CallerMemberName] string memberName = "")
		{
			int? adjustedConfigValue = this.GetAdjustedConfigValue(easyAndNormal, hard, lunatic);
			if (adjustedConfigValue == null)
			{
				throw new InvalidDataException(this.DebugName + " has no '" + memberName + "' in config");
			}
			return adjustedConfigValue.GetValueOrDefault();
		}
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
		protected void SetMaxHpInBattle(int hp, int maxHp)
		{
			base.SetMaxHp(hp, maxHp);
		}
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
		public virtual void OnSpawn(EnemyUnit spawner)
		{
		}
		internal override void Die()
		{
			base.Die();
			this.Intentions.Clear();
			this.NotifyIntentionsChanged();
		}
		internal void Escape()
		{
			this.Intentions.Clear();
			this.NotifyIntentionsChanged();
		}
		internal IEnumerable<BattleAction> GetActions()
		{
			return this.Actions();
		}
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
		protected virtual IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield break;
		}
		protected virtual void UpdateMoveCounters()
		{
		}
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
		protected IEnemyMove AttackMove(string move, [CanBeNull] string gunName, int damage, int times = 1, bool isAccuracy = false, string followGunName = "Instant", bool withSpell = false)
		{
			return new SimpleEnemyMove(withSpell ? ((times > 1) ? Intention.Attack(damage, times, isAccuracy).WithMoveName(move) : Intention.Attack(damage, isAccuracy).WithMoveName(move)) : ((times > 1) ? Intention.Attack(damage, times, isAccuracy) : Intention.Attack(damage, isAccuracy)), this.AttackActions(move, gunName, damage, times, isAccuracy, followGunName));
		}
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
		protected IEnemyMove DefendMove(Unit target, [CanBeNull] string move, int block = 0, int shield = 0, int graze = 0, bool cast = true, PerformAction performAction = null)
		{
			if (block == 0 && shield == 0 && graze == 0)
			{
				throw new ArgumentException("Cannot create defend move with block = 0 AND shield = 0 AND graze = 0");
			}
			return new SimpleEnemyMove((block > 0 || shield > 0) ? Intention.Defend() : Intention.Graze(), this.DefendActions(target, move, block, shield, graze, cast, performAction));
		}
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
		protected IEnemyMove AddCardMove([CanBeNull] string move, IEnumerable<Card> cards, EnemyUnit.AddCardZone zone = EnemyUnit.AddCardZone.Discard, PerformAction perform = null, bool withSpell = false)
		{
			if (!withSpell)
			{
				return new SimpleEnemyMove(Intention.AddCard(), this.AddCardActions(move, cards, zone, perform));
			}
			return new SimpleEnemyMove(Intention.AddCard().WithMoveName(move), this.AddCardActions(move, cards, zone, perform));
		}
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
		protected IEnemyMove PositiveMove([CanBeNull] string move, Type type, int? level = null, int? duration = null, bool withSpell = false, PerformAction performAction = null)
		{
			return new SimpleEnemyMove(withSpell ? Intention.PositiveEffect().WithMoveName(move) : Intention.PositiveEffect(), this.PositiveActions(move, type, level, duration, 0f, performAction));
		}
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
		protected IEnemyMove NegativeMove([CanBeNull] string move, Type type, int? level = null, int? duration = null, bool startAutoDecreasing = true, bool withSpell = false, PerformAction performAction = null)
		{
			return new SimpleEnemyMove(withSpell ? Intention.NegativeEffect(null).WithMoveName(move) : Intention.NegativeEffect(null), this.NegativeActions(move, type, level, duration, startAutoDecreasing, performAction));
		}
		protected IEnumerable<BattleAction> PerformActions([CanBeNull] string move, PerformAction performAction)
		{
			if (move != null)
			{
				yield return new EnemyMoveAction(this, move, true);
			}
			yield return performAction;
			yield break;
		}
		protected IEnemyMove ClearMove(float occupationTime = 0f)
		{
			return new SimpleEnemyMove(Intention.Clear(), new RemoveAllNegativeStatusEffectAction(this, occupationTime));
		}
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
		public void ClearIntentions()
		{
			this.Intentions.Clear();
			this.NotifyIntentionsChanged();
		}
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
		public event Action<EnemyUnit> IntentionsChanged;
		public void NotifyIntentionsChanged()
		{
			Action<EnemyUnit> intentionsChanged = this.IntentionsChanged;
			if (intentionsChanged == null)
			{
				return;
			}
			intentionsChanged.Invoke(this);
		}
		public override void SetView(IUnitView view)
		{
			base.SetView(view);
			IEnemyUnitView enemyUnitView = view as IEnemyUnitView;
			if (enemyUnitView != null)
			{
				this.View = enemyUnitView;
			}
		}
		public new IEnemyUnitView View { get; private set; }
		public GameEvent<DieEventArgs> EnemyPointGenerating { get; } = new GameEvent<DieEventArgs>();
		private readonly List<IEnemyMove> _turnMoves = new List<IEnemyMove>();
		public enum AddCardZone
		{
			Discard,
			Draw,
			Hand
		}
	}
}
