using System;
using UnityEngine;

namespace LBoL.Core.Battle
{
	// Token: 0x0200014D RID: 333
	public abstract class EventBattleAction<TArgs> : BattleAction where TArgs : GameEventArgs
	{
		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06000D63 RID: 3427 RVA: 0x0002553A File Offset: 0x0002373A
		// (set) Token: 0x06000D64 RID: 3428 RVA: 0x00025542 File Offset: 0x00023742
		public TArgs Args { get; internal set; }

		// Token: 0x06000D65 RID: 3429 RVA: 0x0002554B File Offset: 0x0002374B
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x00025568 File Offset: 0x00023768
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			if (this.Args == null)
			{
				Debug.LogWarning("Args == null in " + base.GetType().Name);
			}
			else
			{
				this.Args.ActionSource = source;
			}
			return this;
		}

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x06000D67 RID: 3431 RVA: 0x000255B8 File Offset: 0x000237B8
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06000D68 RID: 3432 RVA: 0x000255CA File Offset: 0x000237CA
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x06000D69 RID: 3433 RVA: 0x000255DC File Offset: 0x000237DC
		public override bool IsCanceled
		{
			get
			{
				return this.Args.IsCanceled;
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x06000D6A RID: 3434 RVA: 0x000255EE File Offset: 0x000237EE
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x06000D6B RID: 3435 RVA: 0x00025600 File Offset: 0x00023800
		public override GameEntity CancelSource
		{
			get
			{
				return this.Args.CancelSource;
			}
		}

		// Token: 0x06000D6C RID: 3436 RVA: 0x00025612 File Offset: 0x00023812
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x00025624 File Offset: 0x00023824
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
