using System;
using System.Collections.Generic;
using LBoL.Core;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000097 RID: 151
	public class GameResultData
	{
		// Token: 0x17000149 RID: 329
		// (get) Token: 0x060007CA RID: 1994 RVA: 0x00024647 File Offset: 0x00022847
		// (set) Token: 0x060007CB RID: 1995 RVA: 0x0002464F File Offset: 0x0002284F
		public string PlayerId { get; set; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x060007CC RID: 1996 RVA: 0x00024658 File Offset: 0x00022858
		// (set) Token: 0x060007CD RID: 1997 RVA: 0x00024660 File Offset: 0x00022860
		public GameResultType Type { get; set; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x060007CE RID: 1998 RVA: 0x00024669 File Offset: 0x00022869
		// (set) Token: 0x060007CF RID: 1999 RVA: 0x00024671 File Offset: 0x00022871
		public int PreviousTotalExp { get; set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x060007D0 RID: 2000 RVA: 0x0002467A File Offset: 0x0002287A
		// (set) Token: 0x060007D1 RID: 2001 RVA: 0x00024682 File Offset: 0x00022882
		public int BluePoint { get; set; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x060007D2 RID: 2002 RVA: 0x0002468B File Offset: 0x0002288B
		// (set) Token: 0x060007D3 RID: 2003 RVA: 0x00024693 File Offset: 0x00022893
		public float DifficultyMultipler { get; set; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x060007D4 RID: 2004 RVA: 0x0002469C File Offset: 0x0002289C
		// (set) Token: 0x060007D5 RID: 2005 RVA: 0x000246A4 File Offset: 0x000228A4
		public List<ScoreData> ScoreDatas { get; set; }

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x060007D6 RID: 2006 RVA: 0x000246AD File Offset: 0x000228AD
		// (set) Token: 0x060007D7 RID: 2007 RVA: 0x000246B5 File Offset: 0x000228B5
		public int DebugExp { get; set; }
	}
}
