using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class DollUsingEventArgs : GameEventArgs
	{
		public Doll Doll { get; set; }
		public UnitSelector Selector { get; set; }
		public int ConsumingMagic { get; set; }
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Doll) + " -> {" + GameEventArgs.DebugString(this.Selector) + "}";
		}
	}
}
