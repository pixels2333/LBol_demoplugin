using System;
using System.Collections;
using JetBrains.Annotations;
namespace LBoL.Core.Battle
{
	public class Phase
	{
		public BattleAction Action
		{
			get
			{
				BattleAction battleAction;
				if (!this._action.TryGetTarget(ref battleAction))
				{
					return null;
				}
				return battleAction;
			}
			private set
			{
				this._action.SetTarget(value);
			}
		}
		public string Name { get; }
		public string DebugDetails
		{
			get
			{
				return this.Action.ExportDebugDetails();
			}
		}
		public bool IsModified
		{
			get
			{
				return this.Action.IsModified;
			}
		}
		public string[] Modifiers
		{
			get
			{
				return this.Action.Modifiers;
			}
		}
		public bool IsCanceled
		{
			get
			{
				return this.Action.IsCanceled;
			}
		}
		public void ClearModifiers()
		{
			this.Action.ClearModifiers();
		}
		public CancelCause CancelCause
		{
			get
			{
				return this.Action.CancelCause;
			}
		}
		[CanBeNull]
		public GameEntity CancelSource
		{
			get
			{
				return this.Action.CancelSource;
			}
		}
		[CanBeNull]
		public Exception Exception { get; private set; }
		internal Phase(BattleAction action, string name, Action handler, bool hasViewer = false)
		{
			this.Action = action;
			this.Name = name;
			this._handler = handler;
			this._hasViewer = hasViewer;
		}
		internal Phase(BattleAction action, string name, IEnumerator handler, bool hasViewer = false)
		{
			this.Action = action;
			this.Name = name;
			this._coroutineHandler = handler;
			this._hasViewer = hasViewer;
		}
		public IEnumerator Flow()
		{
			if (this.IsCanceled)
			{
				yield break;
			}
			if (this._handler != null)
			{
				try
				{
					this._handler.Invoke();
					goto IL_009D;
				}
				catch (Exception ex)
				{
					this.Exception = ex;
					goto IL_009D;
				}
			}
			if (this._coroutineHandler != null)
			{
				for (;;)
				{
					try
					{
						if (!this._coroutineHandler.MoveNext())
						{
							break;
						}
					}
					catch (Exception ex2)
					{
						this.Exception = ex2;
						yield break;
					}
					yield return this._coroutineHandler.Current;
				}
			}
			IL_009D:
			if (!this.IsCanceled && this._hasViewer && this.Exception == null)
			{
				yield return this.Action.Battle.ActionViewer.View(this.Action);
			}
			yield break;
		}
		private readonly WeakReference<BattleAction> _action = new WeakReference<BattleAction>(null);
		[CanBeNull]
		private readonly Action _handler;
		[CanBeNull]
		private readonly IEnumerator _coroutineHandler;
		private readonly bool _hasViewer;
	}
}
