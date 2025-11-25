using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using UnityEngine;
namespace LBoL.Core.Units
{
	public abstract class Unit : GameEntity
	{
		public int TurnCounter { get; internal set; }
		public bool IsExtraTurn { get; internal set; }
		public bool IsInTurn { get; internal set; }
		public int Hp { get; internal set; }
		public int MaxHp { get; protected set; }
		public int Shield { get; internal set; }
		public int Block { get; internal set; }
		public UnitStatus Status { get; internal set; }
		public bool IsAlive
		{
			get
			{
				return this.Status == UnitStatus.Alive;
			}
		}
		public bool IsNotAlive
		{
			get
			{
				return this.Status > UnitStatus.Alive;
			}
		}
		public bool IsDying
		{
			get
			{
				return this.Status == UnitStatus.Dying;
			}
		}
		public bool IsDead
		{
			get
			{
				return this.Status == UnitStatus.Dead;
			}
		}
		public bool IsEscaped
		{
			get
			{
				return this.Status == UnitStatus.Escaped;
			}
		}
		public bool IsInvalidTarget
		{
			get
			{
				UnitStatus status = this.Status;
				return status == UnitStatus.Dead || status == UnitStatus.Escaped;
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			this.Hp = this.MaxHp;
		}
		public abstract UnitName GetName();
		public override string Name
		{
			get
			{
				return this.GetName().ToString();
			}
		}
		public string NameWithColor
		{
			get
			{
				return this.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default);
			}
		}
		public string ShortName
		{
			get
			{
				return this.GetName().ToString(UnitNameStyle.Short);
			}
		}
		public string ShortNameWithColor
		{
			get
			{
				return this.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Short);
			}
		}
		public string FullName
		{
			get
			{
				return this.GetName().ToString(UnitNameStyle.Full);
			}
		}
		public string FullNameWithColor
		{
			get
			{
				return this.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Full);
			}
		}
		public BattleController Battle
		{
			get
			{
				BattleController battleController;
				if (!this._battle.TryGetTarget(ref battleController))
				{
					return null;
				}
				return battleController;
			}
			private set
			{
				this._battle.SetTarget(value);
			}
		}
		protected void HandleGameRunEvent<T>(GameEvent<T> e, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._gameRunHandlerHolder.HandleEvent<T>(e, handler, priority);
		}
		protected void HandleGameRunEvent<T>(GameEvent<T> e, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleGameRunEvent<T>(e, handler, this.DefaultEventPriority);
		}
		protected void HandleBattleEvent<T>(GameEvent<T> e, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<T>(e, handler, priority);
		}
		protected void HandleBattleEvent<T>(GameEvent<T> e, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleBattleEvent<T>(e, handler, this.DefaultEventPriority);
		}
		protected void ReactBattleEvent<T>(GameEvent<T> e, Func<T, IEnumerable<BattleAction>> reactor, GameEventPriority priority) where T : GameEventArgs
		{
			this.HandleBattleEvent<T>(e, delegate(T args)
			{
				this.React(reactor.Invoke(args));
			}, priority);
		}
		protected void ReactBattleEvent<T>(GameEvent<T> e, Func<T, IEnumerable<BattleAction>> reactor) where T : GameEventArgs
		{
			this.ReactBattleEvent<T>(e, reactor, this.DefaultEventPriority);
		}
		internal virtual void EnterGameRun(GameRunController gameRun)
		{
			if (base.GameRun != null)
			{
				throw new InvalidOperationException("Cannot enter game-run while already in game-run");
			}
			base.GameRun = gameRun;
			this.OnEnterGameRun(gameRun);
		}
		internal void LeaveGameRun()
		{
			if (base.GameRun == null)
			{
				throw new InvalidOperationException("Cannot leave game-run while not in game-run");
			}
			this._gameRunHandlerHolder.ClearEventHandlers();
			this.OnLeaveGameRun();
			base.GameRun = null;
		}
		protected virtual void OnEnterGameRun(GameRunController gameRun)
		{
		}
		protected virtual void OnLeaveGameRun()
		{
		}
		internal void EnterBattle(BattleController battle)
		{
			if (base.GameRun != battle.GameRun)
			{
				throw new InvalidOperationException("Cannot enter battle while in different game-run");
			}
			if (this.Battle != null)
			{
				throw new InvalidOperationException("Cannot enter battle while already in battle");
			}
			this.Battle = battle;
			PlayerUnit playerUnit = this as PlayerUnit;
			if (playerUnit != null)
			{
				playerUnit.SetDollSlot(4);
			}
			this.OnEnterBattle(battle);
		}
		internal void LeaveBattle()
		{
			if (this.Battle == null)
			{
				throw new InvalidOperationException("Cannot leave battle while not in battle");
			}
			this.TurnCounter = 0;
			this.ClearBlockShield();
			this.ClearStatusEffects();
			PlayerUnit playerUnit = this as PlayerUnit;
			if (playerUnit != null)
			{
				playerUnit.SetDollSlot(0);
			}
			this._battleHandlerHolder.ClearEventHandlers();
			this.OnLeaveBattle();
			this.Battle = null;
		}
		protected virtual void OnEnterBattle(BattleController battle)
		{
		}
		protected virtual void OnLeaveBattle()
		{
		}
		protected override void React(Reactor reactor)
		{
			this.Battle.React(reactor, this, ActionCause.Unit);
		}
		public IReadOnlyList<StatusEffect> StatusEffects
		{
			get
			{
				return this._statusEffects.AsReadOnly();
			}
		}
		public StatusEffect GetStatusEffect(Type type)
		{
			return Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == type);
		}
		public T GetStatusEffect<T>() where T : StatusEffect
		{
			return (T)((object)Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == typeof(T)));
		}
		public StatusEffect GetStatusEffectExtend(Type type)
		{
			return Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType().IsSubclassOf(type));
		}
		public T GetStatusEffectExtend<T>() where T : StatusEffect
		{
			return (T)((object)Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType().IsSubclassOf(typeof(T))));
		}
		public bool TryGetStatusEffect(Type type, out StatusEffect statusEffect)
		{
			statusEffect = Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == type);
			return statusEffect != null;
		}
		public bool TryGetStatusEffect<T>(out T statusEffect) where T : StatusEffect
		{
			statusEffect = Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == typeof(T)) as T;
			return statusEffect != null;
		}
		public bool HasStatusEffect(Type type)
		{
			return Enumerable.Any<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == type);
		}
		public bool HasStatusEffect<T>() where T : StatusEffect
		{
			return Enumerable.Any<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == typeof(T));
		}
		public bool HasStatusEffect(StatusEffect effect)
		{
			return this._statusEffects.Contains(effect);
		}
		internal StatusEffectAddResult? TryAddStatusEffect(StatusEffect effect)
		{
			if (effect.Owner != null)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Try to add effect ",
					effect.DebugName,
					" of ",
					effect.Owner.DebugName,
					" to ",
					this.DebugName
				}));
				StatusEffectAddResult? statusEffectAddResult = default(StatusEffectAddResult?);
				return statusEffectAddResult;
			}
			foreach (StatusEffect statusEffect in this._statusEffects)
			{
				OpposeResult? opposeResult = statusEffect.Oppose(effect);
				OpposeResult? opposeResult2 = opposeResult;
				OpposeResult opposeResult3 = OpposeResult.KeepSelf;
				if ((opposeResult2.GetValueOrDefault() == opposeResult3) & (opposeResult2 != null))
				{
					return new StatusEffectAddResult?(StatusEffectAddResult.Opposed);
				}
				opposeResult2 = opposeResult;
				opposeResult3 = OpposeResult.Neutralize;
				if ((opposeResult2.GetValueOrDefault() == opposeResult3) & (opposeResult2 != null))
				{
					this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f));
					return new StatusEffectAddResult?(StatusEffectAddResult.Neutralized);
				}
				opposeResult2 = opposeResult;
				opposeResult3 = OpposeResult.KeepOther;
				if ((opposeResult2.GetValueOrDefault() == opposeResult3) & (opposeResult2 != null))
				{
					this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f));
				}
			}
			StatusEffect statusEffect2 = this.GetStatusEffect(effect.GetType());
			if (statusEffect2 != null && statusEffect2.Stack(effect))
			{
				return new StatusEffectAddResult?(StatusEffectAddResult.Stacked);
			}
			int? stackActionTriggerLevel = effect.Config.StackActionTriggerLevel;
			if (stackActionTriggerLevel != null)
			{
				int valueOrDefault = stackActionTriggerLevel.GetValueOrDefault();
				if (effect.Level >= valueOrDefault)
				{
					int num = effect.Level / valueOrDefault;
					effect.Level %= valueOrDefault;
					this.Battle.React(new Reactor(effect.StackAction(this, num)), effect, ActionCause.StatusEffect);
				}
				if (effect.Level == 0)
				{
					return new StatusEffectAddResult?(StatusEffectAddResult.Stacked);
				}
			}
			effect.Owner = this;
			effect.TriggerAdding(this);
			effect.ClampMax();
			this._statusEffects.Add(effect);
			effect.TriggerAdded(this);
			if (effect.Type == StatusEffectType.Positive && this is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				if (Enumerable.Count<StatusEffect>(this._statusEffects, (StatusEffect e) => e.Type == StatusEffectType.Positive) >= 20)
				{
					base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.JiangshenYishi);
				}
			}
			return new StatusEffectAddResult?(StatusEffectAddResult.Added);
		}
		internal bool TryRemoveStatusEffect(StatusEffect effect)
		{
			if (!this._statusEffects.Contains(effect))
			{
				return false;
			}
			effect.TriggerRemoving(this);
			this._statusEffects.Remove(effect);
			effect.TriggerRemoved(this);
			effect.Owner = null;
			return true;
		}
		private void ClearBlockShield()
		{
			this.Block = 0;
			this.Shield = 0;
		}
		private void ClearStatusEffects()
		{
			foreach (StatusEffect statusEffect in Enumerable.ToList<StatusEffect>(this._statusEffects))
			{
				if (!this.TryRemoveStatusEffect(statusEffect))
				{
					Debug.LogError("Cannot clear " + statusEffect.DebugName + " on " + this.DebugName);
				}
			}
			if (Enumerable.Any<StatusEffect>(this._statusEffects))
			{
				string[] array = new string[5];
				array[0] = "Clearing ";
				array[1] = this.DebugName;
				array[2] = "'s effect incomplete: [";
				array[3] = string.Join(", ", Enumerable.Select<StatusEffect, string>(this._statusEffects, (StatusEffect e) => e.DebugName));
				array[4] = "]";
				Debug.LogError(string.Concat(array));
			}
		}
		public int TotalFirepower
		{
			get
			{
				TempFirepower statusEffect = this.GetStatusEffect<TempFirepower>();
				int num = ((statusEffect != null) ? statusEffect.Level : 0);
				Firepower statusEffect2 = this.GetStatusEffect<Firepower>();
				int num2 = num + ((statusEffect2 != null) ? statusEffect2.Level : 0);
				TempFirepowerNegative statusEffect3 = this.GetStatusEffect<TempFirepowerNegative>();
				int num3 = num2 - ((statusEffect3 != null) ? statusEffect3.Level : 0);
				FirepowerNegative statusEffect4 = this.GetStatusEffect<FirepowerNegative>();
				int num4 = num3 - ((statusEffect4 != null) ? statusEffect4.Level : 0);
				Grace statusEffect5 = this.GetStatusEffect<Grace>();
				return num4 + ((statusEffect5 != null) ? statusEffect5.Level : 0);
			}
		}
		public int TotalSpirit
		{
			get
			{
				TempSpirit statusEffect = this.GetStatusEffect<TempSpirit>();
				int num = ((statusEffect != null) ? statusEffect.Level : 0);
				Spirit statusEffect2 = this.GetStatusEffect<Spirit>();
				int num2 = num + ((statusEffect2 != null) ? statusEffect2.Level : 0);
				TempSpiritNegative statusEffect3 = this.GetStatusEffect<TempSpiritNegative>();
				int num3 = num2 - ((statusEffect3 != null) ? statusEffect3.Level : 0);
				SpiritNegative statusEffect4 = this.GetStatusEffect<SpiritNegative>();
				int num4 = num3 - ((statusEffect4 != null) ? statusEffect4.Level : 0);
				Grace statusEffect5 = this.GetStatusEffect<Grace>();
				return num4 + ((statusEffect5 != null) ? statusEffect5.Level : 0);
			}
		}
		internal DamageInfo MeasureDamage(DamageInfo info)
		{
			if (info.Damage < 0f)
			{
				info.Damage = 0f;
			}
			else
			{
				info.Damage = info.Damage.Round(1);
			}
			if (info.DamageType == DamageType.Attack && info.Damage > 0f && this.HasStatusEffect<Graze>() && !info.IsAccuracy)
			{
				return new DamageInfo(0f, info.DamageType, true, false, false).BlockBy(this.Block).ShieldBy(this.Shield);
			}
			if (info.DamageType != DamageType.HpLose)
			{
				return info.BlockBy(this.Block).ShieldBy(this.Shield);
			}
			return info;
		}
		internal DamageInfo TakeDamage(DamageInfo info)
		{
			if (info.IsGrazed && info.IsAccuracy)
			{
				Debug.LogError(string.Format("Taking grazed accurary DamageInfo {0}.", info));
			}
			if (this.IsDead)
			{
				Debug.LogError("Damaging already dead " + this.DebugName);
			}
			if (this.IsDying)
			{
				Debug.LogWarning("Damaging dying unit " + this.DebugName);
			}
			if (info.DamageType == DamageType.Attack)
			{
				if (info.IsGrazed)
				{
					this.GetStatusEffect<Graze>().Activate();
				}
				else if (info.IsAccuracy && this.HasStatusEffect<Graze>())
				{
					this.GetStatusEffect<Graze>().BeenAccurate();
				}
			}
			int num = Unit.<TakeDamage>g__CheckIsInt|100_0(info.Damage, "Damage is not int", "Damage is negative");
			int num2 = Unit.<TakeDamage>g__CheckIsInt|100_0(info.DamageBlocked, "Damage blocked is not int", "Damage blocked is negative");
			int num3 = Unit.<TakeDamage>g__CheckIsInt|100_0(info.DamageShielded, "Damage shielded is not int", "Damage shielded is negative");
			this.Hp -= num;
			if (this.Hp < 0)
			{
				info.OverDamage = -this.Hp;
				this.Hp = 0;
			}
			if (this.Block < num2)
			{
				Debug.LogError(string.Format("{0} taking damage is over-blocked (block={1}, taking={2})", this.DebugName, this.Block, num2));
				this.Block = 0;
			}
			else
			{
				this.Block -= num2;
			}
			if (this.Shield < num3)
			{
				Debug.LogError(string.Format("{0} taking damage is over-shielded (shield={1}, taking={2}", this.DebugName, this.Shield, num3));
				this.Shield = 0;
			}
			else
			{
				this.Shield -= num3;
			}
			return info;
		}
		internal void SetMaxHp(int hp, int maxHp)
		{
			if (hp < 0)
			{
				throw new ArgumentException("Hp cannot be negative", "hp");
			}
			if (maxHp < 0)
			{
				throw new ArgumentException("Max hp cannot be negative", "maxHp");
			}
			this.MaxHp = Math.Min(maxHp, 9999);
			this.Hp = Math.Min(hp, this.MaxHp);
		}
		internal int Heal(int healValue)
		{
			int num = Math.Min(this.MaxHp - this.Hp, healValue);
			this.Hp += num;
			return num;
		}
		[return: TupleElementNames(new string[] { "block", "shield" })]
		internal ValueTuple<int, int> GainBlockShield(float block, float shield)
		{
			int num = Math.Min(block.RoundToInt(1), 9999 - this.Block);
			int num2 = Math.Min(shield.RoundToInt(1), 9999 - this.Shield);
			this.Block += num;
			this.Shield += num2;
			if (this.Shield >= 1000 && this is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.BoliJiejie);
			}
			return new ValueTuple<int, int>(num, num2);
		}
		[return: TupleElementNames(new string[] { "block", "shield" })]
		internal ValueTuple<int, int> LoseBlockShield(float block, float shield)
		{
			int num = Math.Min(this.Block, block.RoundToInt(1));
			int num2 = Math.Min(this.Shield, shield.RoundToInt(1));
			this.Block -= num;
			this.Shield -= num2;
			return new ValueTuple<int, int>(num, num2);
		}
		internal virtual void Die()
		{
			this.OnDie();
			UnitStatus status = this.Status;
			if (status == UnitStatus.Alive || status == UnitStatus.Dead)
			{
				Debug.LogError(string.Format("Unit die when {0}", this.Status));
			}
			this.Status = UnitStatus.Dead;
		}
		protected virtual void OnDie()
		{
		}
		public virtual void SetView(IUnitView view)
		{
			this.View = view;
		}
		public virtual TView GetView<TView>() where TView : class, IUnitView
		{
			TView tview = this.View as TView;
			if (tview == null)
			{
				return default(TView);
			}
			return tview;
		}
		public IUnitView View { get; private set; }
		public GameEvent<UnitEventArgs> TurnStarting { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<UnitEventArgs> TurnStarted { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<UnitEventArgs> TurnEnding { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<UnitEventArgs> TurnEnded { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<BlockShieldEventArgs> BlockShieldCasting { get; } = new GameEvent<BlockShieldEventArgs>();
		public GameEvent<BlockShieldEventArgs> BlockShieldGaining { get; } = new GameEvent<BlockShieldEventArgs>();
		public GameEvent<BlockShieldEventArgs> BlockShieldCasted { get; } = new GameEvent<BlockShieldEventArgs>();
		public GameEvent<BlockShieldEventArgs> BlockShieldGained { get; } = new GameEvent<BlockShieldEventArgs>();
		public GameEvent<BlockShieldEventArgs> BlockShieldLosing { get; } = new GameEvent<BlockShieldEventArgs>();
		public GameEvent<BlockShieldEventArgs> BlockShieldLost { get; } = new GameEvent<BlockShieldEventArgs>();
		public GameEvent<DamageDealingEventArgs> DamageDealing { get; } = new GameEvent<DamageDealingEventArgs>();
		public GameEvent<DamageEventArgs> DamageReceiving { get; } = new GameEvent<DamageEventArgs>();
		public GameEvent<DamageEventArgs> DamageGiving { get; } = new GameEvent<DamageEventArgs>();
		public GameEvent<DamageEventArgs> DamageTaking { get; } = new GameEvent<DamageEventArgs>();
		public GameEvent<DamageEventArgs> DamageDealt { get; } = new GameEvent<DamageEventArgs>();
		public GameEvent<DamageEventArgs> DamageReceived { get; } = new GameEvent<DamageEventArgs>();
		public GameEvent<HealEventArgs> HealingGiving { get; } = new GameEvent<HealEventArgs>();
		public GameEvent<HealEventArgs> HealingReceiving { get; } = new GameEvent<HealEventArgs>();
		public GameEvent<HealEventArgs> HealingGiven { get; } = new GameEvent<HealEventArgs>();
		public GameEvent<HealEventArgs> HealingReceived { get; } = new GameEvent<HealEventArgs>();
		public GameEvent<DollValueArgs> DollValueGenerating { get; } = new GameEvent<DollValueArgs>();
		public GameEvent<StatusEffectApplyEventArgs> StatusEffectAdding { get; } = new GameEvent<StatusEffectApplyEventArgs>();
		public GameEvent<StatusEffectApplyEventArgs> StatusEffectAdded { get; } = new GameEvent<StatusEffectApplyEventArgs>();
		public GameEvent<StatusEffectEventArgs> StatusEffectRemoving { get; } = new GameEvent<StatusEffectEventArgs>();
		public GameEvent<StatusEffectEventArgs> StatusEffectRemoved { get; } = new GameEvent<StatusEffectEventArgs>();
		public GameEvent<StatusEffectEventArgs> StatusEffectChanged { get; } = new GameEvent<StatusEffectEventArgs>();
		public GameEvent<MoodChangeEventArgs> MoodChanging { get; } = new GameEvent<MoodChangeEventArgs>();
		public GameEvent<MoodChangeEventArgs> MoodChanged { get; } = new GameEvent<MoodChangeEventArgs>();
		public GameEvent<DieEventArgs> Dying { get; } = new GameEvent<DieEventArgs>();
		public GameEvent<DieEventArgs> Died { get; } = new GameEvent<DieEventArgs>();
		public GameEvent<UnitEventArgs> Escaping { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<UnitEventArgs> Escaped { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<StatisticalDamageEventArgs> StatisticalTotalDamageDealt { get; } = new GameEvent<StatisticalDamageEventArgs>();
		public GameEvent<StatisticalDamageEventArgs> StatisticalTotalDamageReceived { get; } = new GameEvent<StatisticalDamageEventArgs>();
		[CompilerGenerated]
		internal static int <TakeDamage>g__CheckIsInt|100_0(float value, string roundError, string negativeError)
		{
			int num = (int)value;
			if (!value.Approximately((float)num))
			{
				Debug.LogError(string.Concat(new string[]
				{
					roundError,
					" (",
					num.ToString(),
					" vs ",
					value.ToString(),
					")"
				}));
				return value.RoundToInt(1);
			}
			return num;
		}
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();
		private readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();
		private readonly OrderedList<StatusEffect> _statusEffects = new OrderedList<StatusEffect>((StatusEffect a, StatusEffect b) => a.Config.Order.CompareTo(b.Config.Order));
	}
}
