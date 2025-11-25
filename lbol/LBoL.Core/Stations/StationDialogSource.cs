using System;
namespace LBoL.Core.Stations
{
	public class StationDialogSource
	{
		public string DialogName { get; }
		public object CommandHandler { get; }
		public StationDialogSource(string dialogName, object commandHandler)
		{
			this.DialogName = dialogName;
			this.CommandHandler = commandHandler;
		}
	}
}
