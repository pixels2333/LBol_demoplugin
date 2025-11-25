using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public sealed class DollMagicEventArgs : GameEventArgs
	{
		public Doll Doll { get; internal set; }
		public int Magic { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("{0} magic: {1}", GameEventArgs.DebugString(this.Doll), this.Magic);
		}
	}
}
