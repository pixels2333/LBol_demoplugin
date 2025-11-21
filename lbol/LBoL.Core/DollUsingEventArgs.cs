using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000028 RID: 40
	public class DollUsingEventArgs : GameEventArgs
	{
		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000138 RID: 312 RVA: 0x000041E4 File Offset: 0x000023E4
		// (set) Token: 0x06000139 RID: 313 RVA: 0x000041EC File Offset: 0x000023EC
		public Doll Doll { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600013A RID: 314 RVA: 0x000041F5 File Offset: 0x000023F5
		// (set) Token: 0x0600013B RID: 315 RVA: 0x000041FD File Offset: 0x000023FD
		public UnitSelector Selector { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600013C RID: 316 RVA: 0x00004206 File Offset: 0x00002406
		// (set) Token: 0x0600013D RID: 317 RVA: 0x0000420E File Offset: 0x0000240E
		public int ConsumingMagic { get; set; }

		// Token: 0x0600013E RID: 318 RVA: 0x00004217 File Offset: 0x00002417
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Doll) + " -> {" + GameEventArgs.DebugString(this.Selector) + "}";
		}
	}
}
