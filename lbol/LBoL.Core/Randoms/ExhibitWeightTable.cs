using System;
using LBoL.ConfigData;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000ED RID: 237
	public sealed class ExhibitWeightTable
	{
		// Token: 0x06000926 RID: 2342 RVA: 0x0001AB78 File Offset: 0x00018D78
		public ExhibitWeightTable(RarityWeightTable rarityTable, AppearanceWeightTable appearanceTable)
		{
			this.RarityTable = rarityTable;
			this.AppearanceTable = appearanceTable;
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x0001AB8E File Offset: 0x00018D8E
		public ExhibitWeightTable(RarityWeightTable rarityTable)
			: this(rarityTable, AppearanceWeightTable.AllOnes)
		{
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x0001AB9C File Offset: 0x00018D9C
		public ExhibitWeightTable(AppearanceWeightTable shopTable)
			: this(RarityWeightTable.AllOnes, shopTable)
		{
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0001ABAA File Offset: 0x00018DAA
		public static implicit operator ExhibitWeightTable(RarityWeightTable rarityTable)
		{
			return new ExhibitWeightTable(rarityTable);
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0001ABB2 File Offset: 0x00018DB2
		public static implicit operator ExhibitWeightTable(AppearanceWeightTable appearanceTable)
		{
			return new ExhibitWeightTable(appearanceTable);
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x0600092B RID: 2347 RVA: 0x0001ABBA File Offset: 0x00018DBA
		public RarityWeightTable RarityTable { get; }

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x0600092C RID: 2348 RVA: 0x0001ABC2 File Offset: 0x00018DC2
		public AppearanceWeightTable AppearanceTable { get; }

		// Token: 0x0600092D RID: 2349 RVA: 0x0001ABCA File Offset: 0x00018DCA
		public float WeightFor(ExhibitConfig exhibitConfig)
		{
			return this.RarityTable.WeightFor(exhibitConfig.Rarity) * this.AppearanceTable.WeightFor(exhibitConfig.Appearance);
		}

		// Token: 0x040004D4 RID: 1236
		public static readonly ExhibitWeightTable AllOnes = new ExhibitWeightTable(RarityWeightTable.AllOnes, AppearanceWeightTable.AllOnes);
	}
}
