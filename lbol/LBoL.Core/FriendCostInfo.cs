using System;
namespace LBoL.Core
{
	public struct FriendCostInfo
	{
		public FriendCostInfo(int cost, FriendCostType costType)
		{
			this.Cost = cost;
			this.CostType = costType;
		}
		public int Cost { readonly get; set; }
		public FriendCostType CostType { readonly get; set; }
	}
}
