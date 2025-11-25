using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class ForceKillEventArgs : GameEventArgs
	{
		public Unit Source { get; internal set; }
		public Unit Target { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Source) + " -> " + GameEventArgs.DebugString(this.Target);
		}
	}
}
