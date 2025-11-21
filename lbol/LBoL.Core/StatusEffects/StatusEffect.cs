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
	// Token: 0x020000A9 RID: 169
	[Localizable]
	public abstract class StatusEffect : GameEntity, IDisplayWord, IEquatable<IDisplayWord>, INotifyActivating
	{
		// Token: 0x0600079F RID: 1951 RVA: 0x00016854 File Offset: 0x00014A54
		protected BattleAction BuffAction(Type type, int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			Unit player = this.Battle.Player;
			int? num = new int?(level);
			int? num2 = new int?(duration);
			int? num3 = new int?(limit);
			return new ApplyStatusEffectAction(type, player, num, num2, new int?(count), num3, occupationTime, true);
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x00016892 File Offset: 0x00014A92
		protected BattleAction BuffAction<TEffect>(int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			return this.BuffAction(typeof(TEffect), level, duration, limit, count, occupationTime);
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x000168AC File Offset: 0x00014AAC
		protected BattleAction DebuffAction(Type type, Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			int? num = new int?(level);
			int? num2 = new int?(duration);
			int? num3 = new int?(limit);
			return new ApplyStatusEffectAction(type, target, num, num2, new int?(count), num3, occupationTime, startAutoDecreasing);
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x000168E4 File Offset: 0x00014AE4
		protected BattleAction DebuffAction<TEffect>(Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), target, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x0001690C File Offset: 0x00014B0C
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

		// Token: 0x060007A4 RID: 1956 RVA: 0x00016960 File Offset: 0x00014B60
		protected IEnumerable<BattleAction> DebuffAction<TEffect>(IEnumerable<Unit> targets, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), targets, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x00016988 File Offset: 0x00014B88
		protected BattleAction UpgradeAllHandsAction()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.HandZone, (Card card) => card.CanUpgradeAndPositive));
			if (list.Count <= 0)
			{
				return null;
			}
			return new UpgradeCardsAction(list);
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x000169DC File Offset: 0x00014BDC
		protected StatusEffect()
		{
			Type type = base.GetType();
			if (!StatusEffect.OpposeHandlerDict.ContainsKey(type))
			{
				StatusEffect.OpposeHandlerDict.Add(type, new StatusEffect.OpposeHandler(type));
			}
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x00016A32 File Offset: 0x00014C32
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<StatusEffect>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x00016A42 File Offset: 0x00014C42
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<StatusEffect>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x060007A9 RID: 1961 RVA: 0x00016A54 File Offset: 0x00014C54
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

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x060007AA RID: 1962 RVA: 0x00016AA0 File Offset: 0x00014CA0
		public string Brief
		{
			get
			{
				return this.LocalizeProperty("Brief", true, false);
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x060007AB RID: 1963 RVA: 0x00016AAF File Offset: 0x00014CAF
		string IDisplayWord.Description
		{
			get
			{
				string brief = this.Brief;
				return ((brief != null) ? brief.RuntimeFormat(base.FormatWrapper) : null) ?? ("<" + base.GetType().Name + ".Brief>");
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x060007AC RID: 1964 RVA: 0x00016AE7 File Offset: 0x00014CE7
		bool IDisplayWord.IsVerbose
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x060007AD RID: 1965 RVA: 0x00016AEA File Offset: 0x00014CEA
		public virtual string IconName
		{
			get
			{
				return base.Id;
			}
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x060007AE RID: 1966 RVA: 0x00016AF2 File Offset: 0x00014CF2
		public virtual string OverrideIconName
		{
			get
			{
				return this.Config.ImageId;
			}
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x060007AF RID: 1967 RVA: 0x00016AFF File Offset: 0x00014CFF
		// (set) Token: 0x060007B0 RID: 1968 RVA: 0x00016B07 File Offset: 0x00014D07
		public StatusEffectConfig Config { get; private set; }

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x060007B1 RID: 1969 RVA: 0x00016B10 File Offset: 0x00014D10
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x060007B2 RID: 1970 RVA: 0x00016B1D File Offset: 0x00014D1D
		public StatusEffectType Type
		{
			get
			{
				return this.Config.Type;
			}
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x060007B3 RID: 1971 RVA: 0x00016B2A File Offset: 0x00014D2A
		// (set) Token: 0x060007B4 RID: 1972 RVA: 0x00016B32 File Offset: 0x00014D32
		public virtual bool ForceNotShowDownText { get; set; }

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x060007B5 RID: 1973 RVA: 0x00016B3B File Offset: 0x00014D3B
		public bool HasLevel
		{
			get
			{
				return this.Config.HasLevel;
			}
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x060007B6 RID: 1974 RVA: 0x00016B48 File Offset: 0x00014D48
		// (set) Token: 0x060007B7 RID: 1975 RVA: 0x00016B74 File Offset: 0x00014D74
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

		// Token: 0x060007B8 RID: 1976 RVA: 0x00016BE7 File Offset: 0x00014DE7
		internal void SetInitLevel(int level)
		{
			this._level = level;
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x060007B9 RID: 1977 RVA: 0x00016BF0 File Offset: 0x00014DF0
		// (set) Token: 0x060007BA RID: 1978 RVA: 0x00016BF8 File Offset: 0x00014DF8
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

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x060007BB RID: 1979 RVA: 0x00016C07 File Offset: 0x00014E07
		public bool HasCount
		{
			get
			{
				return this.Config.HasCount;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x060007BC RID: 1980 RVA: 0x00016C14 File Offset: 0x00014E14
		// (set) Token: 0x060007BD RID: 1981 RVA: 0x00016C40 File Offset: 0x00014E40
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

		// Token: 0x060007BE RID: 1982 RVA: 0x00016CB3 File Offset: 0x00014EB3
		internal void SetInitCount(int count)
		{
			this._count = count;
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x060007BF RID: 1983 RVA: 0x00016CBC File Offset: 0x00014EBC
		// (set) Token: 0x060007C0 RID: 1984 RVA: 0x00016CC4 File Offset: 0x00014EC4
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

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x060007C1 RID: 1985 RVA: 0x00016CD3 File Offset: 0x00014ED3
		public bool HasDuration
		{
			get
			{
				return this.Config.HasDuration;
			}
		}

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x060007C2 RID: 1986 RVA: 0x00016CE0 File Offset: 0x00014EE0
		// (set) Token: 0x060007C3 RID: 1987 RVA: 0x00016D0C File Offset: 0x00014F0C
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

		// Token: 0x060007C4 RID: 1988 RVA: 0x00016D7F File Offset: 0x00014F7F
		internal void SetInitDuration(int duration)
		{
			this._duration = duration;
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x060007C5 RID: 1989 RVA: 0x00016D88 File Offset: 0x00014F88
		public bool IsStackable
		{
			get
			{
				return this.Config.IsStackable;
			}
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x060007C6 RID: 1990 RVA: 0x00016D98 File Offset: 0x00014F98
		[UsedImplicitly]
		public int TriggerLevel
		{
			get
			{
				return this.Config.StackActionTriggerLevel.GetValueOrDefault();
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x060007C7 RID: 1991 RVA: 0x00016DB8 File Offset: 0x00014FB8
		public bool ShowPlusByLimit
		{
			get
			{
				return this.Config.ShowPlusByLimit;
			}
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00016DC8 File Offset: 0x00014FC8
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

		// Token: 0x060007C9 RID: 1993 RVA: 0x000171A8 File Offset: 0x000153A8
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

		// Token: 0x060007CA RID: 1994 RVA: 0x00017210 File Offset: 0x00015410
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

		// Token: 0x060007CB RID: 1995 RVA: 0x000173DB File Offset: 0x000155DB
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new StatusEffect.StatusEffectFormatWrapper(this);
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x060007CC RID: 1996 RVA: 0x000173E4 File Offset: 0x000155E4
		// (set) Token: 0x060007CD RID: 1997 RVA: 0x00017403 File Offset: 0x00015603
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

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x060007CE RID: 1998 RVA: 0x00017411 File Offset: 0x00015611
		// (set) Token: 0x060007CF RID: 1999 RVA: 0x00017419 File Offset: 0x00015619
		internal bool IsAutoDecreasing { get; set; }

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x060007D0 RID: 2000 RVA: 0x00017422 File Offset: 0x00015622
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

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x060007D1 RID: 2001 RVA: 0x00017435 File Offset: 0x00015635
		// (set) Token: 0x060007D2 RID: 2002 RVA: 0x0001743D File Offset: 0x0001563D
		public Card SourceCard { get; set; }

		// Token: 0x060007D3 RID: 2003 RVA: 0x00017446 File Offset: 0x00015646
		internal void TriggerAdding(Unit unit)
		{
			this.OnAdding(unit);
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x0001744F File Offset: 0x0001564F
		internal void TriggerAdded(Unit unit)
		{
			this.OnAdded(unit);
		}

		// Token: 0x060007D5 RID: 2005 RVA: 0x00017458 File Offset: 0x00015658
		internal void TriggerRemoving(Unit unit)
		{
			this.OnRemoving(unit);
		}

		// Token: 0x060007D6 RID: 2006 RVA: 0x00017461 File Offset: 0x00015661
		internal void TriggerRemoved(Unit unit)
		{
			this.OnRemoved(unit);
			this._ownerHandlerHolder.ClearEventHandlers();
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00017475 File Offset: 0x00015675
		protected virtual void OnAdding(Unit unit)
		{
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x00017477 File Offset: 0x00015677
		protected virtual void OnAdded(Unit unit)
		{
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x00017479 File Offset: 0x00015679
		protected virtual void OnRemoving(Unit unit)
		{
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x0001747B File Offset: 0x0001567B
		protected virtual void OnRemoved(Unit unit)
		{
		}

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060007DB RID: 2011 RVA: 0x00017480 File Offset: 0x00015680
		// (remove) Token: 0x060007DC RID: 2012 RVA: 0x000174B8 File Offset: 0x000156B8
		public event Action Activating;

		// Token: 0x060007DD RID: 2013 RVA: 0x000174ED File Offset: 0x000156ED
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x060007DE RID: 2014 RVA: 0x000174FF File Offset: 0x000156FF
		// (set) Token: 0x060007DF RID: 2015 RVA: 0x00017507 File Offset: 0x00015707
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

		// Token: 0x060007E0 RID: 2016 RVA: 0x00017520 File Offset: 0x00015720
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

		// Token: 0x060007E1 RID: 2017 RVA: 0x0001756E File Offset: 0x0001576E
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[StatusEffect: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.StatusEffect);
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x000175A1 File Offset: 0x000157A1
		protected void HandleOwnerEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._ownerHandlerHolder.HandleEvent<T>(@event, handler, priority);
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x000175B1 File Offset: 0x000157B1
		protected void HandleOwnerEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleOwnerEvent<T>(@event, handler, this.DefaultEventPriority);
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x000175C4 File Offset: 0x000157C4
		protected void ReactOwnerEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleOwnerEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x000175F9 File Offset: 0x000157F9
		protected void ReactOwnerEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.ReactOwnerEvent<TEventArgs>(@event, reactor, this.DefaultEventPriority);
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x00017609 File Offset: 0x00015809
		public virtual IEnumerable<BattleAction> StackAction(Unit targetOwner, int targetLevel)
		{
			throw new InvalidOperationException(this.DebugName + " has no stack action");
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x00017620 File Offset: 0x00015820
		internal OpposeResult? Oppose(StatusEffect effect)
		{
			return StatusEffect.OpposeHandlerDict[base.GetType()].Oppose(this, effect);
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x0001763C File Offset: 0x0001583C
		bool IEquatable<IDisplayWord>.Equals(IDisplayWord other)
		{
			StatusEffect statusEffect = other as StatusEffect;
			return statusEffect != null && base.GetType() == statusEffect.GetType();
		}

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x060007E9 RID: 2025 RVA: 0x00017666 File Offset: 0x00015866
		protected virtual Keyword Keywords
		{
			get
			{
				return this.Config.Keywords;
			}
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x060007EA RID: 2026 RVA: 0x00017673 File Offset: 0x00015873
		protected virtual IEnumerable<string> RelativeEffects
		{
			get
			{
				return this.Config.RelativeEffects;
			}
		}

		// Token: 0x060007EB RID: 2027 RVA: 0x00017680 File Offset: 0x00015880
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Keywords, this.RelativeEffects, verbose, default(Keyword?));
		}

		// Token: 0x060007EC RID: 2028 RVA: 0x000176AE File Offset: 0x000158AE
		public virtual bool ShouldPreventCardUsage(Card card)
		{
			return false;
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x060007ED RID: 2029 RVA: 0x000176B1 File Offset: 0x000158B1
		public virtual string PreventCardUsageMessage
		{
			get
			{
				throw new InvalidOperationException("Cannot get prevent card message key for " + this.DebugName);
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x060007EE RID: 2030 RVA: 0x000176C8 File Offset: 0x000158C8
		public virtual string UnitEffectName
		{
			get
			{
				return null;
			}
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x000176CB File Offset: 0x000158CB
		public int GetSeLevel(Type seType)
		{
			if (this.Battle == null || !this.Battle.Player.HasStatusEffect(seType))
			{
				return 0;
			}
			return this.Battle.Player.GetStatusEffect(seType).Level;
		}

		// Token: 0x060007F0 RID: 2032 RVA: 0x00017700 File Offset: 0x00015900
		public int GetSeLevel<T>() where T : StatusEffect
		{
			return this.GetSeLevel(typeof(T));
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x060007F1 RID: 2033 RVA: 0x00017712 File Offset: 0x00015912
		[UsedImplicitly]
		public string SourceCardName
		{
			get
			{
				return UiUtils.WrapByColor(this.SourceCard.Name, GlobalConfig.EntityColor);
			}
		}

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x060007F2 RID: 2034 RVA: 0x00017729 File Offset: 0x00015929
		protected string ExtraDescription
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription", true, true);
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x060007F3 RID: 2035 RVA: 0x00017738 File Offset: 0x00015938
		protected string ExtraDescription2
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription2", true, true);
			}
		}

		// Token: 0x04000359 RID: 857
		private static readonly Dictionary<Type, StatusEffect.OpposeHandler> OpposeHandlerDict = new Dictionary<Type, StatusEffect.OpposeHandler>();

		// Token: 0x0400035C RID: 860
		private int _level;

		// Token: 0x0400035D RID: 861
		private bool _showCount = true;

		// Token: 0x0400035E RID: 862
		private int _count;

		// Token: 0x0400035F RID: 863
		private int _limit;

		// Token: 0x04000360 RID: 864
		private int _duration;

		// Token: 0x04000361 RID: 865
		private readonly WeakReference<Unit> _owner = new WeakReference<Unit>(null);

		// Token: 0x04000365 RID: 869
		private bool _highlight;

		// Token: 0x04000366 RID: 870
		private readonly GameEventHandlerHolder _ownerHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x0200024F RID: 591
		private class OpposeHandler
		{
			// Token: 0x06001266 RID: 4710 RVA: 0x00031CD4 File Offset: 0x0002FED4
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

			// Token: 0x06001267 RID: 4711 RVA: 0x00031D6C File Offset: 0x0002FF6C
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

			// Token: 0x0400091E RID: 2334
			[TupleElementNames(new string[] { "otherType", "handler" })]
			private readonly List<ValueTuple<Type, Func<StatusEffect, StatusEffect, OpposeResult>>> _opposingTypes = new List<ValueTuple<Type, Func<StatusEffect, StatusEffect, OpposeResult>>>();
		}

		// Token: 0x02000250 RID: 592
		internal sealed class StatusEffectFormatWrapper : GameEntityFormatWrapper
		{
			// Token: 0x06001268 RID: 4712 RVA: 0x00031DF4 File Offset: 0x0002FFF4
			public StatusEffectFormatWrapper(StatusEffect effect)
				: base(effect)
			{
				this._effect = effect;
			}

			// Token: 0x06001269 RID: 4713 RVA: 0x00031E04 File Offset: 0x00030004
			protected override object GetArgument(string key)
			{
				if (!(key == "OwnerName"))
				{
					return base.GetArgument(key);
				}
				Unit owner = this._effect.Owner;
				return ((owner != null) ? owner.GetName() : null) ?? UnitNameTable.GetDefaultOwnerName();
			}

			// Token: 0x0600126A RID: 4714 RVA: 0x00031E3C File Offset: 0x0003003C
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

			// Token: 0x0400091F RID: 2335
			private readonly StatusEffect _effect;
		}
	}
}
