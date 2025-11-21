using System;
using LBoL.Core.Cards;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000DD RID: 221
	public sealed class TransformCardContent
	{
		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000D4B RID: 3403 RVA: 0x00040E31 File Offset: 0x0003F031
		// (set) Token: 0x06000D4C RID: 3404 RVA: 0x00040E39 File Offset: 0x0003F039
		public Card Card { get; set; }

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000D4D RID: 3405 RVA: 0x00040E42 File Offset: 0x0003F042
		// (set) Token: 0x06000D4E RID: 3406 RVA: 0x00040E4A File Offset: 0x0003F04A
		public Card TransformCard { get; set; }

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06000D4F RID: 3407 RVA: 0x00040E53 File Offset: 0x0003F053
		// (set) Token: 0x06000D50 RID: 3408 RVA: 0x00040E5B File Offset: 0x0003F05B
		public Action OnConfirm { get; set; }
	}
}
