using System;
using System.Linq;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200002F RID: 47
	public class DamageDealingEventArgs : GameEventArgs
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000170 RID: 368 RVA: 0x000044E9 File Offset: 0x000026E9
		// (set) Token: 0x06000171 RID: 369 RVA: 0x000044F1 File Offset: 0x000026F1
		public Unit Source { get; internal set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000172 RID: 370 RVA: 0x000044FA File Offset: 0x000026FA
		// (set) Token: 0x06000173 RID: 371 RVA: 0x00004502 File Offset: 0x00002702
		public Unit[] Targets { get; internal set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000174 RID: 372 RVA: 0x0000450B File Offset: 0x0000270B
		// (set) Token: 0x06000175 RID: 373 RVA: 0x00004513 File Offset: 0x00002713
		public string GunName { get; internal set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000176 RID: 374 RVA: 0x0000451C File Offset: 0x0000271C
		// (set) Token: 0x06000177 RID: 375 RVA: 0x00004524 File Offset: 0x00002724
		public DamageInfo DamageInfo { get; set; }

		// Token: 0x06000178 RID: 376 RVA: 0x00004530 File Offset: 0x00002730
		protected override string GetBaseDebugString()
		{
			Unit[] targets = this.Targets;
			if (targets == null || targets.Length != 1)
			{
				return string.Format("{0} --- {1} --> [{2}]", GameEventArgs.DebugString(this.Source), this.DamageInfo, string.Join(", ", Enumerable.Select<Unit, string>(this.Targets, new Func<Unit, string>(GameEventArgs.DebugString))));
			}
			return string.Format("{0} --- {1} --> {2}", GameEventArgs.DebugString(this.Source), this.DamageInfo, GameEventArgs.DebugString(this.Targets[0]));
		}
	}
}
