using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000B7 RID: 183
	public class ShowCardsPayload
	{
		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000A91 RID: 2705 RVA: 0x0003541C File Offset: 0x0003361C
		// (set) Token: 0x06000A92 RID: 2706 RVA: 0x00035424 File Offset: 0x00033624
		public string Name { get; set; }

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000A93 RID: 2707 RVA: 0x0003542D File Offset: 0x0003362D
		// (set) Token: 0x06000A94 RID: 2708 RVA: 0x00035435 File Offset: 0x00033635
		public string Description { get; set; }

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000A95 RID: 2709 RVA: 0x0003543E File Offset: 0x0003363E
		// (set) Token: 0x06000A96 RID: 2710 RVA: 0x00035446 File Offset: 0x00033646
		public IReadOnlyList<Card> Cards { get; set; }

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000A97 RID: 2711 RVA: 0x0003544F File Offset: 0x0003364F
		// (set) Token: 0x06000A98 RID: 2712 RVA: 0x00035457 File Offset: 0x00033657
		public bool CanCancel { get; set; } = true;

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000A99 RID: 2713 RVA: 0x00035460 File Offset: 0x00033660
		// (set) Token: 0x06000A9A RID: 2714 RVA: 0x00035468 File Offset: 0x00033668
		public InteractionType InteractionType { get; set; }

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000A9B RID: 2715 RVA: 0x00035471 File Offset: 0x00033671
		// (set) Token: 0x06000A9C RID: 2716 RVA: 0x00035479 File Offset: 0x00033679
		public ShowCardZone CardZone { get; set; }

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000A9D RID: 2717 RVA: 0x00035482 File Offset: 0x00033682
		// (set) Token: 0x06000A9E RID: 2718 RVA: 0x0003548A File Offset: 0x0003368A
		public bool HideActualOrder { get; set; }

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000A9F RID: 2719 RVA: 0x00035493 File Offset: 0x00033693
		// (set) Token: 0x06000AA0 RID: 2720 RVA: 0x0003549B File Offset: 0x0003369B
		public IReadOnlyList<Card> PayCards { get; set; }

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000AA1 RID: 2721 RVA: 0x000354A4 File Offset: 0x000336A4
		// (set) Token: 0x06000AA2 RID: 2722 RVA: 0x000354AC File Offset: 0x000336AC
		public int Money { get; set; }

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000AA3 RID: 2723 RVA: 0x000354B5 File Offset: 0x000336B5
		// (set) Token: 0x06000AA4 RID: 2724 RVA: 0x000354BD File Offset: 0x000336BD
		public int Price { get; set; }
	}
}
