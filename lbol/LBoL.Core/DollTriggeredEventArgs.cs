using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public sealed class DollTriggeredEventArgs : GameEventArgs
	{
		public Doll Doll { get; internal set; }
		public bool Remove { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} remove: {1}", GameEventArgs.DebugString(this.Doll), this.Remove);
		}
	}
}
