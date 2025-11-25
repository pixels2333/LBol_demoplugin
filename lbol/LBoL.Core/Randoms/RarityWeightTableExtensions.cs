using System;
namespace LBoL.Core.Randoms
{
	public static class RarityWeightTableExtensions
	{
		public static RarityWeightTable WithCommon(this RarityWeightTable table, float common)
		{
			return new RarityWeightTable(common, table.Uncommon, table.Rare, table.Mythic);
		}
		public static RarityWeightTable WithUncommon(this RarityWeightTable table, float uncommon)
		{
			return new RarityWeightTable(table.Common, uncommon, table.Rare, table.Mythic);
		}
		public static RarityWeightTable WithRare(this RarityWeightTable table, float rare)
		{
			return new RarityWeightTable(table.Common, table.Uncommon, rare, table.Mythic);
		}
	}
}
