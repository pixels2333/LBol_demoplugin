using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base;
using LBoL.ConfigData;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000EB RID: 235
	public sealed class CardWeightTable
	{
		// Token: 0x06000915 RID: 2325 RVA: 0x0001A8E3 File Offset: 0x00018AE3
		public CardWeightTable(RarityWeightTable rarityTable, OwnerWeightTable ownerTable, CardTypeWeightTable cardTypeTable, bool includeOutsideKeyword = false)
		{
			this.RarityTable = rarityTable;
			this.OwnerTable = ownerTable;
			this.CardTypeTable = cardTypeTable;
			this.IncludeOutsideKeyword = includeOutsideKeyword;
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x0001A908 File Offset: 0x00018B08
		public CardWeightTable(RarityWeightTable rarityTable)
			: this(rarityTable, OwnerWeightTable.Valid, CardTypeWeightTable.AllOnes, false)
		{
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x0001A91C File Offset: 0x00018B1C
		public static implicit operator CardWeightTable(RarityWeightTable rarityTable)
		{
			return new CardWeightTable(rarityTable);
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0001A924 File Offset: 0x00018B24
		public CardWeightTable(OwnerWeightTable ownerTable)
			: this(RarityWeightTable.AllOnes, ownerTable, CardTypeWeightTable.AllOnes, false)
		{
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0001A938 File Offset: 0x00018B38
		public static implicit operator CardWeightTable(OwnerWeightTable ownerTable)
		{
			return new CardWeightTable(ownerTable);
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0001A940 File Offset: 0x00018B40
		public CardWeightTable(CardTypeWeightTable cardTypeTable)
			: this(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, cardTypeTable, false)
		{
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0001A954 File Offset: 0x00018B54
		public static implicit operator CardWeightTable(CardTypeWeightTable cardTypeTable)
		{
			return new CardWeightTable(cardTypeTable);
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x0600091C RID: 2332 RVA: 0x0001A95C File Offset: 0x00018B5C
		public RarityWeightTable RarityTable { get; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x0600091D RID: 2333 RVA: 0x0001A964 File Offset: 0x00018B64
		public OwnerWeightTable OwnerTable { get; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x0600091E RID: 2334 RVA: 0x0001A96C File Offset: 0x00018B6C
		public CardTypeWeightTable CardTypeTable { get; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x0600091F RID: 2335 RVA: 0x0001A974 File Offset: 0x00018B74
		public bool IncludeOutsideKeyword { get; }

		// Token: 0x06000920 RID: 2336 RVA: 0x0001A97C File Offset: 0x00018B7C
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

		// Token: 0x06000921 RID: 2337 RVA: 0x0001AA3E File Offset: 0x00018C3E
		public override string ToString()
		{
			return string.Format("CardWeightTable: rarity=[{0}], owner=[{1}], card-type=[{2}]", this.RarityTable, this.OwnerTable, this.CardTypeTable);
		}

		// Token: 0x040004C7 RID: 1223
		public static readonly CardWeightTable AllOnes = new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, CardTypeWeightTable.AllOnes, false);

		// Token: 0x040004C8 RID: 1224
		public static readonly CardWeightTable WithoutTool = new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false);

		// Token: 0x040004C9 RID: 1225
		public static readonly CardWeightTable OnlyTool = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false);

		// Token: 0x040004CA RID: 1226
		public static readonly CardWeightTable ShopAtk = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlyAttack, true);

		// Token: 0x040004CB RID: 1227
		public static readonly CardWeightTable ShopDef = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlyDefense, true);

		// Token: 0x040004CC RID: 1228
		public static readonly CardWeightTable ShopSkl = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlySkill, true);

		// Token: 0x040004CD RID: 1229
		public static readonly CardWeightTable ShopAbl = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.OnlyAbility, true);

		// Token: 0x040004CE RID: 1230
		public static readonly CardWeightTable ShopSkillAndFriend = new CardWeightTable(RarityWeightTable.ShopCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.ShopSkillAndFriend, true);

		// Token: 0x040004D3 RID: 1235
		private const float GadgetsRatio = 2f;
	}
}
