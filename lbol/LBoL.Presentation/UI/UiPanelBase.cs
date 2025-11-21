using System;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Presentation.InputSystemExtend;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000036 RID: 54
	public class UiPanelBase : UiBase, IInteractablePanel
	{
		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060003AE RID: 942 RVA: 0x0000F9C3 File Offset: 0x0000DBC3
		public virtual PanelLayer Layer
		{
			get
			{
				return PanelLayer.Normal;
			}
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0000F9C6 File Offset: 0x0000DBC6
		public void Hide()
		{
			this.Hide(true);
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0000F9D0 File Offset: 0x0000DBD0
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

		// Token: 0x060003B1 RID: 945 RVA: 0x0000FA1C File Offset: 0x0000DC1C
		protected void HandleGameRunEvent<T>(GameEvent<T> e, GameEventHandler<T> action) where T : GameEventArgs
		{
			this._handlerHolder.HandleEvent<T>(e, action, GameEventPriority.Lowest);
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x0000FA30 File Offset: 0x0000DC30
		protected void HandleBattleEvent<T>(GameEvent<T> e, GameEventHandler<T> action) where T : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<T>(e, action, GameEventPriority.Lowest);
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060003B3 RID: 947 RVA: 0x0000FA44 File Offset: 0x0000DC44
		// (set) Token: 0x060003B4 RID: 948 RVA: 0x0000FA63 File Offset: 0x0000DC63
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

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060003B5 RID: 949 RVA: 0x0000FA74 File Offset: 0x0000DC74
		// (set) Token: 0x060003B6 RID: 950 RVA: 0x0000FA93 File Offset: 0x0000DC93
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

		// Token: 0x060003B7 RID: 951 RVA: 0x0000FAA1 File Offset: 0x0000DCA1
		internal void EnterGameRun(GameRunController gameRun)
		{
			if (this.GameRun != null)
			{
				throw new InvalidOperationException("Cannot enter game-run while already in game-run");
			}
			this.GameRun = gameRun;
			this.OnEnterGameRun();
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0000FAC3 File Offset: 0x0000DCC3
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

		// Token: 0x060003B9 RID: 953 RVA: 0x0000FAF0 File Offset: 0x0000DCF0
		protected virtual void OnEnterGameRun()
		{
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0000FAF2 File Offset: 0x0000DCF2
		protected virtual void OnLeaveGameRun()
		{
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0000FAF4 File Offset: 0x0000DCF4
		internal void EnterBattle(BattleController battle)
		{
			this.Battle = battle;
			this.OnEnterBattle();
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0000FB03 File Offset: 0x0000DD03
		internal void LeaveBattle()
		{
			this.OnLeaveBattle();
			this._battleHandlerHolder.ClearEventHandlers();
			this.Battle = null;
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0000FB1D File Offset: 0x0000DD1D
		protected virtual void OnEnterBattle()
		{
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0000FB1F File Offset: 0x0000DD1F
		protected virtual void OnLeaveBattle()
		{
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0000FB24 File Offset: 0x0000DD24
		public string GetPanelName()
		{
			string name = base.gameObject.name;
			if (!name.Contains("(clone)", 5))
			{
				return name;
			}
			return name.Replace("(clone)", "", 5).Trim();
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0000FB63 File Offset: 0x0000DD63
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

		// Token: 0x060003C1 RID: 961 RVA: 0x0000FB8E File Offset: 0x0000DD8E
		protected sealed override void OnTransitionOutFinished()
		{
			base.OnTransitionOutFinished();
			Singleton<GamepadNavigationManager>.Instance.RemovePanel(this);
			Singleton<GamepadNavigationManager>.Instance.SetAvailable(true);
			GamepadNavigationManager.RefreshSelection();
		}

		// Token: 0x040001B4 RID: 436
		private readonly GameEventHandlerHolder _handlerHolder = new GameEventHandlerHolder();

		// Token: 0x040001B5 RID: 437
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x040001B6 RID: 438
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);

		// Token: 0x040001B7 RID: 439
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);
	}
}
