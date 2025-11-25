using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base.Extensions;
namespace LBoL.Core.Battle
{
	public abstract class BattleAction
	{
		internal bool Resolved { get; set; }
		internal abstract IEnumerable<Phase> GetPhases();
		protected BattleAction()
			: this(ActionCause.None)
		{
		}
		private BattleAction(ActionCause cause)
		{
			this.Cause = cause;
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
			internal set
			{
				this._battle.SetTarget(value);
			}
		}
		public GameEntity Source
		{
			get
			{
				GameEntity gameEntity;
				if (!this._source.TryGetTarget(ref gameEntity))
				{
					return null;
				}
				return gameEntity;
			}
			private set
			{
				this._source.SetTarget(value);
			}
		}
		public ActionCause Cause { get; private set; }
		public virtual BattleAction SetCause(ActionCause cause)
		{
			this.Cause = cause;
			return this;
		}
		public virtual BattleAction SetSource(GameEntity source)
		{
			this.Source = source;
			return this;
		}
		public abstract bool IsModified { get; }
		public abstract string[] Modifiers { get; }
		public abstract bool IsCanceled { get; }
		public abstract CancelCause CancelCause { get; }
		public virtual GameEntity CancelSource
		{
			get
			{
				return null;
			}
		}
		public abstract void ClearModifiers();
		public virtual string Name
		{
			get
			{
				return base.GetType().Name.TryRemoveEnd("Action");
			}
		}
		internal Phase CreateEventPhase<TEventArgs>(string name, TEventArgs args, [MaybeNull] GameEvent<TEventArgs> e) where TEventArgs : GameEventArgs
		{
			return this.CreatePhase(name, delegate
			{
				GameEvent<TEventArgs> e2 = e;
				if (e2 == null)
				{
					return;
				}
				e2.Execute(args);
			}, false);
		}
		internal Phase CreatePhase(string name, Action handler, bool hasViewer = false)
		{
			return new Phase(this, name, handler, hasViewer);
		}
		internal Phase CreatePhase(string name, IEnumerator handler, bool hasViewer = false)
		{
			return new Phase(this, name, handler, hasViewer);
		}
		protected void React(Reactor reactor, GameEntity source = null, ActionCause? cause = null)
		{
			this.Battle.React(reactor, source ?? this.Source, cause ?? this.Cause);
		}
		[return: MaybeNull]
		public abstract string ExportDebugDetails();
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);
		private readonly WeakReference<GameEntity> _source = new WeakReference<GameEntity>(null);
	}
}
