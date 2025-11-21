using System;

namespace LBoL.Core
{
	// Token: 0x0200004D RID: 77
	public class VisualSourceData
	{
		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000363 RID: 867 RVA: 0x0000B6C1 File Offset: 0x000098C1
		// (set) Token: 0x06000364 RID: 868 RVA: 0x0000B6C9 File Offset: 0x000098C9
		public VisualSourceType SourceType { get; set; }

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000365 RID: 869 RVA: 0x0000B6D2 File Offset: 0x000098D2
		// (set) Token: 0x06000366 RID: 870 RVA: 0x0000B6DA File Offset: 0x000098DA
		public int Index { get; set; }

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x06000367 RID: 871 RVA: 0x0000B6E3 File Offset: 0x000098E3
		// (set) Token: 0x06000368 RID: 872 RVA: 0x0000B6EB File Offset: 0x000098EB
		public GameEntity Source { get; set; }
	}
}
