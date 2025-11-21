using System;
using System.Collections.Generic;
using System.Linq;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000124 RID: 292
	public sealed class DialogOptionsPhase : DialogPhase
	{
		// Token: 0x1700034B RID: 843
		// (get) Token: 0x06000A66 RID: 2662 RVA: 0x0001D36A File Offset: 0x0001B56A
		public DialogOption[] Options { get; }

		// Token: 0x06000A67 RID: 2663 RVA: 0x0001D372 File Offset: 0x0001B572
		internal DialogOptionsPhase(IEnumerable<DialogOption> options)
		{
			this.Options = Enumerable.ToArray<DialogOption>(options);
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x0001D386 File Offset: 0x0001B586
		public override string ToString()
		{
			return string.Format("Options: [{0}]", this.Options.Length);
		}
	}
}
