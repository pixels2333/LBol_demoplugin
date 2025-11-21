using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000032 RID: 50
	public class StatisticalDamageEventArgs : GameEventArgs
	{
		// Token: 0x1700007C RID: 124
		// (get) Token: 0x0600018C RID: 396 RVA: 0x0000469A File Offset: 0x0000289A
		// (set) Token: 0x0600018D RID: 397 RVA: 0x000046A2 File Offset: 0x000028A2
		public Unit DamageSource { get; internal set; }

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600018E RID: 398 RVA: 0x000046AB File Offset: 0x000028AB
		// (set) Token: 0x0600018F RID: 399 RVA: 0x000046B3 File Offset: 0x000028B3
		public IReadOnlyDictionary<Unit, IReadOnlyList<DamageEventArgs>> ArgsTable { get; internal set; }
	}
}
