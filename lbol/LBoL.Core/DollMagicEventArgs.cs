using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200002E RID: 46
	public sealed class DollMagicEventArgs : GameEventArgs
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600016A RID: 362 RVA: 0x0000449D File Offset: 0x0000269D
		// (set) Token: 0x0600016B RID: 363 RVA: 0x000044A5 File Offset: 0x000026A5
		public Doll Doll { get; internal set; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600016C RID: 364 RVA: 0x000044AE File Offset: 0x000026AE
		// (set) Token: 0x0600016D RID: 365 RVA: 0x000044B6 File Offset: 0x000026B6
		public int Magic { get; set; }

		// Token: 0x0600016E RID: 366 RVA: 0x000044BF File Offset: 0x000026BF
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} magic: {1}", GameEventArgs.DebugString(this.Doll), this.Magic);
		}
	}
}
