using System;
namespace LBoL.Core
{
	public class DollValueArgs : GameEventArgs
	{
		public int Value { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Value = {0}", this.Value);
		}
	}
}
