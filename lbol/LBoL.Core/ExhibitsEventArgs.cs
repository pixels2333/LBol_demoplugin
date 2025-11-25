using System;
using System.Linq;
namespace LBoL.Core
{
	public class ExhibitsEventArgs : GameEventArgs
	{
		public Exhibit[] Exhibits { get; internal set; }
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
