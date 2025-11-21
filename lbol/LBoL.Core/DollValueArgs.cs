using System;

namespace LBoL.Core
{
	// Token: 0x02000038 RID: 56
	public class DollValueArgs : GameEventArgs
	{
		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060001BB RID: 443 RVA: 0x000049D5 File Offset: 0x00002BD5
		// (set) Token: 0x060001BC RID: 444 RVA: 0x000049DD File Offset: 0x00002BDD
		public int Value { get; set; }

		// Token: 0x060001BD RID: 445 RVA: 0x000049E6 File Offset: 0x00002BE6
		protected override string GetBaseDebugString()
		{
			return string.Format("Value = {0}", this.Value);
		}
	}
}
