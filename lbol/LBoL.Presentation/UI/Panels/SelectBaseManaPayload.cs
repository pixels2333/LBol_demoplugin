using System;
using LBoL.Base;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000AE RID: 174
	public class SelectBaseManaPayload
	{
		// Token: 0x17000180 RID: 384
		// (get) Token: 0x0600099B RID: 2459 RVA: 0x000312FA File Offset: 0x0002F4FA
		// (set) Token: 0x0600099C RID: 2460 RVA: 0x00031302 File Offset: 0x0002F502
		public ManaGroup BaseMana { get; set; }

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x0600099D RID: 2461 RVA: 0x0003130B File Offset: 0x0002F50B
		// (set) Token: 0x0600099E RID: 2462 RVA: 0x00031313 File Offset: 0x0002F513
		public int Min { get; set; }

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x0600099F RID: 2463 RVA: 0x0003131C File Offset: 0x0002F51C
		// (set) Token: 0x060009A0 RID: 2464 RVA: 0x00031324 File Offset: 0x0002F524
		public int Max { get; set; }

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x060009A1 RID: 2465 RVA: 0x0003132D File Offset: 0x0002F52D
		// (set) Token: 0x060009A2 RID: 2466 RVA: 0x00031335 File Offset: 0x0002F535
		public bool CanCancel { get; set; }
	}
}
