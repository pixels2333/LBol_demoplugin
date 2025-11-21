using System;
using LBoL.Core.Stations;

namespace LBoL.Core
{
	// Token: 0x0200001B RID: 27
	public class StationEventArgs : GameEventArgs
	{
		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000EF RID: 239 RVA: 0x00003DB7 File Offset: 0x00001FB7
		// (set) Token: 0x060000F0 RID: 240 RVA: 0x00003DBF File Offset: 0x00001FBF
		public Station Station { get; internal set; }

		// Token: 0x060000F1 RID: 241 RVA: 0x00003DC8 File Offset: 0x00001FC8
		protected override string GetBaseDebugString()
		{
			return string.Format("Station = {0}", this.Station);
		}
	}
}
