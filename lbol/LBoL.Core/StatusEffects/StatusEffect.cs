using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.StatusEffects
{
	[Localizable]
	public abstract class StatusEffect : GameEntity, IDisplayWord, IEquatable<IDisplayWord>, INotifyActivating
	{
		protected BattleAction BuffAction(Type type, int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			Unit player = this.Battle.Player;
			int? num = new int?(level);
			int? num2 = new int?(duration);
			int? num3 = new int?(limit);
			return new ApplyStatusEffectAction(type, player, num, num2, new int?(count), num3, occupationTime, true);
		}
		protected BattleAction BuffAction<TEffect>(int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			return this.BuffAction(typeof(TEffect), level, duration, limit, count, occupationTime);
		}
		protected BattleAction DebuffAction(Type type, Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			int? num = new int?(level);
			int? num2 = new int?(duration);
			int? num3 = new int?(limit);
			return new ApplyStatusEffectAction(type, target, num, num2, new int?(count), num3, occupationTime, startAutoDecreasing);
		}
		protected BattleAction DebuffAction<TEffect>(Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), target, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}
		protected IEnumerable<BattleAction> DebuffAction(Type type, IEnumerable<Unit> targets, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			List<Unit> list = Enumerable.ToList<Unit>(targets);
			foreach (Unit unit in list)
			{
				Unit unit2 = unit;
				int? num = new int?(level);
				int? num2 = new int?(duration);
				int? num3 = new int?(limit);
				ApplyStatusEffectAction applyStatusEffectAction = new ApplyStatusEffectAction(type, unit2, num, num2, new int?(count), num3, (unit == Enumerable.LastOrDefault<Unit>(list)) ? occupationTime : 0f, startAutoDecreasing);
				yield return applyStatusEffectAction;
			}
			List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
			yield break;
			yield break;
		}
		protected IEnumerable<BattleAction> DebuffAction<TEffect>(IEnumerable<Unit> targets, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), targets, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}
		protected BattleAction UpgradeAllHandsAction()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.HandZone, (Card card) => card.CanUpgradeAndPositive));
			if (list.Count <= 0)
			{
				return null;
			}
			return new UpgradeCardsAction(list);
		}
		protected StatusEffect()
		{
			Type type = base.GetType();
			if (!StatusEffect.OpposeHandlerDict.ContainsKey(type))
			{
				StatusEffect.OpposeHandlerDict.Add(type, new StatusEffect.OpposeHandler(type));
			}
		}
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<StatusEffect>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<StatusEffect>.LocalizeListProperty(base.Id, key, required);
		}
		public override string Name
		{
			get
			{
				string text;
				try
				{
					text = base.BaseName.RuntimeFormat(this);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					text = "<" + this.DebugName + " Error>";
				}
				return text;
			}
		}
		public string Brief
		{
			get
			{
				return this.LocalizeProperty("Brief", true, false);
			}
		}
		string IDisplayWord.Description
		{
			get
			{
				string brief = this.Brief;
				return ((brief != null) ? brief.RuntimeFormat(base.FormatWrapper) : null) ?? ("<" + base.GetType().Name + ".Brief>");
			}
		}
		bool IDisplayWord.IsVerbose
		{
			get
			{
				return false;
			}
		}
		public virtual string IconName
		{
			get
			{
				return base.Id;
			}
		}
		public virtual string OverrideIconName
		{
			get
			{
				return this.Config.ImageId;
			}
		}
		public StatusEffectConfig Config { get; private set; }
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}
		public StatusEffectType Type
		{
			get
			{
				return this.Config.Type;
			}
		}
		public virtual bool ForceNotShowDownText { get; set; }
		public bool HasLevel
		{
			get
			{
				return this.Config.HasLevel;
			}
		}
		public int Level
		{
			get
			{
				if (!this.HasLevel)
				{
					throw new InvalidOperationException("<" + this.DebugName + "> has no level");
				}
				return this._level;
			}
			set
			{
				if (!this.HasLevel)
				{
					throw new InvalidOperationException("<" + this.DebugName + "> has no level");
				}
				if (value < 0)
				{
					throw new ArgumentException(string.Format("Negative level '{0}' for {1}", value, this.DebugName));
				}
				int num = Math.Min(value, 999);
				if (this._level == num)
				{
					return;
				}
				this._level = num;
				this.NotifyChanged();
			}
		}
		internal void SetInitLevel(int level)
		{
			this._level = level;
		}
		public bool ShowCount
		{
			get
			{
				return this._showCount;
			}
			set
			{
				this._showCount = value;
				this.NotifyChanged();
			}
		}
		public bool HasCount
		{
			get
			{
				return this.Config.HasCount;
			}
		}
		public int Count
		{
			get
			{
				if (!this.HasCount)
				{
					throw new InvalidOperationException("<" + this.DebugName + "> has no count");
				}
				return this._count;
			}
			set
			{
				if (!this.HasCount)
				{
					throw new InvalidOperationException("<" + this.DebugName + "> has no count");
				}
				if (value < 0)
				{
					throw new ArgumentException(string.Format("Negative count '{0}' for {1}", value, this.DebugName));
				}
				int num = Math.Min(value, 999);
				if (this._count == num)
				{
					return;
				}
				this._count = num;
				this.NotifyChanged();
			}
		}
		internal void SetInitCount(int count)
		{
			this._count = count;
		}
		public int Limit
		{
			get
			{
				return this._limit;
			}
			set
			{
				this._limit = value;
				this.NotifyChanged();
			}
		}
		public bool HasDuration
		{
			get
			{
				return this.Config.HasDuration;
			}
		}
		public int Duration
		{
			get
			{
				if (!this.HasDuration)
				{
					throw new InvalidOperationException("<" + this.DebugName + "> has no duration");
				}
				return this._duration;
			}
			set
			{
				if (!this.HasDuration)
				{
					throw new InvalidOperationException("<" + this.DebugName + "> has no duration");
				}
				if (value < 0)
				{
					throw new ArgumentException(string.Format("Negative duration '{0}' for {1}", value, this.DebugName));
				}
				int num = Math.Min(value, 999);
				if (this._duration == num)
				{
					return;
				}
				this._duration = num;
				this.NotifyChanged();
			}
		}
		internal void SetInitDuration(int duration)
		{
			this._duration = duration;
		}
		public bool IsStackable
		{
			get
			{
				return this.Config.IsStackable;
			}
		}
		[UsedImplicitly]
		public int TriggerLevel
		{
			get
			{
				return this.Config.StackActionTriggerLevel.GetValueOrDefault();
			}
		}
		public bool ShowPlusByLimit
		{
			get
			{
				return this.Config.ShowPlusByLimit;
			}
		}
		public virtual bool Stack(StatusEffect other)
		{
			if (!this.IsStackable)
			{
				return false;
			}
			int? stackActionTriggerLevel = this.Config.StackActionTriggerLevel;
			if (stackActionTriggerLevel != null)
			{
				int valueOrDefault = stackActionTriggerLevel.GetValueOrDefault();
				this._level += other._level;
				if (this._level >= valueOrDefault)
				{
					int num = this._level / valueOrDefault;
					this._level %= valueOrDefault;
					this.NotifyChanged();
					this.NotifyActivating();
					if (this._level == 0)
					{
						this.React(new RemoveStatusEffectAction(this, true, 0.1f));
					}
					this.React(new Reactor(this.StackAction(this.Owner, num)));
				}
				else
				{
					this.NotifyChanged();
				}
				return true;
			}
			StackType? stackType;
			if (this.HasLevel)
			{
				stackType = this.Config.LevelStackType;
				if (stackType != null)
				{
					int num2;
					switch (stackType.GetValueOrDefault())
					{
					case StackType.Add:
						num2 = Math.Min(this.Level + other.Level, 999);
						break;
					case StackType.Max:
						num2 = Math.Max(this.Level, other.Level);
						break;
					case StackType.Min:
						num2 = Math.Min(this.Level, other.Level);
						break;
					case StackType.Keep:
						num2 = this.Level;
						break;
					case StackType.Overwrite:
						num2 = other.Level;
						break;
					default:
						goto IL_0145;
					}
					this._level = num2;
					goto IL_0172;
				}
				IL_0145:
				throw new InvalidDataException(string.Format("Invalid stack type {0} for {1}", this.Config.LevelStackType, this.DebugName));
			}
			IL_0172:
			if (this.HasDuration)
			{
				stackType = this.Config.DurationStackType;
				if (stackType != null)
				{
					int num2;
					switch (stackType.GetValueOrDefault())
					{
					case StackType.Add:
						num2 = Math.Min(this.Duration + other.Duration, 999);
						break;
					case StackType.Max:
						num2 = Math.Max(this.Duration, other.Duration);
						break;
					case StackType.Min:
						num2 = Math.Max(this.Duration, other.Duration);
						break;
					case StackType.Keep:
						num2 = this.Duration;
						break;
					case StackType.Overwrite:
						num2 = other.Duration;
						break;
					default:
						goto IL_020D;
					}
					this._duration = num2;
					goto IL_023A;
				}
				IL_020D:
				throw new InvalidDataException(string.Format("Invalid stack type {0} for {1}", this.Config.DurationStackType, this.DebugName));
			}
			IL_023A:
			if (this.HasCount)
			{
				stackType = this.Config.CountStackType;
				if (stackType != null)
				{
					int num2;
					switch (stackType.GetValueOrDefault())
					{
					case StackType.Add:
						num2 = Math.Min(this.Count + other.Count, 999);
						break;
					case StackType.Max:
						num2 = Math.Max(this.Count, other.Count);
						break;
					case StackType.Min:
						num2 = Math.Min(this.Count, other.Count);
						break;
					case StackType.Keep:
						num2 = this.Count;
						break;
					case StackType.Overwrite:
						num2 = other.Count;
						break;
					default:
						goto IL_02D5;
					}
					this._count = num2;
					goto IL_0302;
				}
				IL_02D5:
				throw new InvalidDataException(string.Format("Invalid stack type {0} for {1}", this.Config.CountStackType, this.DebugName));
			}
			IL_0302:
			stackType = this.Config.LimitStackType;
			if (stackType != null)
			{
				int num2;
				switch (stackType.GetValueOrDefault())
				{
				case StackType.Add:
					num2 = this.Limit + other.Limit;
					break;
				case StackType.Max:
					num2 = Math.Max(this.Limit, other.Limit);
					break;
				case StackType.Min:
					num2 = Math.Min(this.Limit, other.Limit);
					break;
				case StackType.Keep:
					num2 = this.Limit;
					break;
				case StackType.Overwrite:
					num2 = other.Limit;
					break;
				default:
					goto IL_0388;
				}
				this._limit = num2;
				this.IsAutoDecreasing = this.IsAutoDecreasing || other.IsAutoDecreasing;
				this.NotifyChanged();
				return true;
			}
			IL_0388:
			throw new InvalidDataException(string.Format("Invalid stack type {0} for {1}", this.Config.CountStackType, this.DebugName));
		}
		internal void ClampMax()
		{
			if (this.HasLevel)
			{
				this.Level = Math.Min(this.Level, 999);
			}
			if (this.HasDuration)
			{
				this.Duration = Math.Min(this.Duration, 999);
			}
			if (this.HasCount)
			{
				this.Count = Math.Min(this.Count, 999);
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			this.Config = StatusEffectConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find status-effect config for " + base.Id);
			}
			if (this.IsStackable)
			{
				if (this.HasLevel && this.Config.LevelStackType == null)
				{
					throw new InvalidDataException(this.DebugName + " is stackable but LevelStackType is null");
				}
				if (this.HasDuration && this.Config.DurationStackType == null)
				{
					throw new InvalidDataException(this.DebugName + " is stackable but DurationStackType is null");
				}
				if (this.HasCount && this.Config.CountStackType == null)
				{
					throw new InvalidDataException(this.DebugName + " is stackable but CountStackType is null");
				}
			}
			if (this.Config.StackActionTriggerLevel != null)
			{
				if (!this.IsStackable)
				{
					throw new InvalidDataException(this.DebugName + " has 'StackActionTriggerLevel' but is not stackable");
				}
				if (!this.HasLevel)
				{
					throw new InvalidDataException(this.DebugName + " has 'StackActionTriggerLevel' but does not have level");
				}
				if (this.Config.LevelStackType != null && this.Config.LevelStackType.Value != StackType.Add)
				{
					throw new InvalidDataException(string.Format("{0} has '{1}' but has '{2}' which is not 'Add'", this.DebugName, "StackActionTriggerLevel", this.Config.LevelStackType));
				}
				if (this.HasDuration)
				{
					throw new InvalidDataException(this.DebugName + " has 'StackActionTriggerLevel' but also has duration");
				}
				if (this.HasCount)
				{
					throw new InvalidDataException(this.DebugName + " has 'StackActionTriggerLevel' but also has count");
				}
			}
		}
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new StatusEffect.StatusEffectFormatWrapper(this);
		}
		public Unit Owner
		{
			get
			{
				Unit unit;
				if (!this._owner.TryGetTarget(ref unit))
				{
					return null;
				}
				return unit;
			}
			internal set
			{
				this._owner.SetTarget(value);
			}
		}
		internal bool IsAutoDecreasing { get; set; }
		public BattleController Battle
		{
			get
			{
				Unit owner = this.Owner;
				if (owner == null)
				{
					return null;
				}
				return owner.Battle;
			}
		}
		public Card SourceCard { get; set; }
		internal void TriggerAdding(Unit unit)
		{
			this.OnAdding(unit);
		}
		internal void TriggerAdded(Unit unit)
		{
			this.OnAdded(unit);
		}
		internal void TriggerRemoving(Unit unit)
		{
			this.OnRemoving(unit);
		}
		internal void TriggerRemoved(Unit unit)
		{
			this.OnRemoved(unit);
			this._ownerHandlerHolder.ClearEventHandlers();
		}
		protected virtual void OnAdding(Unit unit)
		{
		}
		protected virtual void OnAdded(Unit unit)
		{
		}
		protected virtual void OnRemoving(Unit unit)
		{
		}
		protected virtual void OnRemoved(Unit unit)
		{
		}
		public event Action Activating;
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}
		public bool Highlight
		{
			get
			{
				return this._highlight;
			}
			set
			{
				if (this._highlight != value)
				{
					this._highlight = value;
					this.NotifyChanged();
				}
			}
		}
		public override void NotifyChanged()
		{
			base.NotifyChanged();
			BattleController battle = this.Battle;
			if (battle != null)
			{
				battle.TriggerGlobalStatusChanged();
			}
			StatusEffectEventArgs statusEffectEventArgs = new StatusEffectEventArgs
			{
				Effect = this,
				CanCancel = false
			};
			Unit owner = this.Owner;
			if (owner == null)
			{
				return;
			}
			owner.StatusEffectChanged.Execute(statusEffectEventArgs);
		}
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[StatusEffect: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.StatusEffect);
		}
		protected void HandleOwnerEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._ownerHandlerHolder.HandleEvent<T>(@event, handler, priority);
		}
		protected void HandleOwnerEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleOwnerEvent<T>(@event, handler, this.DefaultEventPriority);
		}
		protected void ReactOwnerEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleOwnerEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}
		protected void ReactOwnerEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.ReactOwnerEvent<TEventArgs>(@event, reactor, this.DefaultEventPriority);
		}
		public virtual IEnumerable<BattleAction> StackAction(Unit targetOwner, int targetLevel)
		{
			throw new InvalidOperationException(this.DebugName + " has no stack action");
		}
		internal OpposeResult? Oppose(StatusEffect effect)
		{
			return StatusEffect.OpposeHandlerDict[base.GetType()].Oppose(this, effect);
		}
		bool IEquatable<IDisplayWord>.Equals(IDisplayWord other)
		{
			StatusEffect statusEffect = other as StatusEffect;
			return statusEffect != null && base.GetType() == statusEffect.GetType();
		}
		protected virtual Keyword Keywords
		{
			get
			{
				return this.Config.Keywords;
			}
		}
		protected virtual IEnumerable<string> RelativeEffects
		{
			get
			{
				return this.Config.RelativeEffects;
			}
		}
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Keywords, this.RelativeEffects, verbose, default(Keyword?));
		}
		public virtual bool ShouldPreventCardUsage(Card card)
		{
			return false;
		}
		public virtual string PreventCardUsageMessage
		{
			get
			{
				throw new InvalidOperationException("Cannot get prevent card message key for " + this.DebugName);
			}
		}
		public virtual string UnitEffectName
		{
			get
			{
				return null;
			}
		}
		public int GetSeLevel(Type seType)
		{
			if (this.Battle == null || !this.Battle.Player.HasStatusEffect(seType))
			{
				return 0;
			}
			return this.Battle.Player.GetStatusEffect(seType).Level;
		}
		public int GetSeLevel<T>() where T : StatusEffect
		{
			return this.GetSeLevel(typeof(T));
		}
		[UsedImplicitly]
		public string SourceCardName
		{
			get
			{
				return UiUtils.WrapByColor(this.SourceCard.Name, GlobalConfig.EntityColor);
			}
		}
		protected string ExtraDescription
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription", true, true);
			}
		}
		protected string ExtraDescription2
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription2", true, true);
			}
		}
		private static readonly Dictionary<Type, StatusEffect.OpposeHandler> OpposeHandlerDict = new Dictionary<Type, StatusEffect.OpposeHandler>();
		private int _level;
		private bool _showCount = true;
		private int _count;
		private int _limit;
		private int _duration;
		private readonly WeakReference<Unit> _owner = new WeakReference<Unit>(null);
		private bool _highlight;
		private readonly GameEventHandlerHolder _ownerHandlerHolder = new GameEventHandlerHolder();
		private class OpposeHandler
		{
			public OpposeHandler(Type type)
			{
				foreach (Type type2 in type.GetInterfaces())
				{
					if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IOpposing<>))
					{
						Type type3 = type2.GetGenericArguments()[0];
						MethodInfo method = type2.GetMethod("Oppose");
						this._opposingTypes.Add(new ValueTuple<Type, Func<StatusEffect, StatusEffect, OpposeResult>>(type3, (StatusEffect self, StatusEffect other) => (OpposeResult)method.Invoke(self, new object[] { other })));
					}
				}
			}
			public OpposeResult? Oppose(StatusEffect self, StatusEffect effect)
			{
				Type type = effect.GetType();
				foreach (ValueTuple<Type, Func<StatusEffect, StatusEffect, OpposeResult>> valueTuple in this._opposingTypes)
				{
					Type item = valueTuple.Item1;
					Func<StatusEffect, StatusEffect, OpposeResult> item2 = valueTuple.Item2;
					if (item.IsAssignableFrom(type))
					{
						return new OpposeResult?(item2.Invoke(self, effect));
					}
				}
				return default(OpposeResult?);
			}
			[TupleElementNames(new string[] { "otherType", "handler" })]
			private readonly List<ValueTuple<Type, Func<StatusEffect, StatusEffect, OpposeResult>>> _opposingTypes = new List<ValueTuple<Type, Func<StatusEffect, StatusEffect, OpposeResult>>>();
		}
		internal sealed class StatusEffectFormatWrapper : GameEntityFormatWrapper
		{
			public StatusEffectFormatWrapper(StatusEffect effect)
				: base(effect)
			{
				this._effect = effect;
			}
			protected override object GetArgument(string key)
			{
				if (!(key == "OwnerName"))
				{
					return base.GetArgument(key);
				}
				Unit owner = this._effect.Owner;
				return ((owner != null) ? owner.GetName() : null) ?? UnitNameTable.GetDefaultOwnerName();
			}
			protected override string FormatArgument(object arg, string format)
			{
				if (arg is DamageInfo)
				{
					DamageInfo damageInfo = (DamageInfo)arg;
					int num = (int)damageInfo.Damage;
					BattleController battle = this._effect.Battle;
					if (battle == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(num, num, format);
					}
					int num2 = battle.CalculateDamage(this._effect, battle.Player, null, damageInfo);
					return GameEntityFormatWrapper.WrappedFormatNumber(num, num2, format);
				}
				else
				{
					if (!(arg is ScryInfo))
					{
						return base.FormatArgument(arg, format);
					}
					int count = ((ScryInfo)arg).Count;
					BattleController battle2 = this._effect.Battle;
					if (battle2 == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(count, count, format);
					}
					int num3 = battle2.CalculateScry(this._effect, count);
					return GameEntityFormatWrapper.WrappedFormatNumber(count, num3, format);
				}
			}
			private readonly StatusEffect _effect;
		}
	}
}
