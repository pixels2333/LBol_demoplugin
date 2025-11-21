using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000029 RID: 41
	public class StatusEffectEventArgs : GameEventArgs
	{
		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000140 RID: 320 RVA: 0x00004246 File Offset: 0x00002446
		// (set) Token: 0x06000141 RID: 321 RVA: 0x0000424E File Offset: 0x0000244E
		public Unit Unit { get; internal set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000142 RID: 322 RVA: 0x00004257 File Offset: 0x00002457
		// (set) Token: 0x06000143 RID: 323 RVA: 0x0000425F File Offset: 0x0000245F
		public StatusEffect Effect { get; internal set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000144 RID: 324 RVA: 0x00004268 File Offset: 0x00002468
		// (set) Token: 0x06000145 RID: 325 RVA: 0x00004270 File Offset: 0x00002470
		public float WaitTime { get; set; }

		// Token: 0x06000146 RID: 326 RVA: 0x00004279 File Offset: 0x00002479
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Effect) + " -> " + GameEventArgs.DebugString(this.Unit);
		}
	}
}
