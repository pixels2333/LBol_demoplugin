using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000152 RID: 338
	public class RewardInteraction : Interaction
	{
		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x06000D8A RID: 3466 RVA: 0x00025744 File Offset: 0x00023944
		public IReadOnlyList<Exhibit> PendingExhibits { get; }

		// Token: 0x06000D8B RID: 3467 RVA: 0x0002574C File Offset: 0x0002394C
		public RewardInteraction(IEnumerable<Exhibit> exhibits)
		{
			this.PendingExhibits = new List<Exhibit>(exhibits).AsReadOnly();
		}
	}
}
