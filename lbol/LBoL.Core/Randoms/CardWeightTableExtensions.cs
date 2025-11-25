using System;
namespace LBoL.Core.Randoms
{
	public static class CardWeightTableExtensions
	{
		public static CardWeightTable WithRarity(this CardWeightTable table, RarityWeightTable rarityTable)
		{
			return new CardWeightTable(rarityTable, table.OwnerTable, table.CardTypeTable, false);
		}
		public static CardWeightTable WithOwner(this CardWeightTable table, OwnerWeightTable ownerTable)
		{
			return new CardWeightTable(table.RarityTable, ownerTable, table.CardTypeTable, false);
		}
		public static CardWeightTable WithCardType(this CardWeightTable table, CardTypeWeightTable cardTypeTable)
		{
			return new CardWeightTable(table.RarityTable, table.OwnerTable, cardTypeTable, false);
		}
	}
}
