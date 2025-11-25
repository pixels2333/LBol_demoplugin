using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Core
{
	[Localizable]
	public abstract class JadeBox : GameEntity
	{
		public JadeBoxConfig Config { get; private set; }
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<JadeBox>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<JadeBox>.LocalizeListProperty(base.Id, key, required);
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
		public virtual string IconName
		{
			get
			{
				return base.Id;
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
			this.Config = JadeBoxConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find JadeBox config for <" + base.Id + ">");
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
		internal void TriggerGain(GameRunController gameRun)
		{
			this.OnGain(gameRun);
		}
		internal void TriggerAdded()
		{
			this.OnAdded();
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
		protected virtual void OnGain(GameRunController gameRun)
		{
		}
		protected virtual void OnAdded()
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
				Debug.LogError("[JadeBox: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.JadeBox);
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
		private readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();
	}
}
