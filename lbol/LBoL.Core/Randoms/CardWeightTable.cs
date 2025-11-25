using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base;
using LBoL.ConfigData;
namespace LBoL.Core.Randoms
{
	public sealed class CardWeightTable
	{
		public CardWeightTable(RarityWeightTable rarityTable, OwnerWeightTable ownerTable, CardTypeWeightTable cardTypeTable, bool includeOutsideKeyword = false)
		{
			this.RarityTable = rarityTable;
			this.OwnerTable = ownerTable;
			this.CardTypeTable = cardTypeTable;
			this.IncludeOutsideKeyword = includeOutsideKeyword;
		}
		public CardWeightTable(RarityWeightTable rarityTable)
			: this(rarityTable, OwnerWeightTable.Valid, CardTypeWeightTable.AllOnes, false)
		{
		}
		public static implicit operator CardWeightTable(RarityWeightTable rarityTable)
		{
			return new CardWeightTable(rarityTable);
		}
		public CardWeightTable(OwnerWeightTable ownerTable)
			: this(RarityWeightTable.AllOnes, ownerTable, CardTypeWeightTable.AllOnes, false)
		{
		}
		public static implicit operator CardWeightTable(OwnerWeightTable ownerTable)
		{
			return new CardWeightTable(ownerTable);
		}
		public CardWeightTable(CardTypeWeightTable cardTypeTable)
			: this(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, cardTypeTable, false)
		{
		}
		public static implicit operator CardWeightTable(CardTypeWeightTable cardTypeTable)
		{
			return new CardWeightTable(cardTypeTable);
		}
		public RarityWeightTable RarityTable { get; }
		public OwnerWeightTable OwnerTable { get; }
		public CardTypeWeightTable CardTypeTable { get; }
		public bool IncludeOutsideKeyword { get; }
		public float WeightFor(CardConfig cardConfig, [MaybeNull] string playerId, ISet<string> exhibitOwnerIds)
		{
			float num = this.RarityTable.WeightFor(cardConfig.Rarity);
			string owner = cardConfig.Owner;
			if (owner != null)
			{
				if (playerId != null && owner == playerId)
				{
					num *= this.OwnerTable.Player;
				}
				else if (exhibitOwnerIds.Contains(owner))
				{
					num *= this.OwnerTable.ExhibitOwner;
				}
				else
				{
					num *= this.OwnerTable.Other;
				}
			}
			else
			{
				num *= this.OwnerTable.Neutral;
			}
			num *= this.CardTypeTable.WeightFor(cardConfig.Type);
			if (cardConfig.Keywords.HasFlag(Keyword.Gadgets))
			{
				num *= (this.IncludeOutsideKeyword ? 2f : 0f);
			}
			return num;
		}
		public override string ToString()
		{
			return string.Format("CardWeightTable: rarity=[{0}], owner=[{1}], card-type=[{2}]", this.RarityTable, this.OwnerTable, this.CardTypeTable);
		}
		public static readonly CardWeightTable AllOnes = new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, CardTypeWeightTable.AllOnes, false);
		public static readonly CardWeightTable WithoutTool = new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false);
		public static readonly CardWeightTable OnlyTool = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false);
		public static readonly CardWeightTable ShopAtk = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlyAttack, true);
		public static readonly CardWeightTable ShopDef = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlyDefense, true);
		public static readonly CardWeightTable ShopSkl = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlySkill, true);
		public static readonly CardWeightTable ShopAbl = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlyAbility, true);
		public static readonly CardWeightTable ShopSkillAndFriend = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.ShopSkillAndFriend, true);
		private const float GadgetsRatio = 2f;
	}
}
