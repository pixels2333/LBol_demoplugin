using System;
namespace LBoL.Core
{
	public struct ShieldInfo
	{
		public int Shield { readonly get; set; }
		public BlockShieldType Type { readonly get; set; }
		public ShieldInfo(int shield, BlockShieldType type = BlockShieldType.Normal)
		{
			this.Shield = shield;
			this.Type = type;
		}
	}
}
