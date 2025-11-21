using System;

namespace LBoL.Core
{
	// Token: 0x0200003D RID: 61
	public class ScryEventArgs : GameEventArgs
	{
		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060001D5 RID: 469 RVA: 0x00004B42 File Offset: 0x00002D42
		// (set) Token: 0x060001D6 RID: 470 RVA: 0x00004B4A File Offset: 0x00002D4A
		public ScryInfo ScryInfo { get; set; }

		// Token: 0x060001D7 RID: 471 RVA: 0x00004B54 File Offset: 0x00002D54
		protected override string GetBaseDebugString()
		{
			return this.ScryInfo.Count.ToString();
		}
	}
}
