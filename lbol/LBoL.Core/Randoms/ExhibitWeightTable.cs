using System;
using LBoL.ConfigData;
namespace LBoL.Core.Randoms
{
	public sealed class ExhibitWeightTable
	{
		public ExhibitWeightTable(RarityWeightTable rarityTable, AppearanceWeightTable appearanceTable)
		{
			this.RarityTable = rarityTable;
			this.AppearanceTable = appearanceTable;
		}
		public ExhibitWeightTable(RarityWeightTable rarityTable)
			: this(rarityTable, AppearanceWeightTable.AllOnes)
		{
		}
		public ExhibitWeightTable(AppearanceWeightTable shopTable)
			: this(RarityWeightTable.AllOnes, shopTable)
		{
		}
		public static implicit operator ExhibitWeightTable(RarityWeightTable rarityTable)
		{
			return new ExhibitWeightTable(rarityTable);
		}
		public static implicit operator ExhibitWeightTable(AppearanceWeightTable appearanceTable)
		{
			return new ExhibitWeightTable(appearanceTable);
		}
		public RarityWeightTable RarityTable { get; }
		public AppearanceWeightTable AppearanceTable { get; }
		public float WeightFor(ExhibitConfig exhibitConfig)
		{
			return this.RarityTable.WeightFor(exhibitConfig.Rarity) * this.AppearanceTable.WeightFor(exhibitConfig.Appearance);
		}
		public static readonly ExhibitWeightTable AllOnes = new ExhibitWeightTable(RarityWeightTable.AllOnes, AppearanceWeightTable.AllOnes);
	}
}
