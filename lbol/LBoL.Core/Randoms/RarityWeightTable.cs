using System;
using LBoL.Base;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000F1 RID: 241
	public sealed class RarityWeightTable
	{
		// Token: 0x0600093B RID: 2363 RVA: 0x0001ADA3 File Offset: 0x00018FA3
		public RarityWeightTable(float common, float uncommon, float rare, float mythic)
		{
			this.Common = common;
			this.Uncommon = uncommon;
			this.Rare = rare;
			this.Mythic = mythic;
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x0600093C RID: 2364 RVA: 0x0001ADC8 File Offset: 0x00018FC8
		public float Common { get; }

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x0600093D RID: 2365 RVA: 0x0001ADD0 File Offset: 0x00018FD0
		public float Uncommon { get; }

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x0600093E RID: 2366 RVA: 0x0001ADD8 File Offset: 0x00018FD8
		public float Rare { get; }

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x0600093F RID: 2367 RVA: 0x0001ADE0 File Offset: 0x00018FE0
		public float Mythic { get; }

		// Token: 0x06000940 RID: 2368 RVA: 0x0001ADE8 File Offset: 0x00018FE8
		public float WeightFor(Rarity rarity)
		{
			switch (rarity)
			{
			case Rarity.Common:
				return this.Common;
			case Rarity.Uncommon:
				return this.Uncommon;
			case Rarity.Rare:
				return this.Rare;
			case Rarity.Mythic:
				return this.Mythic;
			}
			throw new ArgumentOutOfRangeException("rarity", rarity, null);
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x0001AE48 File Offset: 0x00019048
		public override string ToString()
		{
			return string.Format("RarityWeightTable common=[{0}], uncommon=[{1}], rare=[{2}], mythic=[{3}]", new object[] { this.Common, this.Uncommon, this.Rare, this.Mythic });
		}

		// Token: 0x040004EA RID: 1258
		public static readonly RarityWeightTable AllOnes = new RarityWeightTable(1f, 1f, 1f, 1f);

		// Token: 0x040004EB RID: 1259
		public static readonly RarityWeightTable BattleCard = new RarityWeightTable(1f, 1f, 0.5f, 0f);

		// Token: 0x040004EC RID: 1260
		public static readonly RarityWeightTable NonCommon = new RarityWeightTable(0f, 1f, 0.5f, 0f);

		// Token: 0x040004ED RID: 1261
		public static readonly RarityWeightTable NoneRare = new RarityWeightTable(1f, 1f, 0f, 0f);

		// Token: 0x040004EE RID: 1262
		public static readonly RarityWeightTable EnemyCard = new RarityWeightTable(1f, 0.5f, 0.05f, 0f);

		// Token: 0x040004EF RID: 1263
		public static readonly RarityWeightTable EliteCard = new RarityWeightTable(1f, 0.8f, 0.15f, 0f);

		// Token: 0x040004F0 RID: 1264
		public static readonly RarityWeightTable ShopCard = new RarityWeightTable(1f, 0.7f, 0.15f, 0f);

		// Token: 0x040004F1 RID: 1265
		public static readonly RarityWeightTable OnlyCommon = new RarityWeightTable(1f, 0f, 0f, 0f);

		// Token: 0x040004F2 RID: 1266
		public static readonly RarityWeightTable OnlyUncommon = new RarityWeightTable(0f, 1f, 0f, 0f);

		// Token: 0x040004F3 RID: 1267
		public static readonly RarityWeightTable OnlyRare = new RarityWeightTable(0f, 0f, 1f, 0f);
	}
}
