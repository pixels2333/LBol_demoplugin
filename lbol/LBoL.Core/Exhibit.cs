using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core
{
	[Localizable]
	public abstract class Exhibit : GameEntity, INotifyActivating
	{
		public ExhibitConfig Config { get; private set; }
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Exhibit>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<Exhibit>.LocalizeListProperty(base.Id, key, required);
		}
		protected override string GetBaseDescription()
		{
			return this.LocalizeProperty("Description", true, false);
		}
		public EntityName GetName()
		{
			return new EntityName("Exhibit." + base.Id, this.Name);
		}
		public override string Description
		{
			get
			{
				string text;
				try
				{
					string baseDescription = this.GetBaseDescription();
					text = ((baseDescription == null) ? "" : baseDescription.RuntimeFormat(base.FormatWrapper));
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					text = "<Error>";
				}
				return text;
			}
		}
		public string FlavorText
		{
			get
			{
				return this.LocalizeProperty("FlavorText", false, false);
			}
		}
		protected string ExtraDescription
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription", true, true);
			}
		}
		public string OwnerId { get; private set; }
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
				return null;
			}
		}
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			this.Config = ExhibitConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find exhibit config for <" + base.Id + ">");
			}
			this.OwnerId = (this.Config.Owner.IsNullOrWhiteSpace() ? null : this.Config.Owner);
			if (this.Config.HasCounter)
			{
				int? initialCounter = this.Config.InitialCounter;
				if (initialCounter != null)
				{
					int valueOrDefault = initialCounter.GetValueOrDefault();
					this.Counter = valueOrDefault;
				}
			}
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
		public int Value1
		{
			get
			{
				int? value = this.Config.Value1;
				if (value == null)
				{
					throw new InvalidDataException(this.DebugName + " has no 'Value1' in config");
				}
				return value.GetValueOrDefault();
			}
		}
		public int Value2
		{
			get
			{
				int? value = this.Config.Value2;
				if (value == null)
				{
					throw new InvalidDataException(this.DebugName + " has no 'Value2' in config");
				}
				return value.GetValueOrDefault();
			}
		}
		public int Value3
		{
			get
			{
				int? value = this.Config.Value3;
				if (value == null)
				{
					throw new InvalidDataException(this.DebugName + " has no 'Value3' in config");
				}
				return value.GetValueOrDefault();
			}
		}
		[UsedImplicitly]
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
		[UsedImplicitly]
		public BaseManaGroup BaseMana
		{
			get
			{
				ManaColor? baseManaColor = this.Config.BaseManaColor;
				if (baseManaColor != null)
				{
					ManaColor valueOrDefault = baseManaColor.GetValueOrDefault();
					return new BaseManaGroup(ManaGroup.Single(valueOrDefault) * this.Config.BaseManaAmount);
				}
				return new BaseManaGroup(ManaGroup.Empty);
			}
		}
		public bool HasCounter
		{
			get
			{
				return this.Config.HasCounter;
			}
		}
		[UsedImplicitly]
		public int InitialCounter
		{
			get
			{
				return this.Config.InitialCounter.GetValueOrDefault();
			}
		}
		public virtual bool ShowCounter
		{
			get
			{
				return true;
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active != value)
				{
					this._active = value;
					this.NotifyChanged();
				}
			}
		}
		public bool Blackout
		{
			get
			{
				return this._blackout;
			}
			set
			{
				if (this._blackout != value)
				{
					this._blackout = value;
					this.NotifyChanged();
				}
			}
		}
		public int Counter
		{
			get
			{
				return this._counter;
			}
			set
			{
				int num = Math.Min(value, 999);
				if (this._counter != num)
				{
					this._counter = num;
					this.NotifyChanged();
				}
			}
		}
		public ExhibitLosableType LosableType
		{
			get
			{
				return this.Config.LosableType;
			}
		}
		public bool IsSentinel
		{
			get
			{
				return this.Config.IsSentinel;
			}
		}
		public int? CardInstanceId { get; protected internal set; }
		public BattleController Battle
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null)
				{
					return null;
				}
				return gameRun.Battle;
			}
		}
		internal void TriggerAdding(PlayerUnit unit)
		{
			this.OnAdding(unit);
		}
		internal void TriggerAdded(PlayerUnit unit)
		{
			this.OnAdded(unit);
		}
		internal void TriggerRemoving(PlayerUnit unit)
		{
			this.OnRemoving(unit);
			this._gameRunHandlerHolder.ClearEventHandlers();
		}
		internal void TriggerRemoved(PlayerUnit unit)
		{
			this.OnRemoved(unit);
		}
		internal IEnumerator TriggerGain(PlayerUnit unit)
		{
			IEnumerator enumerator = this.SpecialGain(unit);
			if (enumerator != null)
			{
				yield return enumerator;
			}
			else
			{
				this.OnGain(unit);
			}
			yield break;
		}
		internal void TriggerGainInstantly(PlayerUnit unit)
		{
			if (this.SpecialGain(unit) != null)
			{
				Debug.LogError("Cannot gain " + this.DebugName + " instantly (init-exhibit or debug-add) because it has special gain interaction, triggering OnGain() instead.");
			}
			this.OnGain(unit);
		}
		internal void TriggerLose(PlayerUnit unit)
		{
			if (this.Battle != null)
			{
				this.OnLeaveBattle();
				this._battleHandlerHolder.ClearEventHandlers();
			}
			this.OnLose(unit);
		}
		internal void EnterBattle()
		{
			this.OnEnterBattle();
		}
		internal void LeaveBattle()
		{
			this.OnLeaveBattle();
			this._battleHandlerHolder.ClearEventHandlers();
		}
		protected virtual void OnAdding(PlayerUnit player)
		{
		}
		protected virtual void OnAdded(PlayerUnit player)
		{
		}
		protected virtual void OnRemoving(PlayerUnit player)
		{
		}
		protected virtual void OnRemoved(PlayerUnit player)
		{
		}
		protected virtual IEnumerator SpecialGain(PlayerUnit unit)
		{
			return null;
		}
		protected virtual void OnGain(PlayerUnit player)
		{
		}
		protected virtual void OnLose(PlayerUnit player)
		{
		}
		protected virtual void OnEnterBattle()
		{
		}
		protected virtual void OnLeaveBattle()
		{
		}
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}
		public event Action Activating;
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[Exhibit: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.Exhibit);
		}
		public virtual IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}
		public virtual IEnumerable<Card> EnumerateRelativeCards()
		{
			IReadOnlyList<string> readOnlyList = this.Config.RelativeCards ?? Array.Empty<string>();
			foreach (string text in readOnlyList)
			{
				Card card;
				if (Enumerable.Last<char>(text) == '+')
				{
					string text2 = text;
					card = Library.CreateCard(text2.Substring(0, text2.Length - 1));
					card.Upgrade();
				}
				else
				{
					card = Library.CreateCard(text);
				}
				card.GameRun = base.GameRun;
				yield return card;
			}
			IEnumerator<string> enumerator = null;
			yield break;
			yield break;
		}
		protected void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this._gameRunHandlerHolder.HandleEvent<TEventArgs>(@event, handler, priority);
		}
		protected void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
		{
			this.HandleGameRunEvent<TEventArgs>(@event, handler, this.DefaultEventPriority);
		}
		protected void HandleBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<TEventArgs>(@event, handler, priority);
		}
		protected void HandleBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, handler, this.DefaultEventPriority);
		}
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.ReactBattleEvent<TEventArgs>(@event, reactor, this.DefaultEventPriority);
		}
		private readonly WeakReference<PlayerUnit> _owner = new WeakReference<PlayerUnit>(null);
		private bool _active;
		private bool _blackout;
		private int _counter;
		private readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();
	}
}
