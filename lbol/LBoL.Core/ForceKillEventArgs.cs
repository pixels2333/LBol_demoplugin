using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000031 RID: 49
	public class ForceKillEventArgs : GameEventArgs
	{
		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000186 RID: 390 RVA: 0x0000464E File Offset: 0x0000284E
		// (set) Token: 0x06000187 RID: 391 RVA: 0x00004656 File Offset: 0x00002856
		public Unit Source { get; internal set; }

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000188 RID: 392 RVA: 0x0000465F File Offset: 0x0000285F
		// (set) Token: 0x06000189 RID: 393 RVA: 0x00004667 File Offset: 0x00002867
		public Unit Target { get; internal set; }

		// Token: 0x0600018A RID: 394 RVA: 0x00004670 File Offset: 0x00002870
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Source) + " -> " + GameEventArgs.DebugString(this.Target);
		}
	}
}
