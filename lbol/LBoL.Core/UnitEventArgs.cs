using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class UnitEventArgs : GameEventArgs
	{
		public Unit Unit { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return "Unit = " + GameEventArgs.DebugString(this.Unit);
		}
	}
}
