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
	public sealed class ShopStation : Station
	{
		public override StationType Type
		{
			get
			{
				return StationType.Shop;
			}
		}
		public List<ShopItem<Card>> ShopCards { get; private set; }
		private List<string> ShopCardIds
		{
			get
			{
				return Enumerable.ToList<string>(Enumerable.Select<ShopItem<Card>, string>(this.ShopCards, (ShopItem<Card> shopCard) => shopCard.Content.Id));
			}
		}
		public List<ShopItem<Exhibit>> ShopExhibits { get; private set; }
		public bool CanUseCardService { get; internal set; } = true;
		public int UpgradeDeckCardPrice
		{
			get
			{
				return base.GameRun.UpgradeDeckCardPrice;
			}
		}
		public int RemoveDeckCardPrice
		{
			get
			{
				return base.GameRun.RemoveDeckCardPrice;
			}
		}
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
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = StationType.Shop
			};
		}
		public int DiscountCardNo;
	}
}
