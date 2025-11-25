using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public sealed class DollEventArgs : GameEventArgs
	{
		public Doll Doll { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Doll) ?? "";
		}
	}
}
