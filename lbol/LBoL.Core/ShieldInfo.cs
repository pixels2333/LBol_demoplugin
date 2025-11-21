using System;

namespace LBoL.Core
{
	// Token: 0x0200006C RID: 108
	public struct ShieldInfo
	{
		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600047F RID: 1151 RVA: 0x0000FA18 File Offset: 0x0000DC18
		// (set) Token: 0x06000480 RID: 1152 RVA: 0x0000FA20 File Offset: 0x0000DC20
		public int Shield { readonly get; set; }

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000481 RID: 1153 RVA: 0x0000FA29 File Offset: 0x0000DC29
		// (set) Token: 0x06000482 RID: 1154 RVA: 0x0000FA31 File Offset: 0x0000DC31
		public BlockShieldType Type { readonly get; set; }

		// Token: 0x06000483 RID: 1155 RVA: 0x0000FA3A File Offset: 0x0000DC3A
		public ShieldInfo(int shield, BlockShieldType type = BlockShieldType.Normal)
		{
			this.Shield = shield;
			this.Type = type;
		}
	}
}
