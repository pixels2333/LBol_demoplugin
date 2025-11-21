using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000027 RID: 39
	public class UsUsingEventArgs : GameEventArgs
	{
		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000130 RID: 304 RVA: 0x00004182 File Offset: 0x00002382
		// (set) Token: 0x06000131 RID: 305 RVA: 0x0000418A File Offset: 0x0000238A
		public UltimateSkill Us { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00004193 File Offset: 0x00002393
		// (set) Token: 0x06000133 RID: 307 RVA: 0x0000419B File Offset: 0x0000239B
		public UnitSelector Selector { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000134 RID: 308 RVA: 0x000041A4 File Offset: 0x000023A4
		// (set) Token: 0x06000135 RID: 309 RVA: 0x000041AC File Offset: 0x000023AC
		public int ConsumingPower { get; set; }

		// Token: 0x06000136 RID: 310 RVA: 0x000041B5 File Offset: 0x000023B5
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Us) + " -> {" + GameEventArgs.DebugString(this.Selector) + "}";
		}
	}
}
