using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200018A RID: 394
	public sealed class InteractionAction : BattleAction
	{
		// Token: 0x06000EAF RID: 3759 RVA: 0x00027DFC File Offset: 0x00025FFC
		public InteractionAction(Interaction interaction, bool canCancel = false)
		{
			this._interaction = interaction;
			this._interaction.CanCancel = canCancel;
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x00027E17 File Offset: 0x00026017
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Resolve", base.Battle.GameRun.InteractionViewer.View(this._interaction), false);
			yield break;
		}

		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x06000EB1 RID: 3761 RVA: 0x00027E27 File Offset: 0x00026027
		public override bool IsModified
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x06000EB2 RID: 3762 RVA: 0x00027E2A File Offset: 0x0002602A
		public override string[] Modifiers
		{
			get
			{
				return Array.Empty<string>();
			}
		}

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x06000EB3 RID: 3763 RVA: 0x00027E31 File Offset: 0x00026031
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x06000EB4 RID: 3764 RVA: 0x00027E34 File Offset: 0x00026034
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}

		// Token: 0x06000EB5 RID: 3765 RVA: 0x00027E37 File Offset: 0x00026037
		public override void ClearModifiers()
		{
		}

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x06000EB6 RID: 3766 RVA: 0x00027E39 File Offset: 0x00026039
		public override string Name
		{
			get
			{
				return base.Name + " (" + this._interaction.GetType().Name + ")";
			}
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x00027E60 File Offset: 0x00026060
		public override string ExportDebugDetails()
		{
			return string.Empty;
		}

		// Token: 0x04000689 RID: 1673
		private readonly Interaction _interaction;
	}
}
