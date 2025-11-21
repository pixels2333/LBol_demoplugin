using System;
using System.Collections.Generic;
using LBoL.Core.Stations;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000AC RID: 172
	public class ShowRewardContent
	{
		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000978 RID: 2424 RVA: 0x00030681 File Offset: 0x0002E881
		// (set) Token: 0x06000979 RID: 2425 RVA: 0x00030689 File Offset: 0x0002E889
		public RewardType RewardType { get; set; }

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x0600097A RID: 2426 RVA: 0x00030692 File Offset: 0x0002E892
		// (set) Token: 0x0600097B RID: 2427 RVA: 0x0003069A File Offset: 0x0002E89A
		public List<StationReward> Rewards { get; set; }

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x0600097C RID: 2428 RVA: 0x000306A3 File Offset: 0x0002E8A3
		// (set) Token: 0x0600097D RID: 2429 RVA: 0x000306AB File Offset: 0x0002E8AB
		public Station Station { get; set; }

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x0600097E RID: 2430 RVA: 0x000306B4 File Offset: 0x0002E8B4
		// (set) Token: 0x0600097F RID: 2431 RVA: 0x000306BC File Offset: 0x0002E8BC
		public bool ShowNextButton { get; set; }
	}
}
