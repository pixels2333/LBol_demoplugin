using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base.Extensions;

namespace LBoL.Core.Battle
{
	// Token: 0x0200014C RID: 332
	public abstract class BattleAction
	{
		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x06000D4A RID: 3402 RVA: 0x000253D0 File Offset: 0x000235D0
		// (set) Token: 0x06000D4B RID: 3403 RVA: 0x000253D8 File Offset: 0x000235D8
		internal bool Resolved { get; set; }

		// Token: 0x06000D4C RID: 3404
		internal abstract IEnumerable<Phase> GetPhases();

		// Token: 0x06000D4D RID: 3405 RVA: 0x000253E1 File Offset: 0x000235E1
		protected BattleAction()
			: this(ActionCause.None)
		{
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x000253EA File Offset: 0x000235EA
		private BattleAction(ActionCause cause)
		{
			this.Cause = cause;
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x06000D4F RID: 3407 RVA: 0x00025414 File Offset: 0x00023614
		// (set) Token: 0x06000D50 RID: 3408 RVA: 0x00025433 File Offset: 0x00023633
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

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x06000D51 RID: 3409 RVA: 0x00025444 File Offset: 0x00023644
		// (set) Token: 0x06000D52 RID: 3410 RVA: 0x00025463 File Offset: 0x00023663
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

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06000D53 RID: 3411 RVA: 0x00025471 File Offset: 0x00023671
		// (set) Token: 0x06000D54 RID: 3412 RVA: 0x00025479 File Offset: 0x00023679
		public ActionCause Cause { get; private set; }

		// Token: 0x06000D55 RID: 3413 RVA: 0x00025482 File Offset: 0x00023682
		public virtual BattleAction SetCause(ActionCause cause)
		{
			this.Cause = cause;
			return this;
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x0002548C File Offset: 0x0002368C
		public virtual BattleAction SetSource(GameEntity source)
		{
			this.Source = source;
			return this;
		}

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x06000D57 RID: 3415
		public abstract bool IsModified { get; }

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x06000D58 RID: 3416
		public abstract string[] Modifiers { get; }

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x06000D59 RID: 3417
		public abstract bool IsCanceled { get; }

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x06000D5A RID: 3418
		public abstract CancelCause CancelCause { get; }

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x06000D5B RID: 3419 RVA: 0x00025496 File Offset: 0x00023696
		public virtual GameEntity CancelSource
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06000D5C RID: 3420
		public abstract void ClearModifiers();

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x06000D5D RID: 3421 RVA: 0x00025499 File Offset: 0x00023699
		public virtual string Name
		{
			get
			{
				return base.GetType().Name.TryRemoveEnd("Action");
			}
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x000254B0 File Offset: 0x000236B0
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

		// Token: 0x06000D5F RID: 3423 RVA: 0x000254E5 File Offset: 0x000236E5
		internal Phase CreatePhase(string name, Action handler, bool hasViewer = false)
		{
			return new Phase(this, name, handler, hasViewer);
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x000254F0 File Offset: 0x000236F0
		internal Phase CreatePhase(string name, IEnumerator handler, bool hasViewer = false)
		{
			return new Phase(this, name, handler, hasViewer);
		}

		// Token: 0x06000D61 RID: 3425 RVA: 0x000254FC File Offset: 0x000236FC
		protected void React(Reactor reactor, GameEntity source = null, ActionCause? cause = null)
		{
			this.Battle.React(reactor, source ?? this.Source, cause ?? this.Cause);
		}

		// Token: 0x06000D62 RID: 3426
		[return: MaybeNull]
		public abstract string ExportDebugDetails();

		// Token: 0x0400062E RID: 1582
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);

		// Token: 0x0400062F RID: 1583
		private readonly WeakReference<GameEntity> _source = new WeakReference<GameEntity>(null);
	}
}
