using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using UnityEngine;
namespace LBoL.Core.Units
{
	public abstract class Doll : GameEntity
	{
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Doll>.LocalizeProperty(base.Id, key, decorated, required);
		}
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return GameEventPriority.ConfigDefault;
			}
		}
		public DollConfig Config { get; protected set; }
		public override void Initialize()
		{
			base.Initialize();
			this.Config = DollConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find doll config for <" + base.Id + ">");
			}
			this.Magic = this.InitialMagic;
		}
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new Doll.DollFormatWrapper(this);
		}
		[UsedImplicitly]
		public virtual DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)this.ConfigDamage, false);
			}
		}
		public int ConfigDamage
		{
			get
			{
				int? damage = this.Config.Damage;
				if (damage != null)
				{
					return damage.GetValueOrDefault();
				}
				throw new InvalidDataException("<" + this.DebugName + "> has empty damage config");
			}
		}
		public int Value1
		{
			get
			{
				int? value = this.Config.Value1;
				if (value != null)
				{
					int valueOrDefault = value.GetValueOrDefault();
					return this.CalculateDollValue(valueOrDefault);
				}
				throw new InvalidDataException(this.DebugName + " has no 'Value1' in config");
			}
		}
		public int Value2
		{
			get
			{
				int? value = this.Config.Value2;
				if (value != null)
				{
					int valueOrDefault = value.GetValueOrDefault();
					return this.CalculateDollValue(valueOrDefault);
				}
				throw new InvalidDataException(this.DebugName + " has no 'Value2' in config");
			}
		}
		public int Magic
		{
			get
			{
				return this._magic;
			}
			set
			{
				this._magic = value;
				this.NotifyChanged();
			}
		}
		public bool Usable
		{
			get
			{
				return this.Config.Usable;
			}
		}
		public bool HasMagic
		{
			get
			{
				return this.Config.HasMagic;
			}
		}
		public int InitialMagic
		{
			get
			{
				return this.Config.InitialMagic;
			}
		}
		public int MagicCost
		{
			get
			{
				return this.Config.MagicCost;
			}
		}
		public int MaxMagic
		{
			get
			{
				return this.Config.MaxMagic;
			}
		}
		public ManaGroup Mana
		{
			get
			{
				ManaGroup? mana = this.Config.Mana;
				if (mana == null)
				{
					throw new InvalidDataException("<" + base.Id + "> has empty Mana config");
				}
				return mana.GetValueOrDefault();
			}
		}
		public virtual int? UpCounter
		{
			get
			{
				return default(int?);
			}
		}
		public virtual Color UpCounterColor
		{
			get
			{
				return Color.white;
			}
		}
		public virtual int? DownCounter
		{
			get
			{
				return default(int?);
			}
		}
		public virtual Color DownCounterColor
		{
			get
			{
				return Color.white;
			}
		}
		protected int CalculateDamage(DamageInfo damage)
		{
			if (this.Owner == null)
			{
				return damage.Damage.RoundToInt(1);
			}
			return this.Owner.Battle.CalculateDamage(this, this.Owner, null, damage);
		}
		protected int CalculateBlock(BlockInfo block)
		{
			if (this.Owner == null)
			{
				return block.Block;
			}
			return this.Owner.Battle.CalculateBlockShield(this, (float)block.Block, 0f, block.Type).Item1;
		}
		protected int CalculateDollValue(int value)
		{
			PlayerUnit owner = this.Owner;
			int? num;
			if (owner == null)
			{
				num = default(int?);
			}
			else
			{
				BattleController battle = owner.Battle;
				num = ((battle != null) ? new int?(battle.CalculateDollValue(this, value)) : default(int?));
			}
			int? num2 = num;
			if (num2 == null)
			{
				return value;
			}
			return num2.GetValueOrDefault();
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
		public PlayerUnit Owner
		{
			get
			{
				PlayerUnit playerUnit;
				if (!this._owner.TryGetTarget(ref playerUnit))
				{
					return null;
				}
				return playerUnit;
			}
			internal set
			{
				this._owner.SetTarget(value);
			}
		}
		public BattleController Battle
		{
			get
			{
				PlayerUnit owner = this.Owner;
				if (owner == null)
				{
					return null;
				}
				return owner.Battle;
			}
		}
		internal void TriggerAdding(PlayerUnit owner)
		{
			this.OnAdding(owner);
		}
		internal void TriggerAdded(PlayerUnit owner)
		{
			this.OnAdded(owner);
		}
		internal void TriggerRemoving(PlayerUnit owner)
		{
			this.OnRemoving(owner);
		}
		internal void TriggerRemoved(PlayerUnit owner)
		{
			this.OnRemoved(owner);
			this._ownerHandlerHolder.ClearEventHandlers();
		}
		protected virtual void OnAdding(PlayerUnit owner)
		{
		}
		protected virtual void OnAdded(PlayerUnit owner)
		{
		}
		protected virtual void OnRemoving(PlayerUnit owner)
		{
		}
		protected virtual void OnRemoved(PlayerUnit owner)
		{
		}
		internal IEnumerable<BattleAction> GetPassiveActions(List<DamageAction> damageActions)
		{
			if (this.PassiveActions() == null)
			{
				yield break;
			}
			foreach (BattleAction battleAction in this.PassiveActions())
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					damageActions.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		protected virtual IEnumerable<BattleAction> PassiveActions()
		{
			if (this.HasMagic)
			{
				yield return new DollGainMagicAction(this, 1);
			}
			yield break;
		}
		internal IEnumerable<BattleAction> GetActiveActions(List<DamageAction> damageActions)
		{
			foreach (BattleAction battleAction in this.ActiveActions())
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					damageActions.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		protected virtual IEnumerable<BattleAction> ActiveActions()
		{
			return null;
		}
		protected void NotifyPassiveActivating()
		{
			Action passiveActivating = this.PassiveActivating;
			if (passiveActivating == null)
			{
				return;
			}
			passiveActivating.Invoke();
		}
		protected void NotifyActiveActivating()
		{
			Action activeActivating = this.ActiveActivating;
			if (activeActivating == null)
			{
				return;
			}
			activeActivating.Invoke();
		}
		public event Action PassiveActivating;
		public event Action ActiveActivating;
		public bool ConsumeMagic(int magic)
		{
			this.Magic -= magic;
			if (this.Magic >= 0)
			{
				return true;
			}
			this.Magic = 0;
			return false;
		}
		public TargetType TargetType { get; protected set; }
		public string GunName { get; protected set; }
		public Unit PendingTarget
		{
			get
			{
				return this._pendingTarget;
			}
			set
			{
				if (value != this._pendingTarget)
				{
					this._pendingTarget = value;
					this.NotifyChanged();
				}
			}
		}
		internal IEnumerable<BattleAction> GetActions(UnitSelector selector, IList<DamageAction> damageActions)
		{
			if (selector.Type == TargetType.SingleEnemy)
			{
				this.PendingTarget = selector.SelectedEnemy;
			}
			foreach (BattleAction battleAction in this.Actions(selector))
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					damageActions.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			this.PendingTarget = null;
			yield break;
			yield break;
		}
		protected abstract IEnumerable<BattleAction> Actions(UnitSelector selector);
		public virtual IEnumerable<BattleAction> OnRemove()
		{
			return null;
		}
		private int _magic;
		private readonly GameEventHandlerHolder _ownerHandlerHolder = new GameEventHandlerHolder();
		private readonly WeakReference<PlayerUnit> _owner = new WeakReference<PlayerUnit>(null);
		private Unit _pendingTarget;
		private sealed class DollFormatWrapper : GameEntityFormatWrapper
		{
			public DollFormatWrapper(Doll doll)
				: base(doll)
			{
				this._doll = doll;
			}
			protected override object GetArgument(string key)
			{
				if (!(key == "OwnerName"))
				{
					return base.GetArgument(key);
				}
				PlayerUnit owner = this._doll.Owner;
				return ((owner != null) ? owner.GetName() : null) ?? UnitNameTable.GetDefaultOwnerName();
			}
			protected override string FormatArgument(object arg, string format)
			{
				if (!(arg is DamageInfo))
				{
					return base.FormatArgument(arg, format);
				}
				DamageInfo damageInfo = (DamageInfo)arg;
				int num = (int)damageInfo.Damage;
				BattleController battle = this._doll.Battle;
				if (battle == null)
				{
					return GameEntityFormatWrapper.WrappedFormatNumber(num, num, format);
				}
				int num2 = battle.CalculateDamage(this._doll, battle.Player, null, damageInfo);
				return GameEntityFormatWrapper.WrappedFormatNumber(num, num2, format);
			}
			private readonly Doll _doll;
		}
	}
}
