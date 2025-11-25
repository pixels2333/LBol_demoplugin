using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class DamageEventArgs : GameEventArgs
	{
		public Unit Source { get; internal set; }
		public Unit Target { get; internal set; }
		public string GunName { get; internal set; }
		public DamageInfo DamageInfo { get; set; }
		public bool Kill { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} --- {1} --> {2}", GameEventArgs.DebugString(this.Source), this.DamageInfo, GameEventArgs.DebugString(this.Target));
		}
	}
}
