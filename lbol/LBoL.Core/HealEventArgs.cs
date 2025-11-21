using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000037 RID: 55
	public class HealEventArgs : GameEventArgs
	{
		// Token: 0x1700008C RID: 140
		// (get) Token: 0x060001B1 RID: 433 RVA: 0x0000495C File Offset: 0x00002B5C
		// (set) Token: 0x060001B2 RID: 434 RVA: 0x00004964 File Offset: 0x00002B64
		public Unit Source { get; internal set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060001B3 RID: 435 RVA: 0x0000496D File Offset: 0x00002B6D
		// (set) Token: 0x060001B4 RID: 436 RVA: 0x00004975 File Offset: 0x00002B75
		public Unit Target { get; internal set; }

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x060001B5 RID: 437 RVA: 0x0000497E File Offset: 0x00002B7E
		// (set) Token: 0x060001B6 RID: 438 RVA: 0x00004986 File Offset: 0x00002B86
		public float Amount { get; set; }

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060001B7 RID: 439 RVA: 0x0000498F File Offset: 0x00002B8F
		// (set) Token: 0x060001B8 RID: 440 RVA: 0x00004997 File Offset: 0x00002B97
		public HealType HealType { get; set; }

		// Token: 0x060001B9 RID: 441 RVA: 0x000049A0 File Offset: 0x00002BA0
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} --- {1} --> {2}", GameEventArgs.DebugString(this.Source), this.Amount, GameEventArgs.DebugString(this.Target));
		}
	}
}
