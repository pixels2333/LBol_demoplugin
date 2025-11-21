using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle
{
	// Token: 0x0200014F RID: 335
	public abstract class SimpleAction : BattleAction
	{
		// Token: 0x06000D76 RID: 3446 RVA: 0x00025683 File Offset: 0x00023883
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Resolve", new Action(this.ResolvePhase), true);
			yield break;
		}

		// Token: 0x06000D77 RID: 3447 RVA: 0x00025693 File Offset: 0x00023893
		protected virtual void ResolvePhase()
		{
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x06000D78 RID: 3448 RVA: 0x00025695 File Offset: 0x00023895
		public override bool IsModified
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x06000D79 RID: 3449 RVA: 0x00025698 File Offset: 0x00023898
		public override string[] Modifiers
		{
			get
			{
				return Array.Empty<string>();
			}
		}

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x06000D7A RID: 3450 RVA: 0x0002569F File Offset: 0x0002389F
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x06000D7B RID: 3451 RVA: 0x000256A2 File Offset: 0x000238A2
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x000256A5 File Offset: 0x000238A5
		public override void ClearModifiers()
		{
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x000256A7 File Offset: 0x000238A7
		public override string ExportDebugDetails()
		{
			return null;
		}
	}
}
