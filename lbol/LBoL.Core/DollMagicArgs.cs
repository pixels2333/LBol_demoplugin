using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class DollMagicArgs : GameEventArgs
	{
		public Doll Doll { get; set; }
		public int Magic { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Doll = {0}, Magic = {1}", GameEventArgs.DebugString(this.Doll), this.Magic);
		}
	}
}
