using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200002C RID: 44
	public sealed class DollEventArgs : GameEventArgs
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000160 RID: 352 RVA: 0x00004422 File Offset: 0x00002622
		// (set) Token: 0x06000161 RID: 353 RVA: 0x0000442A File Offset: 0x0000262A
		public Doll Doll { get; internal set; }

		// Token: 0x06000162 RID: 354 RVA: 0x00004433 File Offset: 0x00002633
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Doll) ?? "";
		}
	}
}
