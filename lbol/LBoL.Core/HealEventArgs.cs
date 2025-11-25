using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class HealEventArgs : GameEventArgs
	{
		public Unit Source { get; internal set; }
		public Unit Target { get; internal set; }
		public float Amount { get; set; }
		public HealType HealType { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} --- {1} --> {2}", GameEventArgs.DebugString(this.Source), this.Amount, GameEventArgs.DebugString(this.Target));
		}
	}
}
