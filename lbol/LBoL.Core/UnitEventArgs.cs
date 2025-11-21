using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200001A RID: 26
	public class UnitEventArgs : GameEventArgs
	{
		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00003D87 File Offset: 0x00001F87
		// (set) Token: 0x060000EC RID: 236 RVA: 0x00003D8F File Offset: 0x00001F8F
		public Unit Unit { get; internal set; }

		// Token: 0x060000ED RID: 237 RVA: 0x00003D98 File Offset: 0x00001F98
		protected override string GetBaseDebugString()
		{
			return "Unit = " + GameEventArgs.DebugString(this.Unit);
		}
	}
}
