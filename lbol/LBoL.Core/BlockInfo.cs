using System;

namespace LBoL.Core
{
	// Token: 0x02000007 RID: 7
	public struct BlockInfo
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002195 File Offset: 0x00000395
		// (set) Token: 0x0600000A RID: 10 RVA: 0x0000219D File Offset: 0x0000039D
		public int Block { readonly get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000B RID: 11 RVA: 0x000021A6 File Offset: 0x000003A6
		// (set) Token: 0x0600000C RID: 12 RVA: 0x000021AE File Offset: 0x000003AE
		public BlockShieldType Type { readonly get; set; }

		// Token: 0x0600000D RID: 13 RVA: 0x000021B7 File Offset: 0x000003B7
		public BlockInfo(int block, BlockShieldType type = BlockShieldType.Normal)
		{
			this.Block = block;
			this.Type = type;
		}
	}
}
