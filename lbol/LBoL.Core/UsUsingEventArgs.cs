using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class UsUsingEventArgs : GameEventArgs
	{
		public UltimateSkill Us { get; set; }
		public UnitSelector Selector { get; set; }
		public int ConsumingPower { get; set; }
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Us) + " -> {" + GameEventArgs.DebugString(this.Selector) + "}";
		}
	}
}
