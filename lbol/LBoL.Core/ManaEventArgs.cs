using System;
using LBoL.Base;
namespace LBoL.Core
{
	public class ManaEventArgs : GameEventArgs
	{
		public ManaGroup Value { get; set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Mana = [{0}]", this.Value);
		}
	}
}
