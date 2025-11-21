using System;

namespace LBoL.Core.Adventures
{
	// Token: 0x020001BB RID: 443
	public class AdventureInfoAttribute : Attribute
	{
		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x06000FDA RID: 4058 RVA: 0x0002A5D2 File Offset: 0x000287D2
		// (set) Token: 0x06000FDB RID: 4059 RVA: 0x0002A5DA File Offset: 0x000287DA
		public Type WeighterType { get; set; }
	}
}
