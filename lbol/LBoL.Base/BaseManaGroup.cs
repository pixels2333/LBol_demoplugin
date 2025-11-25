using System;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	[ConfigValueConverter(typeof(BaseManaGroupConverter), new string[] { })]
	public struct BaseManaGroup
	{
		public ManaGroup Value { readonly get; set; }
		public BaseManaGroup(ManaGroup mana)
		{
			this.Value = mana;
		}
		public static implicit operator ManaGroup(BaseManaGroup baseMana)
		{
			return baseMana.Value;
		}
		public static implicit operator BaseManaGroup(ManaGroup mana)
		{
			return new BaseManaGroup(mana);
		}
	}
}
