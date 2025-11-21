using System;
using LBoL.Core.Cards;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000DB RID: 219
	public sealed class RemoveCardContent
	{
		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000D3C RID: 3388 RVA: 0x00040CA4 File Offset: 0x0003EEA4
		// (set) Token: 0x06000D3D RID: 3389 RVA: 0x00040CAC File Offset: 0x0003EEAC
		public Card Card { get; set; }

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000D3E RID: 3390 RVA: 0x00040CB5 File Offset: 0x0003EEB5
		// (set) Token: 0x06000D3F RID: 3391 RVA: 0x00040CBD File Offset: 0x0003EEBD
		public Action OnConfirm { get; set; }
	}
}
