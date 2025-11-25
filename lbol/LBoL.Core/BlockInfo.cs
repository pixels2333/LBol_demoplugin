using System;
namespace LBoL.Core
{
	public struct BlockInfo
	{
		public int Block { readonly get; set; }
		public BlockShieldType Type { readonly get; set; }
		public BlockInfo(int block, BlockShieldType type = BlockShieldType.Normal)
		{
			this.Block = block;
			this.Type = type;
		}
	}
}
