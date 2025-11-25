using System;
using LBoL.Core.Stations;
namespace LBoL.Core
{
	public class StationEventArgs : GameEventArgs
	{
		public Station Station { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Station = {0}", this.Station);
		}
	}
}
