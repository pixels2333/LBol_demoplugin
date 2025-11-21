using System;
using LBoL.Base;

namespace LBoL.Core
{
	// Token: 0x0200003B RID: 59
	public class ManaEventArgs : GameEventArgs
	{
		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x00004A81 File Offset: 0x00002C81
		// (set) Token: 0x060001CA RID: 458 RVA: 0x00004A89 File Offset: 0x00002C89
		public ManaGroup Value { get; set; }

		// Token: 0x060001CB RID: 459 RVA: 0x00004A92 File Offset: 0x00002C92
		protected override string GetBaseDebugString()
		{
			return string.Format("Mana = [{0}]", this.Value);
		}
	}
}
