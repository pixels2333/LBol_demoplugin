using System;

namespace LBoL.Core
{
	// Token: 0x0200003A RID: 58
	public class PowerEventArgs : GameEventArgs
	{
		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x00004A51 File Offset: 0x00002C51
		// (set) Token: 0x060001C6 RID: 454 RVA: 0x00004A59 File Offset: 0x00002C59
		public int Power { get; set; }

		// Token: 0x060001C7 RID: 455 RVA: 0x00004A62 File Offset: 0x00002C62
		protected override string GetBaseDebugString()
		{
			return string.Format("Power = {0}", this.Power);
		}
	}
}
