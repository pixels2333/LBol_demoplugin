using System;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Presentation.InputSystemExtend;
namespace LBoL.Presentation.UI
{
	public class UiPanelBase : UiBase, IInteractablePanel
	{
		public virtual PanelLayer Layer
		{
			get
			{
				return PanelLayer.Normal;
			}
		}
		public void Hide()
		{
			this.Hide(true);
		}
		public void Hide(bool transition)
		{
			if (this && base.IsVisible)
			{
				Singleton<GamepadNavigationManager>.Instance.SetAvailable(false);
				base.KillTransitionIn();
				this.OnHiding();
				if (transition)
				{
					base.DoTransitionOut();
				}
				else
				{
					this.OnTransitionOutFinished();
				}
				base.IsVisible = false;
			}
		}
		protected void HandleGameRunEvent<T>(GameEvent<T> e, GameEventHandler<T> action) where T : GameEventArgs
		{
			this._handlerHolder.HandleEvent<T>(e, action, GameEventPriority.Lowest);
		}
		protected void HandleBattleEvent<T>(GameEvent<T> e, GameEventHandler<T> action) where T : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<T>(e, action, GameEventPriority.Lowest);
		}
		private protected GameRunController GameRun
		{
			protected get
			{
				GameRunController gameRunController;
				if (!this._gameRun.TryGetTarget(ref gameRunController))
				{
					return null;
				}
				return gameRunController;
			}
			private set
			{
				this._gameRun.SetTarget(value);
			}
		}
		private protected BattleController Battle
		{
			protected get
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
		internal void EnterGameRun(GameRunController gameRun)
		{
			if (this.GameRun != null)
			{
				throw new InvalidOperationException("Cannot enter game-run while already in game-run");
			}
			this.GameRun = gameRun;
			this.OnEnterGameRun();
		}
		internal void LeaveGameRun()
		{
			if (this.GameRun == null)
			{
				throw new InvalidOperationException("Cannot leave game-run while not in game-run");
			}
			this.OnLeaveGameRun();
			this._handlerHolder.ClearEventHandlers();
			this.GameRun = null;
		}
		protected virtual void OnEnterGameRun()
		{
		}
		protected virtual void OnLeaveGameRun()
		{
		}
		internal void EnterBattle(BattleController battle)
		{
			this.Battle = battle;
			this.OnEnterBattle();
		}
		internal void LeaveBattle()
		{
			this.OnLeaveBattle();
			this._battleHandlerHolder.ClearEventHandlers();
			this.Battle = null;
		}
		protected virtual void OnEnterBattle()
		{
		}
		protected virtual void OnLeaveBattle()
		{
		}
		public string GetPanelName()
		{
			string name = base.gameObject.name;
			if (!name.Contains("(clone)", 5))
			{
				return name;
			}
			return name.Replace("(clone)", "", 5).Trim();
		}
		protected sealed override void OnTransitionInFinished()
		{
			base.OnTransitionInFinished();
			if (!this.enableGamepadInputInTransitionIn)
			{
				Singleton<GamepadNavigationManager>.Instance.AddPanel(this);
				Singleton<GamepadNavigationManager>.Instance.SetAvailable(true);
			}
			GamepadNavigationManager.RefreshSelection();
		}
		protected sealed override void OnTransitionOutFinished()
		{
			base.OnTransitionOutFinished();
			Singleton<GamepadNavigationManager>.Instance.RemovePanel(this);
			Singleton<GamepadNavigationManager>.Instance.SetAvailable(true);
			GamepadNavigationManager.RefreshSelection();
		}
		private readonly GameEventHandlerHolder _handlerHolder = new GameEventHandlerHolder();
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);
	}
}
