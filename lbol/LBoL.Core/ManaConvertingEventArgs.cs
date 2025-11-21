using System;
using LBoL.Base;

namespace LBoL.Core
{
	// Token: 0x0200003C RID: 60
	public class ManaConvertingEventArgs : GameEventArgs
	{
		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060001CD RID: 461 RVA: 0x00004AB1 File Offset: 0x00002CB1
		// (set) Token: 0x060001CE RID: 462 RVA: 0x00004AB9 File Offset: 0x00002CB9
		public ManaGroup Input { get; set; }

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060001CF RID: 463 RVA: 0x00004AC2 File Offset: 0x00002CC2
		// (set) Token: 0x060001D0 RID: 464 RVA: 0x00004ACA File Offset: 0x00002CCA
		public ManaGroup Output { get; set; }

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x00004AD3 File Offset: 0x00002CD3
		// (set) Token: 0x060001D2 RID: 466 RVA: 0x00004ADB File Offset: 0x00002CDB
		public bool AllowPartial { get; set; }

		// Token: 0x060001D3 RID: 467 RVA: 0x00004AE4 File Offset: 0x00002CE4
		protected override string GetBaseDebugString()
		{
			if (!this.AllowPartial)
			{
				return string.Format("Mana [{0}] => [{1}]", this.Input, this.Output);
			}
			return string.Format("Mana [{0}] (up-to) => [{1}]", this.Input, this.Output);
		}
	}
}
