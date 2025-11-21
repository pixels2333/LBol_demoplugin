using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base.Extensions;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActionRecord
{
	// Token: 0x020001B8 RID: 440
	public class PhaseRecord
	{
		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x06000FA1 RID: 4001 RVA: 0x0002A02F File Offset: 0x0002822F
		public List<ActionRecord> Reactions { get; } = new List<ActionRecord>();

		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x06000FA2 RID: 4002 RVA: 0x0002A037 File Offset: 0x00028237
		public string Name { get; }

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x06000FA3 RID: 4003 RVA: 0x0002A03F File Offset: 0x0002823F
		public string Details { get; }

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x06000FA4 RID: 4004 RVA: 0x0002A047 File Offset: 0x00028247
		public bool IsModified { get; }

		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x06000FA5 RID: 4005 RVA: 0x0002A04F File Offset: 0x0002824F
		public string[] Modifiers { get; }

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x06000FA6 RID: 4006 RVA: 0x0002A057 File Offset: 0x00028257
		public bool IsCanceled { get; }

		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x06000FA7 RID: 4007 RVA: 0x0002A05F File Offset: 0x0002825F
		public CancelCause CancelCause { get; }

		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x06000FA8 RID: 4008 RVA: 0x0002A067 File Offset: 0x00028267
		public string CancelSource { get; }

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x06000FA9 RID: 4009 RVA: 0x0002A06F File Offset: 0x0002826F
		public string ExceptionString
		{
			[return: MaybeNull]
			get;
		}

		// Token: 0x06000FAA RID: 4010 RVA: 0x0002A078 File Offset: 0x00028278
		internal PhaseRecord(string name, string details, bool isModified, string[] modifiers, bool isCanceled, CancelCause cancelCause, [MaybeNull] GameEntity cancelSource = null, [MaybeNull] Exception exception = null)
		{
			this.Name = name;
			this.Details = details;
			this.IsModified = isModified;
			this.Modifiers = modifiers;
			this.IsCanceled = isCanceled;
			this.CancelCause = cancelCause;
			this.CancelSource = ((cancelSource != null) ? cancelSource.Name : null);
			this.ExceptionString = ((exception != null) ? exception.Message : null);
		}

		// Token: 0x06000FAB RID: 4011 RVA: 0x0002A0EC File Offset: 0x000282EC
		internal PhaseRecord(Phase phase)
			: this(phase.Name, phase.DebugDetails, phase.IsModified, phase.Modifiers, phase.IsCanceled, phase.CancelCause, phase.CancelSource, phase.Exception)
		{
			if (phase.Name.IsNullOrWhiteSpace())
			{
				this.Name = "<Unnamed>";
				Debug.LogWarning("Unnamed phase in " + phase.Action.Name);
			}
			phase.ClearModifiers();
		}
	}
}
