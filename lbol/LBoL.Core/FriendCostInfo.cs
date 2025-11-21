using System;

namespace LBoL.Core
{
	// Token: 0x02000010 RID: 16
	public struct FriendCostInfo
	{
		// Token: 0x060000A3 RID: 163 RVA: 0x000034F3 File Offset: 0x000016F3
		public FriendCostInfo(int cost, FriendCostType costType)
		{
			this.Cost = cost;
			this.CostType = costType;
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x00003503 File Offset: 0x00001703
		// (set) Token: 0x060000A5 RID: 165 RVA: 0x0000350B File Offset: 0x0000170B
		public int Cost { readonly get; set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x00003514 File Offset: 0x00001714
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x0000351C File Offset: 0x0000171C
		public FriendCostType CostType { readonly get; set; }
	}
}
