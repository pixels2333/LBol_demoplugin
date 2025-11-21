using System;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000D9 RID: 217
	public sealed class MessageContent
	{
		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000D1D RID: 3357 RVA: 0x00040998 File Offset: 0x0003EB98
		// (set) Token: 0x06000D1E RID: 3358 RVA: 0x000409A0 File Offset: 0x0003EBA0
		public string Text { get; set; }

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06000D1F RID: 3359 RVA: 0x000409A9 File Offset: 0x0003EBA9
		// (set) Token: 0x06000D20 RID: 3360 RVA: 0x000409B1 File Offset: 0x0003EBB1
		public string TextKey { get; set; }

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x06000D21 RID: 3361 RVA: 0x000409BA File Offset: 0x0003EBBA
		// (set) Token: 0x06000D22 RID: 3362 RVA: 0x000409C2 File Offset: 0x0003EBC2
		public object[] TextArguments { get; set; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06000D23 RID: 3363 RVA: 0x000409CB File Offset: 0x0003EBCB
		// (set) Token: 0x06000D24 RID: 3364 RVA: 0x000409D3 File Offset: 0x0003EBD3
		public string SubText { get; set; }

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06000D25 RID: 3365 RVA: 0x000409DC File Offset: 0x0003EBDC
		// (set) Token: 0x06000D26 RID: 3366 RVA: 0x000409E4 File Offset: 0x0003EBE4
		public string SubTextKey { get; set; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06000D27 RID: 3367 RVA: 0x000409ED File Offset: 0x0003EBED
		// (set) Token: 0x06000D28 RID: 3368 RVA: 0x000409F5 File Offset: 0x0003EBF5
		public object[] SubTextArguments { get; set; }

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x06000D29 RID: 3369 RVA: 0x000409FE File Offset: 0x0003EBFE
		// (set) Token: 0x06000D2A RID: 3370 RVA: 0x00040A06 File Offset: 0x0003EC06
		public DialogButtons Buttons { get; set; }

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x06000D2B RID: 3371 RVA: 0x00040A0F File Offset: 0x0003EC0F
		// (set) Token: 0x06000D2C RID: 3372 RVA: 0x00040A17 File Offset: 0x0003EC17
		public MessageIcon Icon { get; set; }

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06000D2D RID: 3373 RVA: 0x00040A20 File Offset: 0x0003EC20
		// (set) Token: 0x06000D2E RID: 3374 RVA: 0x00040A28 File Offset: 0x0003EC28
		public Action OnConfirm { get; set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06000D2F RID: 3375 RVA: 0x00040A31 File Offset: 0x0003EC31
		// (set) Token: 0x06000D30 RID: 3376 RVA: 0x00040A39 File Offset: 0x0003EC39
		public Action OnCancel { get; set; }

		// Token: 0x040009EC RID: 2540
		private string _text;

		// Token: 0x040009ED RID: 2541
		private string _textKey;

		// Token: 0x040009EE RID: 2542
		private string _subText;

		// Token: 0x040009EF RID: 2543
		private string _subTextKey;
	}
}
