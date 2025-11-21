using System;
using LBoL.Core.Cards;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000DF RID: 223
	public sealed class UpgradeCardContent
	{
		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000D5C RID: 3420 RVA: 0x00041035 File Offset: 0x0003F235
		// (set) Token: 0x06000D5D RID: 3421 RVA: 0x0004103D File Offset: 0x0003F23D
		public Card Card { get; set; }

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000D5E RID: 3422 RVA: 0x00041046 File Offset: 0x0003F246
		// (set) Token: 0x06000D5F RID: 3423 RVA: 0x0004104E File Offset: 0x0003F24E
		public int Price { get; set; }

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000D60 RID: 3424 RVA: 0x00041057 File Offset: 0x0003F257
		// (set) Token: 0x06000D61 RID: 3425 RVA: 0x0004105F File Offset: 0x0003F25F
		public int Money { get; set; }

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000D62 RID: 3426 RVA: 0x00041068 File Offset: 0x0003F268
		// (set) Token: 0x06000D63 RID: 3427 RVA: 0x00041070 File Offset: 0x0003F270
		public Action OnConfirm { get; set; }
	}
}
