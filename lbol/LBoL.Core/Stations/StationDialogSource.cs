using System;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C7 RID: 199
	public class StationDialogSource
	{
		// Token: 0x170002CA RID: 714
		// (get) Token: 0x060008AF RID: 2223 RVA: 0x000197B8 File Offset: 0x000179B8
		public string DialogName { get; }

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x060008B0 RID: 2224 RVA: 0x000197C0 File Offset: 0x000179C0
		public object CommandHandler { get; }

		// Token: 0x060008B1 RID: 2225 RVA: 0x000197C8 File Offset: 0x000179C8
		public StationDialogSource(string dialogName, object commandHandler)
		{
			this.DialogName = dialogName;
			this.CommandHandler = commandHandler;
		}
	}
}
