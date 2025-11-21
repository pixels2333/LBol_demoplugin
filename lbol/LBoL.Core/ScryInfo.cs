using System;

namespace LBoL.Core
{
	// Token: 0x0200006B RID: 107
	public struct ScryInfo
	{
		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x0000F9EF File Offset: 0x0000DBEF
		// (set) Token: 0x0600047C RID: 1148 RVA: 0x0000F9F7 File Offset: 0x0000DBF7
		public int Count { readonly get; set; }

		// Token: 0x0600047D RID: 1149 RVA: 0x0000FA00 File Offset: 0x0000DC00
		public ScryInfo(int count)
		{
			this.Count = count;
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x0000FA09 File Offset: 0x0000DC09
		public ScryInfo IncreasedBy(int delta)
		{
			return new ScryInfo(this.Count + delta);
		}
	}
}
