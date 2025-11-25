using System;
namespace LBoL.Core
{
	public class PowerEventArgs : GameEventArgs
	{
		public int Power { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Power = {0}", this.Power);
		}
	}
}
