using System;
using System.Linq;

namespace LBoL.Core
{
	// Token: 0x0200001C RID: 28
	public class ExhibitsEventArgs : GameEventArgs
	{
		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000F3 RID: 243 RVA: 0x00003DE2 File Offset: 0x00001FE2
		// (set) Token: 0x060000F4 RID: 244 RVA: 0x00003DEA File Offset: 0x00001FEA
		public Exhibit[] Exhibits { get; internal set; }

		// Token: 0x060000F5 RID: 245 RVA: 0x00003DF3 File Offset: 0x00001FF3
		protected override string GetBaseDebugString()
		{
			if (this.Exhibits == null)
			{
				return "";
			}
			return "Exhibit = " + string.Join(", ", Enumerable.Select<Exhibit, string>(this.Exhibits, new Func<Exhibit, string>(GameEventArgs.DebugString)));
		}
	}
}
