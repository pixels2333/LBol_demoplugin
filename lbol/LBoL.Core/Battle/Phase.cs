using System;
using System.Collections;
using JetBrains.Annotations;

namespace LBoL.Core.Battle
{
	// Token: 0x02000147 RID: 327
	public class Phase
	{
		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x06000D23 RID: 3363 RVA: 0x00025238 File Offset: 0x00023438
		// (set) Token: 0x06000D24 RID: 3364 RVA: 0x00025257 File Offset: 0x00023457
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

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x06000D25 RID: 3365 RVA: 0x00025265 File Offset: 0x00023465
		public string Name { get; }

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x06000D26 RID: 3366 RVA: 0x0002526D File Offset: 0x0002346D
		public string DebugDetails
		{
			get
			{
				return this.Action.ExportDebugDetails();
			}
		}

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x06000D27 RID: 3367 RVA: 0x0002527A File Offset: 0x0002347A
		public bool IsModified
		{
			get
			{
				return this.Action.IsModified;
			}
		}

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x06000D28 RID: 3368 RVA: 0x00025287 File Offset: 0x00023487
		public string[] Modifiers
		{
			get
			{
				return this.Action.Modifiers;
			}
		}

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x06000D29 RID: 3369 RVA: 0x00025294 File Offset: 0x00023494
		public bool IsCanceled
		{
			get
			{
				return this.Action.IsCanceled;
			}
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x000252A1 File Offset: 0x000234A1
		public void ClearModifiers()
		{
			this.Action.ClearModifiers();
		}

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x06000D2B RID: 3371 RVA: 0x000252AE File Offset: 0x000234AE
		public CancelCause CancelCause
		{
			get
			{
				return this.Action.CancelCause;
			}
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x06000D2C RID: 3372 RVA: 0x000252BB File Offset: 0x000234BB
		[CanBeNull]
		public GameEntity CancelSource
		{
			get
			{
				return this.Action.CancelSource;
			}
		}

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x06000D2D RID: 3373 RVA: 0x000252C8 File Offset: 0x000234C8
		// (set) Token: 0x06000D2E RID: 3374 RVA: 0x000252D0 File Offset: 0x000234D0
		[CanBeNull]
		public Exception Exception { get; private set; }

		// Token: 0x06000D2F RID: 3375 RVA: 0x000252D9 File Offset: 0x000234D9
		internal Phase(BattleAction action, string name, Action handler, bool hasViewer = false)
		{
			this.Action = action;
			this.Name = name;
			this._handler = handler;
			this._hasViewer = hasViewer;
		}

		// Token: 0x06000D30 RID: 3376 RVA: 0x0002530A File Offset: 0x0002350A
		internal Phase(BattleAction action, string name, IEnumerator handler, bool hasViewer = false)
		{
			this.Action = action;
			this.Name = name;
			this._coroutineHandler = handler;
			this._hasViewer = hasViewer;
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x0002533B File Offset: 0x0002353B
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

		// Token: 0x04000624 RID: 1572
		private readonly WeakReference<BattleAction> _action = new WeakReference<BattleAction>(null);

		// Token: 0x04000627 RID: 1575
		[CanBeNull]
		private readonly Action _handler;

		// Token: 0x04000628 RID: 1576
		[CanBeNull]
		private readonly IEnumerator _coroutineHandler;

		// Token: 0x04000629 RID: 1577
		private readonly bool _hasViewer;
	}
}
