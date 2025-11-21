using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200002D RID: 45
	public sealed class DollTriggeredEventArgs : GameEventArgs
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000164 RID: 356 RVA: 0x00004451 File Offset: 0x00002651
		// (set) Token: 0x06000165 RID: 357 RVA: 0x00004459 File Offset: 0x00002659
		public Doll Doll { get; internal set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000166 RID: 358 RVA: 0x00004462 File Offset: 0x00002662
		// (set) Token: 0x06000167 RID: 359 RVA: 0x0000446A File Offset: 0x0000266A
		public bool Remove { get; set; }

		// Token: 0x06000168 RID: 360 RVA: 0x00004473 File Offset: 0x00002673
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} remove: {1}", GameEventArgs.DebugString(this.Doll), this.Remove);
		}
	}
}
