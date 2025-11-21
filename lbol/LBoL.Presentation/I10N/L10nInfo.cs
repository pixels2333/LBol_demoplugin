using System;
using System.Globalization;

namespace LBoL.Presentation.I10N
{
	// Token: 0x020000F2 RID: 242
	public class L10nInfo
	{
		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000DC9 RID: 3529 RVA: 0x0004250C File Offset: 0x0004070C
		// (set) Token: 0x06000DCA RID: 3530 RVA: 0x00042514 File Offset: 0x00040714
		public float VnTextRevealSpeed { get; set; }

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000DCB RID: 3531 RVA: 0x0004251D File Offset: 0x0004071D
		// (set) Token: 0x06000DCC RID: 3532 RVA: 0x00042525 File Offset: 0x00040725
		public float VnTextRevealAhead { get; set; }

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000DCD RID: 3533 RVA: 0x0004252E File Offset: 0x0004072E
		// (set) Token: 0x06000DCE RID: 3534 RVA: 0x00042536 File Offset: 0x00040736
		public bool PreferShortName { get; set; }

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000DCF RID: 3535 RVA: 0x0004253F File Offset: 0x0004073F
		// (set) Token: 0x06000DD0 RID: 3536 RVA: 0x00042547 File Offset: 0x00040747
		public bool PreferItalicInFlavor { get; set; }

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000DD1 RID: 3537 RVA: 0x00042550 File Offset: 0x00040750
		// (set) Token: 0x06000DD2 RID: 3538 RVA: 0x00042558 File Offset: 0x00040758
		public bool PreferWideTooltip { get; set; }

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000DD3 RID: 3539 RVA: 0x00042561 File Offset: 0x00040761
		// (set) Token: 0x06000DD4 RID: 3540 RVA: 0x00042569 File Offset: 0x00040769
		public bool HideExhibitRarity { get; set; }

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000DD5 RID: 3541 RVA: 0x00042572 File Offset: 0x00040772
		// (set) Token: 0x06000DD6 RID: 3542 RVA: 0x0004257A File Offset: 0x0004077A
		public CultureInfo Culture { get; set; }
	}
}
