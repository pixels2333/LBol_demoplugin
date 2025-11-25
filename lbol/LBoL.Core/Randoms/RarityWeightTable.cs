using System;
using LBoL.Base;
namespace LBoL.Core.Randoms
{
	public sealed class RarityWeightTable
	{
		public RarityWeightTable(float common, float uncommon, float rare, float mythic)
		{
			this.Common = common;
			this.Uncommon = uncommon;
			this.Rare = rare;
			this.Mythic = mythic;
		}
		public float Common { get; }
		public float Uncommon { get; }
		public float Rare { get; }
		public float Mythic { get; }
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
		public override string ToString()
		{
			return string.Format("RarityWeightTable common=[{0}], uncommon=[{1}], rare=[{2}], mythic=[{3}]", new object[] { this.Common, this.Uncommon, this.Rare, this.Mythic });
		}
		public static readonly RarityWeightTable AllOnes = new RarityWeightTable(1f, 1f, 1f, 1f);
		public static readonly RarityWeightTable BattleCard = new RarityWeightTable(1f, 1f, 0.5f, 0f);
		public static readonly RarityWeightTable NonCommon = new RarityWeightTable(0f, 1f, 0.5f, 0f);
		public static readonly RarityWeightTable NoneRare = new RarityWeightTable(1f, 1f, 0f, 0f);
		public static readonly RarityWeightTable EnemyCard = new RarityWeightTable(1f, 0.5f, 0.05f, 0f);
		public static readonly RarityWeightTable EliteCard = new RarityWeightTable(1f, 0.8f, 0.15f, 0f);
		public static readonly RarityWeightTable ShopCard = new RarityWeightTable(1f, 0.7f, 0.15f, 0f);
		public static readonly RarityWeightTable OnlyCommon = new RarityWeightTable(1f, 0f, 0f, 0f);
		public static readonly RarityWeightTable OnlyUncommon = new RarityWeightTable(0f, 1f, 0f, 0f);
		public static readonly RarityWeightTable OnlyRare = new RarityWeightTable(0f, 0f, 1f, 0f);
	}
}
