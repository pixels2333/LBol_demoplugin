using System;
using LBoL.Base;
namespace LBoL.Core
{
	public class ManaConvertingEventArgs : GameEventArgs
	{
		public ManaGroup Input { get; set; }
		public ManaGroup Output { get; set; }
		public bool AllowPartial { get; set; }
		protected override string GetBaseDebugString()
		{
			if (!this.AllowPartial)
			{
				return string.Format("Mana [{0}] => [{1}]", this.Input, this.Output);
			}
			return string.Format("Mana [{0}] (up-to) => [{1}]", this.Input, this.Output);
		}
	}
}
