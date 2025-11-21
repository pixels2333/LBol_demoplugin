using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000030 RID: 48
	public class DamageEventArgs : GameEventArgs
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600017A RID: 378 RVA: 0x000045C4 File Offset: 0x000027C4
		// (set) Token: 0x0600017B RID: 379 RVA: 0x000045CC File Offset: 0x000027CC
		public Unit Source { get; internal set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600017C RID: 380 RVA: 0x000045D5 File Offset: 0x000027D5
		// (set) Token: 0x0600017D RID: 381 RVA: 0x000045DD File Offset: 0x000027DD
		public Unit Target { get; internal set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600017E RID: 382 RVA: 0x000045E6 File Offset: 0x000027E6
		// (set) Token: 0x0600017F RID: 383 RVA: 0x000045EE File Offset: 0x000027EE
		public string GunName { get; internal set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000180 RID: 384 RVA: 0x000045F7 File Offset: 0x000027F7
		// (set) Token: 0x06000181 RID: 385 RVA: 0x000045FF File Offset: 0x000027FF
		public DamageInfo DamageInfo { get; set; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000182 RID: 386 RVA: 0x00004608 File Offset: 0x00002808
		// (set) Token: 0x06000183 RID: 387 RVA: 0x00004610 File Offset: 0x00002810
		public bool Kill { get; set; }

		// Token: 0x06000184 RID: 388 RVA: 0x00004619 File Offset: 0x00002819
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} --- {1} --> {2}", GameEventArgs.DebugString(this.Source), this.DamageInfo, GameEventArgs.DebugString(this.Target));
		}
	}
}
