using System;
namespace LBoL.Core
{
	public class ScryEventArgs : GameEventArgs
	{
		public ScryInfo ScryInfo { get; set; }
		protected override string GetBaseDebugString()
		{
			return this.ScryInfo.Count.ToString();
		}
	}
}
