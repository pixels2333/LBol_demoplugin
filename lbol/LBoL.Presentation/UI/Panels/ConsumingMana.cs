using System;
using LBoL.Base;
namespace LBoL.Presentation.UI.Panels
{
	public class ConsumingMana
	{
		public ManaGroup Unpooled { get; }
		public ManaGroup Pooled { get; }
		public ManaGroup TotalMana
		{
			get
			{
				return this.Unpooled + this.Pooled;
			}
		}
		public ConsumingMana(ManaGroup unpooled, ManaGroup pooled)
		{
			this.Unpooled = unpooled;
			this.Pooled = pooled;
		}
		public override string ToString()
		{
			return string.Format("{0}+{1}", this.Unpooled, this.Pooled);
		}
	}
}
