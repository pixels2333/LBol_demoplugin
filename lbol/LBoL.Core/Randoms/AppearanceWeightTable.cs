using System;
using LBoL.Base;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000E9 RID: 233
	public sealed class AppearanceWeightTable
	{
		// Token: 0x06000903 RID: 2307 RVA: 0x0001A4B9 File Offset: 0x000186B9
		public AppearanceWeightTable(float anywhere, float shopOnly, float nonShop, float nowhere)
		{
			this.Anywhere = anywhere;
			this.ShopOnly = shopOnly;
			this.NonShop = nonShop;
			this.Nowhere = nowhere;
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000904 RID: 2308 RVA: 0x0001A4DE File Offset: 0x000186DE
		public float Anywhere { get; }

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000905 RID: 2309 RVA: 0x0001A4E6 File Offset: 0x000186E6
		public float ShopOnly { get; }

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000906 RID: 2310 RVA: 0x0001A4EE File Offset: 0x000186EE
		public float NonShop { get; }

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000907 RID: 2311 RVA: 0x0001A4F6 File Offset: 0x000186F6
		public float Nowhere { get; }

		// Token: 0x06000908 RID: 2312 RVA: 0x0001A500 File Offset: 0x00018700
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

		// Token: 0x040004AE RID: 1198
		public static readonly AppearanceWeightTable AllOnes = new AppearanceWeightTable(1f, 1f, 1f, 1f);

		// Token: 0x040004AF RID: 1199
		public static readonly AppearanceWeightTable OnlyShopOnly = new AppearanceWeightTable(0f, 1f, 0f, 0f);

		// Token: 0x040004B0 RID: 1200
		public static readonly AppearanceWeightTable OnlyNonShop = new AppearanceWeightTable(0f, 0f, 1f, 0f);

		// Token: 0x040004B1 RID: 1201
		public static readonly AppearanceWeightTable NotInShop = new AppearanceWeightTable(1f, 0f, 1f, 0f);
	}
}
