using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using UnityEngine;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C5 RID: 197
	public sealed class ShopStation : Station
	{
		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000878 RID: 2168 RVA: 0x00018C68 File Offset: 0x00016E68
		public override StationType Type
		{
			get
			{
				return StationType.Shop;
			}
		}

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000879 RID: 2169 RVA: 0x00018C6B File Offset: 0x00016E6B
		// (set) Token: 0x0600087A RID: 2170 RVA: 0x00018C73 File Offset: 0x00016E73
		public List<ShopItem<Card>> ShopCards { get; private set; }

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x0600087B RID: 2171 RVA: 0x00018C7C File Offset: 0x00016E7C
		private List<string> ShopCardIds
		{
			get
			{
				return Enumerable.ToList<string>(Enumerable.Select<ShopItem<Card>, string>(this.ShopCards, (ShopItem<Card> shopCard) => shopCard.Content.Id));
			}
		}

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x0600087C RID: 2172 RVA: 0x00018CAD File Offset: 0x00016EAD
		// (set) Token: 0x0600087D RID: 2173 RVA: 0x00018CB5 File Offset: 0x00016EB5
		public List<ShopItem<Exhibit>> ShopExhibits { get; private set; }

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x0600087E RID: 2174 RVA: 0x00018CBE File Offset: 0x00016EBE
		// (set) Token: 0x0600087F RID: 2175 RVA: 0x00018CC6 File Offset: 0x00016EC6
		public bool CanUseCardService { get; internal set; } = true;

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000880 RID: 2176 RVA: 0x00018CCF File Offset: 0x00016ECF
		public int UpgradeDeckCardPrice
		{
			get
			{
				return base.GameRun.UpgradeDeckCardPrice;
			}
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000881 RID: 2177 RVA: 0x00018CDC File Offset: 0x00016EDC
		public int RemoveDeckCardPrice
		{
			get
			{
				return base.GameRun.RemoveDeckCardPrice;
			}
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x00018CEC File Offset: 0x00016EEC
		protected internal override void OnEnter()
		{
			List<ShopItem<Card>> list = new List<ShopItem<Card>>();
			this.DiscountCardNo = base.GameRun.ShopRng.NextInt(0, 7);
			foreach (Card card in base.Stage.GetShopNormalCards())
			{
				list.Add(new ShopItem<Card>(base.GameRun, card, this.GetPrice(card, list.Count == this.DiscountCardNo), false, false));
			}
			list[this.DiscountCardNo].IsDiscounted = true;
			foreach (Card card2 in base.Stage.GetShopToolCards(2))
			{
				list.Add(new ShopItem<Card>(base.GameRun, card2, this.GetPrice(card2, false), false, false));
			}
			this.ShopCards = list;
			List<ShopItem<Exhibit>> list2 = new List<ShopItem<Exhibit>>();
			for (int j = 0; j < 3; j++)
			{
				Exhibit shopExhibit = base.Stage.GetShopExhibit(j == 2);
				list2.Add(new ShopItem<Exhibit>(base.GameRun, shopExhibit, this.GetPrice(shopExhibit), false, false));
			}
			this.ShopExhibits = list2;
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00018E04 File Offset: 0x00017004
		private int GetPrice(Card card, bool isDiscounted)
		{
			float num2;
			if (card.CardType == CardType.Tool)
			{
				int num;
				switch (card.Config.Rarity)
				{
				case Rarity.Common:
					num = GlobalConfig.ToolPrices[0];
					break;
				case Rarity.Uncommon:
					num = GlobalConfig.ToolPrices[1];
					break;
				case Rarity.Rare:
					num = GlobalConfig.ToolPrices[2];
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				num2 = (float)num;
			}
			else
			{
				int num;
				switch (card.Config.Rarity)
				{
				case Rarity.Common:
					num = GlobalConfig.CardPrices[0];
					break;
				case Rarity.Uncommon:
					num = GlobalConfig.CardPrices[1];
					break;
				case Rarity.Rare:
					num = GlobalConfig.CardPrices[2];
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				num2 = (float)num;
			}
			float num3 = base.GameRun.ShopRng.NextFloat(-0.08f, 0f) + 1f;
			num2 *= num3;
			if (isDiscounted)
			{
				num2 /= 2f;
			}
			return Mathf.RoundToInt(num2);
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x00018EE0 File Offset: 0x000170E0
		private int GetPrice(Exhibit exhibit)
		{
			int num;
			switch (exhibit.Config.Rarity)
			{
			case Rarity.Common:
				num = GlobalConfig.ExhibitPrices[0];
				break;
			case Rarity.Uncommon:
				num = GlobalConfig.ExhibitPrices[1];
				break;
			case Rarity.Rare:
				num = GlobalConfig.ExhibitPrices[2];
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			float num2 = (float)num;
			float num3 = base.GameRun.ShopRng.NextFloat(-0.08f, 0f) + 1f;
			return Mathf.RoundToInt(num2 * num3);
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x00018F5C File Offset: 0x0001715C
		public void BuyCard(ShopItem<Card> cardItem)
		{
			int num = this.ShopCards.IndexOf(cardItem);
			if (num < 0)
			{
				throw new ArgumentException(cardItem.Content.Name + " not in shop.");
			}
			if (cardItem.IsSoldOut)
			{
				throw new ArgumentException(cardItem.Content.Name + " is already sold out.");
			}
			base.GameRun.BuyCard(cardItem);
			if (base.GameRun.ShopResupplyFlag > 0)
			{
				Card card = base.Stage.SupplyShopCard(cardItem.Content, this.ShopCardIds);
				this.ShopCards[num] = new ShopItem<Card>(base.GameRun, card, this.GetPrice(card, false), false, false);
			}
			this.RefreshAfterBought();
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x00019012 File Offset: 0x00017212
		public IEnumerator BuyExhibitRunner(ShopItem<Exhibit> exhibitItem)
		{
			int index = this.ShopExhibits.IndexOf(exhibitItem);
			if (index < 0)
			{
				throw new ArgumentException(exhibitItem.Content.Name + " not in shop.");
			}
			if (exhibitItem.IsSoldOut)
			{
				throw new ArgumentException(exhibitItem.Content.Name + " is already sold out.");
			}
			yield return base.GameRun.BuyExhibitRunner(exhibitItem, new VisualSourceData
			{
				SourceType = VisualSourceType.Shop,
				Index = index
			});
			foreach (ShopItem<Card> shopItem in this.ShopCards)
			{
				base.GameRun.UpgradeNewDeckCardOnFlags(shopItem.Content);
			}
			if (base.GameRun.ShopResupplyFlag > 0)
			{
				Exhibit shopExhibit = base.Stage.GetShopExhibit(false);
				this.ShopExhibits[index] = new ShopItem<Exhibit>(base.GameRun, shopExhibit, this.GetPrice(shopExhibit), false, false);
			}
			this.RefreshAfterBought();
			yield break;
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x00019028 File Offset: 0x00017228
		public void UpgradeDeckCard(Card card)
		{
			GameRunController gameRun = base.GameRun;
			if (gameRun.Money < this.UpgradeDeckCardPrice)
			{
				throw new InvalidOperationException("Insufficient fund");
			}
			gameRun.ConsumeMoney(this.UpgradeDeckCardPrice);
			gameRun.Stats.ShopConsumed += this.UpgradeDeckCardPrice;
			gameRun.UpgradeDeckCard(card, true);
			this.CanUseCardService = false;
			this.RefreshAfterBought();
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x0001908C File Offset: 0x0001728C
		public void RemoveDeckCard(Card card)
		{
			GameRunController gameRun = base.GameRun;
			if (gameRun.Money < this.RemoveDeckCardPrice)
			{
				throw new InvalidOperationException("Insufficient fund");
			}
			gameRun.ConsumeMoney(this.RemoveDeckCardPrice);
			gameRun.Stats.ShopConsumed += this.RemoveDeckCardPrice;
			gameRun.RemoveDeckCard(card, true);
			this.CanUseCardService = false;
			int num = gameRun.ShopRemoveCardCounter + 1;
			gameRun.ShopRemoveCardCounter = num;
			this.RefreshAfterBought();
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x00019100 File Offset: 0x00017300
		public void RefreshAfterBought()
		{
			foreach (ShopItem<Card> shopItem in this.ShopCards)
			{
				shopItem.Content.NotifyChanged();
			}
			foreach (ShopItem<Exhibit> shopItem2 in this.ShopExhibits)
			{
				shopItem2.Content.NotifyChanged();
			}
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x0001919C File Offset: 0x0001739C
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = StationType.Shop
			};
		}

		// Token: 0x0400039F RID: 927
		public int DiscountCardNo;
	}
}
