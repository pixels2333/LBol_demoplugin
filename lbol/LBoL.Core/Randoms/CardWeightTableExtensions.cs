using System;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000EC RID: 236
	public static class CardWeightTableExtensions
	{
		// Token: 0x06000923 RID: 2339 RVA: 0x0001AB39 File Offset: 0x00018D39
		public static CardWeightTable WithRarity(this CardWeightTable table, RarityWeightTable rarityTable)
		{
			return new CardWeightTable(rarityTable, table.OwnerTable, table.CardTypeTable, false);
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x0001AB4E File Offset: 0x00018D4E
		public static CardWeightTable WithOwner(this CardWeightTable table, OwnerWeightTable ownerTable)
		{
			return new CardWeightTable(table.RarityTable, ownerTable, table.CardTypeTable, false);
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0001AB63 File Offset: 0x00018D63
		public static CardWeightTable WithCardType(this CardWeightTable table, CardTypeWeightTable cardTypeTable)
		{
			return new CardWeightTable(table.RarityTable, table.OwnerTable, cardTypeTable, false);
		}
	}
}
