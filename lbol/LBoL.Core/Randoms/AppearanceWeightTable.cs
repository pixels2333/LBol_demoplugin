using System;
using LBoL.Base;
namespace LBoL.Core.Randoms
{
	public sealed class AppearanceWeightTable
	{
		public AppearanceWeightTable(float anywhere, float shopOnly, float nonShop, float nowhere)
		{
			this.Anywhere = anywhere;
			this.ShopOnly = shopOnly;
			this.NonShop = nonShop;
			this.Nowhere = nowhere;
		}
		public float Anywhere { get; }
		public float ShopOnly { get; }
		public float NonShop { get; }
		public float Nowhere { get; }
		public float WeightFor(AppearanceType appearance)
		{
			float num;
			switch (appearance)
			{
			case AppearanceType.Anywhere:
				num = this.Anywhere;
				break;
			case AppearanceType.ShopOnly:
				num = this.ShopOnly;
				break;
			case AppearanceType.NonShop:
				num = this.NonShop;
				break;
			case AppearanceType.Nowhere:
				num = this.Nowhere;
				break;
			default:
				throw new ArgumentOutOfRangeException("appearance", appearance, null);
			}
			return num;
		}
		public static readonly AppearanceWeightTable AllOnes = new AppearanceWeightTable(1f, 1f, 1f, 1f);
		public static readonly AppearanceWeightTable OnlyShopOnly = new AppearanceWeightTable(0f, 1f, 0f, 0f);
		public static readonly AppearanceWeightTable OnlyNonShop = new AppearanceWeightTable(0f, 0f, 1f, 0f);
		public static readonly AppearanceWeightTable NotInShop = new AppearanceWeightTable(1f, 0f, 1f, 0f);
	}
}
