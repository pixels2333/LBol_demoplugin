using System;
namespace LBoL.Core.Randoms
{
	public sealed class OwnerWeightTable
	{
		public OwnerWeightTable(float player, float exhibitOwner, float other, float neutral)
		{
			this.Player = player;
			this.ExhibitOwner = exhibitOwner;
			this.Other = other;
			this.Neutral = neutral;
		}
		public float Player { get; }
		public float ExhibitOwner { get; }
		public float Other { get; }
		public float Neutral { get; }
		public static readonly OwnerWeightTable AllOnes = new OwnerWeightTable(1f, 1f, 1f, 1f);
		public static readonly OwnerWeightTable Valid = new OwnerWeightTable(1f, 1f, 0f, 1f);
		public static readonly OwnerWeightTable OnlyPlayer = new OwnerWeightTable(1f, 0f, 0f, 0f);
		public static readonly OwnerWeightTable OnlyFriend = new OwnerWeightTable(0f, 1f, 0f, 0f);
		public static readonly OwnerWeightTable OnlyOther = new OwnerWeightTable(0f, 0f, 1f, 0f);
		public static readonly OwnerWeightTable OnlyNeutral = new OwnerWeightTable(0f, 0f, 0f, 1f);
		public static readonly OwnerWeightTable WithoutPlayer = new OwnerWeightTable(0f, 1f, 0f, 1f);
		public static readonly OwnerWeightTable PlayerAndFriend = new OwnerWeightTable(0.2f, 0.8f, 0f, 0f);
		public static readonly OwnerWeightTable Hierarchy = new OwnerWeightTable(1f, 0.8f, 0f, 0.7f);
	}
}
