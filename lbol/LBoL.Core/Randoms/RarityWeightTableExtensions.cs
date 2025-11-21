using System;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000F2 RID: 242
	public static class RarityWeightTableExtensions
	{
		// Token: 0x06000943 RID: 2371 RVA: 0x0001AFD9 File Offset: 0x000191D9
		public static RarityWeightTable WithCommon(this RarityWeightTable table, float common)
		{
			return new RarityWeightTable(common, table.Uncommon, table.Rare, table.Mythic);
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0001AFF3 File Offset: 0x000191F3
		public static RarityWeightTable WithUncommon(this RarityWeightTable table, float uncommon)
		{
			return new RarityWeightTable(table.Common, uncommon, table.Rare, table.Mythic);
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x0001B00D File Offset: 0x0001920D
		public static RarityWeightTable WithRare(this RarityWeightTable table, float rare)
		{
			return new RarityWeightTable(table.Common, table.Uncommon, rare, table.Mythic);
		}
	}
}
