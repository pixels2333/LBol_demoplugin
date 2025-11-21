using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000B0 RID: 176
	public class SelectCardPayload
	{
		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060009BA RID: 2490 RVA: 0x00031845 File Offset: 0x0002FA45
		// (set) Token: 0x060009BB RID: 2491 RVA: 0x0003184D File Offset: 0x0002FA4D
		public string Name { get; set; }

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060009BC RID: 2492 RVA: 0x00031856 File Offset: 0x0002FA56
		// (set) Token: 0x060009BD RID: 2493 RVA: 0x0003185E File Offset: 0x0002FA5E
		public IEnumerable<Card> Cards { get; set; }

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x060009BE RID: 2494 RVA: 0x00031867 File Offset: 0x0002FA67
		// (set) Token: 0x060009BF RID: 2495 RVA: 0x0003186F File Offset: 0x0002FA6F
		public int Min { get; set; }

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x060009C0 RID: 2496 RVA: 0x00031878 File Offset: 0x0002FA78
		// (set) Token: 0x060009C1 RID: 2497 RVA: 0x00031880 File Offset: 0x0002FA80
		public int Max { get; set; }

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x060009C2 RID: 2498 RVA: 0x00031889 File Offset: 0x0002FA89
		// (set) Token: 0x060009C3 RID: 2499 RVA: 0x00031891 File Offset: 0x0002FA91
		public bool Sortable { get; set; }

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060009C4 RID: 2500 RVA: 0x0003189A File Offset: 0x0002FA9A
		// (set) Token: 0x060009C5 RID: 2501 RVA: 0x000318A2 File Offset: 0x0002FAA2
		public bool CanCancel { get; set; }

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x060009C6 RID: 2502 RVA: 0x000318AB File Offset: 0x0002FAAB
		// (set) Token: 0x060009C7 RID: 2503 RVA: 0x000318B3 File Offset: 0x0002FAB3
		public bool CanSkip { get; set; }

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x060009C8 RID: 2504 RVA: 0x000318BC File Offset: 0x0002FABC
		// (set) Token: 0x060009C9 RID: 2505 RVA: 0x000318C4 File Offset: 0x0002FAC4
		public bool IsAddCardToDeck { get; set; }
	}
}
