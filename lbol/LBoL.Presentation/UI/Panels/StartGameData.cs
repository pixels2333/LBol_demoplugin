using System;
using LBoL.Core;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000BB RID: 187
	public class StartGameData
	{
		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06000AAE RID: 2734 RVA: 0x00035753 File Offset: 0x00033953
		// (set) Token: 0x06000AAF RID: 2735 RVA: 0x0003575B File Offset: 0x0003395B
		public Func<Stage[]> StagesCreateFunc { get; set; }

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06000AB0 RID: 2736 RVA: 0x00035764 File Offset: 0x00033964
		// (set) Token: 0x06000AB1 RID: 2737 RVA: 0x0003576C File Offset: 0x0003396C
		public Type DebutAdventure { get; set; }
	}
}
