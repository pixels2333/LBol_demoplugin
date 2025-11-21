using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000B5 RID: 181
	public class ShopPanel : UiPanel<ShopStation>
	{
		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000A36 RID: 2614 RVA: 0x00033AF4 File Offset: 0x00031CF4
		private static int MaxCardCount
		{
			get
			{
				return 10;
			}
		}

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000A37 RID: 2615 RVA: 0x00033AF8 File Offset: 0x00031CF8
		private static int MaxExhibitCount
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000A38 RID: 2616 RVA: 0x00033AFB File Offset: 0x00031CFB
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000A39 RID: 2617 RVA: 0x00033AFE File Offset: 0x00031CFE
		// (set) Token: 0x06000A3A RID: 2618 RVA: 0x00033B06 File Offset: 0x00031D06
		private ShopStation ShopStation { get; set; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000A3B RID: 2619 RVA: 0x00033B0F File Offset: 0x00031D0F
		// (set) Token: 0x06000A3C RID: 2620 RVA: 0x00033B17 File Offset: 0x00031D17
		public bool LockedByInteractionMinimized { get; set; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000A3D RID: 2621 RVA: 0x00033B20 File Offset: 0x00031D20
		// (set) Token: 0x06000A3E RID: 2622 RVA: 0x00033B28 File Offset: 0x00031D28
		public bool ShowDetailCardService
		{
			get
			{
				return this._showDetailCardService;
			}
			set
			{
				this._showDetailCardService = value;
				this.upgradeButton.gameObject.SetActive(value);
				this.removeButton.gameObject.SetActive(value);
			}
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x00033B54 File Offset: 0x00031D54
		public void Awake()
		{
			foreach (ValueTuple<int, ShopCard> valueTuple in this.shopCardList.WithIndices<ShopCard>())
			{
				int item = valueTuple.Item1;
				ShopCard item2 = valueTuple.Item2;
				item2.ShopPanel = this;
				item2.Index = item;
			}
			foreach (ValueTuple<int, ShopExhibit> valueTuple2 in this.shopExhibitList.WithIndices<ShopExhibit>())
			{
				int item3 = valueTuple2.Item1;
				ShopExhibit item4 = valueTuple2.Item2;
				item4.ShopPanel = this;
				item4.Index = item3;
			}
			this.returnButton.onClick.AddListener(new UnityAction(this.OnReturnButtonClicked));
			this.cardServiceButton.onClick.AddListener(new UnityAction(this.OnCardServiceClicked));
			this.upgradeButton.onClick.AddListener(new UnityAction(this.OnUpgradeClicked));
			this.removeButton.onClick.AddListener(new UnityAction(this.OnRemoveClicked));
			SimpleTooltipSource.CreateWithGeneralKeyAndArgs(this.upgradeButton.gameObject, "Shop.Upgrade", "Shop.UpgradeTooltip", new object[] { 50 }).WithPosition(TooltipDirection.Left, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKeyAndArgs(this.removeButton.gameObject, "Shop.Remove", "Shop.RemoveTooltip", new object[] { 25 }).WithPosition(TooltipDirection.Left, TooltipAlignment.Max);
			this.Close();
		}

		// Token: 0x06000A40 RID: 2624 RVA: 0x00033CE4 File Offset: 0x00031EE4
		public override void OnLocaleChanged()
		{
			this._welcomeQuotes = "Shop.WelcomeQuotes".LocalizeStrings(true);
			this._boughtQuotes = "Shop.BoughtQuotes".LocalizeStrings(true);
			this._cantAffordQuotes = "Shop.CantAffordQuotes".LocalizeStrings(true);
			this.SetCardServicePrice();
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x00033D20 File Offset: 0x00031F20
		private void SetCardServicePrice()
		{
			if (base.GameRun == null)
			{
				return;
			}
			this.upgradePrice.text = string.Format("Shop.ShopUpgradePrice".Localize(true), base.GameRun.UpgradeDeckCardPrice);
			this.removePrice.text = string.Format("Shop.ShopRemovePrice".Localize(true), base.GameRun.RemoveDeckCardPrice);
			bool flag = base.GameRun.Money >= base.GameRun.UpgradeDeckCardPrice;
			this.upgradePrice.color = (flag ? ShopPanel.EnoughColor : ShopPanel.NotEnoughColor);
			flag = base.GameRun.Money >= base.GameRun.RemoveDeckCardPrice;
			this.removePrice.color = (flag ? ShopPanel.EnoughColor : ShopPanel.NotEnoughColor);
		}

		// Token: 0x06000A42 RID: 2626 RVA: 0x00033DF8 File Offset: 0x00031FF8
		protected override void OnShowing(ShopStation shopStation)
		{
			this.ShopStation = shopStation;
			this._quotedSomething = false;
			this.SetShop();
			this._interactable = false;
			GameMaster.ShowPoseAnimation = false;
		}

		// Token: 0x06000A43 RID: 2627 RVA: 0x00033E1B File Offset: 0x0003201B
		protected override void OnShown()
		{
			UiManager.GetPanel<VnPanel>().SetNextButton(false, new int?(0), null);
			base.StartCoroutine(this.CoShowAnimation());
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x00033E3C File Offset: 0x0003203C
		private IEnumerator CoShowAnimation()
		{
			Transform transform = this.box.transform;
			this.box.gameObject.SetActive(true);
			this.cover.DOColor(Color.white, 0.3f).From(Color.black, true, false).SetEase(Ease.InSine)
				.SetAutoKill(true);
			this.box.DOColor(Color.white, 0.3f).From(Color.black, true, false).SetEase(Ease.InSine)
				.SetAutoKill(true);
			transform.DOScale(Vector3.one, 0.4f).From(new Vector3(1.4f, 1.4f, 1f), true, false).SetEase(Ease.InQuad)
				.SetAutoKill(true);
			transform.DOLocalMoveY(0f, 0.4f, false).From(200f, true, false).SetEase(Ease.InSine)
				.SetAutoKill(true);
			yield return new WaitForSecondsRealtime(0.4f);
			AudioManager.PlayUi("WoodBoxDrop", false);
			this.effect.gameObject.SetActive(true);
			this.effect.transform.DOScale(new Vector3(10f, 10f), 0.2f).From(new Vector3(6f, 6f), true, false).OnComplete(delegate
			{
				this.effect.gameObject.SetActive(false);
			})
				.SetAutoKill(true);
			this.effect.DOFade(0f, 0.2f).From(1f, true, false).SetAutoKill(true);
			this.root.DOShakePosition(0.2f, 200f, 20, 90f, false, true, ShakeRandomnessMode.Full).SetAutoKill(true);
			yield return new WaitForSecondsRealtime(0.2f);
			this.shopBoard.SetActive(true);
			yield return this.cover.transform.DOLocalMoveX(3840f, 0.4f, false).WaitForCompletion();
			this.cover.gameObject.SetActive(false);
			if (GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("XCost"))
			{
				ShopCard shopCard = Enumerable.FirstOrDefault<ShopCard>(this.shopCardList, (ShopCard w) => w.Card.IsXCost);
				if (shopCard != null)
				{
					yield return UiManager.GetPanel<HintPanel>().ShowAsync(new HintPayload
					{
						HintKey = "XCost",
						Target = shopCard.CardRectTransform,
						CopyedGameObject = shopCard.CloneCardWidget(null).RectTransform
					});
				}
			}
			if (GameMaster.ShowDetailedHint && GameMaster.ShouldShowHint("AbilityCard"))
			{
				ShopCard shopCard2 = Enumerable.FirstOrDefault<ShopCard>(this.shopCardList, (ShopCard w) => w.Card.CardType == CardType.Ability);
				if (shopCard2 != null)
				{
					yield return UiManager.GetPanel<HintPanel>().ShowAsync(new HintPayload
					{
						HintKey = "AbilityCard",
						Target = shopCard2.CardRectTransform,
						CopyedGameObject = shopCard2.CloneCardWidget(null).RectTransform
					});
				}
			}
			if (GameMaster.ShowDetailedHint && GameMaster.ShouldShowHint("FriendCard"))
			{
				ShopCard shopCard3 = Enumerable.FirstOrDefault<ShopCard>(this.shopCardList, (ShopCard w) => w.Card.CardType == CardType.Friend);
				if (shopCard3 != null)
				{
					yield return UiManager.GetPanel<HintPanel>().ShowAsync(new HintPayload
					{
						HintKey = "FriendCard",
						Target = shopCard3.CardRectTransform,
						CopyedGameObject = shopCard3.CloneCardWidget(null).RectTransform
					});
				}
			}
			if (GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("ToolCard"))
			{
				ShopCard shopCard4 = Enumerable.FirstOrDefault<ShopCard>(this.shopCardList, (ShopCard w) => w.Card.CardType == CardType.Tool);
				if (shopCard4 != null)
				{
					yield return UiManager.GetPanel<HintPanel>().ShowAsync(new HintPayload
					{
						HintKey = "ToolCard",
						Target = shopCard4.CardRectTransform,
						CopyedGameObject = shopCard4.CloneCardWidget(null).RectTransform
					});
				}
			}
			this._interactable = true;
			yield return new WaitForSecondsRealtime(2f);
			if (!this._quotedSomething)
			{
				this.QuoteWelcome();
			}
			yield break;
		}

		// Token: 0x06000A45 RID: 2629 RVA: 0x00033E4B File Offset: 0x0003204B
		protected override void OnHiding()
		{
			this.HideQuote();
		}

		// Token: 0x06000A46 RID: 2630 RVA: 0x00033E54 File Offset: 0x00032054
		protected override void OnHided()
		{
			this.Clear();
			this.Close();
			this.ShopStation = null;
			UiManager.GetPanel<VnPanel>().SetNextButton(true, default(int?), null);
			if (this._bufferedCardWidget)
			{
				Object.Destroy(this._bufferedCardWidget.gameObject);
			}
			GameMaster.ShowPoseAnimation = true;
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x00033EAC File Offset: 0x000320AC
		private void Close()
		{
			this.cover.transform.localPosition = Vector3.zero;
			this.box.gameObject.SetActive(false);
			this.shopBoard.SetActive(false);
			this.cover.gameObject.SetActive(true);
			this.effect.gameObject.SetActive(false);
			this.talkRoot.gameObject.SetActive(false);
		}

		// Token: 0x06000A48 RID: 2632 RVA: 0x00033F20 File Offset: 0x00032120
		public void SetShopAfterBuying()
		{
			int money = this.ShopStation.GameRun.Money;
			int num = Math.Min(this.ShopStation.ShopCards.Count, ShopPanel.MaxCardCount);
			int discountCardNo = this.ShopStation.DiscountCardNo;
			for (int i = 0; i < num; i++)
			{
				ShopItem<Card> shopItem = this.ShopStation.ShopCards[i];
				if (shopItem != null && !shopItem.IsSoldOut)
				{
					if (shopItem.Content != this.shopCardList[i].Card)
					{
						this.shopCardList[i].SetCard(shopItem.Content, shopItem.Price, money >= shopItem.Price, shopItem.IsDiscounted);
					}
					else
					{
						this.shopCardList[i].SetPrice(shopItem.Price, money >= shopItem.Price, shopItem.IsDiscounted);
					}
				}
			}
			int num2 = Math.Min(this.ShopStation.ShopExhibits.Count, 3);
			for (int j = 0; j < num2; j++)
			{
				ShopItem<Exhibit> shopItem2 = this.ShopStation.ShopExhibits[j];
				if (shopItem2 != null && !shopItem2.IsSoldOut)
				{
					if (shopItem2.Content != this.shopExhibitList[j].Exhibit)
					{
						this.shopExhibitList[j].SetExhibit(shopItem2.Content, shopItem2.Price, money >= shopItem2.Price);
					}
					else
					{
						this.shopExhibitList[j].SetPrice(shopItem2.Price, money >= shopItem2.Price);
					}
				}
			}
			this.SetCardService();
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x000340DC File Offset: 0x000322DC
		private void SetShop()
		{
			int money = this.ShopStation.GameRun.Money;
			if (this.ShopStation.ShopCards.Count > ShopPanel.MaxCardCount)
			{
				Debug.LogWarning("there's too many shopCards to show.");
			}
			for (int i = 0; i < ShopPanel.MaxCardCount; i++)
			{
				ShopItem<Card> shopItem = this.ShopStation.ShopCards[i];
				if (shopItem != null)
				{
					if (shopItem.IsSoldOut)
					{
						this.shopCardList[i].ViewBought();
					}
					else
					{
						this.shopCardList[i].SetCard(shopItem.Content, shopItem.Price, money >= shopItem.Price, shopItem.IsDiscounted);
					}
				}
				else
				{
					this.shopCardList[i].ViewBought();
				}
			}
			if (this.ShopStation.ShopExhibits.Count > ShopPanel.MaxExhibitCount)
			{
				Debug.LogWarning("there's too many shopExhibits to show.");
			}
			for (int j = 0; j < ShopPanel.MaxExhibitCount; j++)
			{
				ShopItem<Exhibit> shopItem2 = this.ShopStation.ShopExhibits[j];
				if (shopItem2 != null)
				{
					if (shopItem2.Content != this.shopExhibitList[j].Exhibit)
					{
						this.shopExhibitList[j].SetExhibit(shopItem2.Content, shopItem2.Price, money >= shopItem2.Price);
					}
					else if (shopItem2.IsSoldOut)
					{
						this.shopExhibitList[j].Close();
					}
					else
					{
						this.shopExhibitList[j].SetExhibit(shopItem2.Content, shopItem2.Price, money >= shopItem2.Price);
					}
				}
				else
				{
					this.shopExhibitList[j].Close();
				}
			}
			this.ShowDetailCardService = false;
			this.SetCardService();
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x000342A0 File Offset: 0x000324A0
		private void SetCardService()
		{
			if (this.ShopStation.CanUseCardService)
			{
				this.cardServiceRoot.SetActive(true);
				this.soldOutRoot.SetActive(false);
				this.SetCardServicePrice();
				return;
			}
			this.cardServiceRoot.SetActive(false);
			this.soldOutRoot.SetActive(true);
		}

		// Token: 0x06000A4B RID: 2635 RVA: 0x000342F4 File Offset: 0x000324F4
		private void Clear()
		{
			foreach (ShopCard shopCard in this.shopCardList)
			{
				shopCard.ViewBought();
			}
			foreach (ShopExhibit shopExhibit in this.shopExhibitList)
			{
				shopExhibit.Close();
			}
		}

		// Token: 0x06000A4C RID: 2636 RVA: 0x00034384 File Offset: 0x00032584
		public void BuyCard(int index)
		{
			if (!this._interactable || this.LockedByInteractionMinimized)
			{
				return;
			}
			int money = this.ShopStation.GameRun.Money;
			ShopItem<Card> shopItem = this.ShopStation.ShopCards[index];
			if (shopItem != null)
			{
				ShopCard shopCard = this.shopCardList[index];
				this._bufferedCardWidget = shopCard.CloneCardWidget(this.root);
				this.ShopStation.BuyCard(shopItem);
				if (this._bufferedCardWidget)
				{
					Debug.LogWarning("[ShopPanel] buffered card for " + shopItem.Content.DebugName + " is not taken");
					Object.Destroy(this._bufferedCardWidget.gameObject);
					this._bufferedCardWidget = null;
				}
				this.QuoteBought();
				ShopItem<Card> shopItem2 = this.ShopStation.ShopCards[index];
				if (shopItem != shopItem2)
				{
					shopCard.SetCard(shopItem2.Content, shopItem2.Price, money >= shopItem2.Price, false);
				}
				else
				{
					shopCard.ViewBought();
				}
				this.SetShopAfterBuying();
			}
		}

		// Token: 0x06000A4D RID: 2637 RVA: 0x00034482 File Offset: 0x00032682
		public Vector3 GetExhibitPosition(int index)
		{
			return this.shopExhibitList[index].transform.position;
		}

		// Token: 0x06000A4E RID: 2638 RVA: 0x0003449C File Offset: 0x0003269C
		public void BuyExhibit(int index)
		{
			if (!this._interactable || this.LockedByInteractionMinimized)
			{
				return;
			}
			ShopItem<Exhibit> shopItem = this.ShopStation.ShopExhibits[index];
			if (shopItem != null)
			{
				base.StartCoroutine(this.CoBuyExhibit(shopItem, index));
			}
		}

		// Token: 0x06000A4F RID: 2639 RVA: 0x000344DE File Offset: 0x000326DE
		private IEnumerator CoBuyExhibit(ShopItem<Exhibit> exhibitItem, int index)
		{
			yield return this.ShopStation.BuyExhibitRunner(exhibitItem);
			this.QuoteBought();
			ShopItem<Exhibit> shopItem = this.ShopStation.ShopExhibits[index];
			if (exhibitItem == shopItem)
			{
				this.shopExhibitList[index].Close();
			}
			this.SetShopAfterBuying();
			yield break;
		}

		// Token: 0x06000A50 RID: 2640 RVA: 0x000344FB File Offset: 0x000326FB
		private void OnReturnButtonClicked()
		{
			if (!this._interactable || this.LockedByInteractionMinimized)
			{
				return;
			}
			AudioManager.PlayUi("WoodClick", false);
			base.Hide();
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x00034520 File Offset: 0x00032720
		private void OnCardServiceClicked()
		{
			this.ShowDetailCardService = !this.ShowDetailCardService;
			AudioManager.Button(this.ShowDetailCardService ? 0 : 1);
			GameObject gameObject = GameObject.Find("EventSystem");
			if (gameObject != null)
			{
				gameObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
			}
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x00034570 File Offset: 0x00032770
		private void OnUpgradeClicked()
		{
			if (this.LockedByInteractionMinimized)
			{
				return;
			}
			if (base.GameRun.Money >= this.ShopStation.UpgradeDeckCardPrice)
			{
				base.StartCoroutine(this.CoUpgradeCard());
				AudioManager.Button(0);
				return;
			}
			this.QuoteCantAfford();
			AudioManager.PlayUi("NoMoney", false);
		}

		// Token: 0x06000A53 RID: 2643 RVA: 0x000345C4 File Offset: 0x000327C4
		private void OnRemoveClicked()
		{
			if (this.LockedByInteractionMinimized)
			{
				return;
			}
			if (base.GameRun.Money >= this.ShopStation.RemoveDeckCardPrice)
			{
				base.StartCoroutine(this.CoRemoveCard());
				AudioManager.Button(0);
				return;
			}
			this.QuoteCantAfford();
			AudioManager.PlayUi("NoMoney", false);
		}

		// Token: 0x06000A54 RID: 2644 RVA: 0x00034617 File Offset: 0x00032817
		public IEnumerator CoUpgradeCard()
		{
			ShowCardsPanel panel = UiManager.GetPanel<ShowCardsPanel>();
			ShowCardsPanel showCardsPanel = panel;
			ShowCardsPayload showCardsPayload = new ShowCardsPayload();
			showCardsPayload.Name = "Game.Deck".Localize(true);
			showCardsPayload.Description = "Cards.UpgradeTips".Localize(true);
			showCardsPayload.Cards = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeck, (Card c) => c.CanUpgrade));
			showCardsPayload.CanCancel = true;
			showCardsPayload.InteractionType = InteractionType.Upgrade;
			yield return showCardsPanel.ShowAsync(showCardsPayload);
			if (!panel.IsCanceled)
			{
				Card selectedCard = panel.SelectedCard;
				this.ShopStation.UpgradeDeckCard(selectedCard);
				this.QuoteBought();
				AudioManager.PlayUi("Bought", false);
				this.SetShopAfterBuying();
			}
			yield break;
		}

		// Token: 0x06000A55 RID: 2645 RVA: 0x00034626 File Offset: 0x00032826
		public IEnumerator CoRemoveCard()
		{
			ShowCardsPanel panel = UiManager.GetPanel<ShowCardsPanel>();
			yield return panel.ShowAsync(new ShowCardsPayload
			{
				Name = "Game.Deck".Localize(true),
				Description = "Cards.RemoveTips".Localize(true),
				Cards = Enumerable.ToList<Card>(base.GameRun.BaseDeckWithoutUnremovable),
				CanCancel = true,
				InteractionType = InteractionType.Remove
			});
			if (!panel.IsCanceled)
			{
				Card selectedCard = panel.SelectedCard;
				this.ShopStation.RemoveDeckCard(selectedCard);
				this.QuoteBought();
				AudioManager.PlayUi("Bought", false);
				this.SetShopAfterBuying();
			}
			yield break;
		}

		// Token: 0x06000A56 RID: 2646 RVA: 0x00034635 File Offset: 0x00032835
		public CardWidget ExtractBufferedCard()
		{
			if (!this._bufferedCardWidget)
			{
				Debug.LogWarning("[RewardPanel] Cannot extract buffered card: no buffered card");
				return null;
			}
			CardWidget bufferedCardWidget = this._bufferedCardWidget;
			this._bufferedCardWidget = null;
			bufferedCardWidget.HideTooltip();
			return bufferedCardWidget;
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x00034664 File Offset: 0x00032864
		private void QuoteWelcome()
		{
			int num = Random.Range(0, this._welcomeQuotes.Count);
			this.talkText.text = this._welcomeQuotes[num];
			this.ShowQuote();
		}

		// Token: 0x06000A58 RID: 2648 RVA: 0x000346A0 File Offset: 0x000328A0
		private void QuoteBought()
		{
			int num = Random.Range(0, this._boughtQuotes.Count);
			this.talkText.text = this._boughtQuotes[num];
			this.ShowQuote();
		}

		// Token: 0x06000A59 RID: 2649 RVA: 0x000346DC File Offset: 0x000328DC
		public void QuoteCantAfford()
		{
			int num = Random.Range(0, this._cantAffordQuotes.Count);
			this.talkText.text = this._cantAffordQuotes[num];
			this.ShowQuote();
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x00034718 File Offset: 0x00032918
		private void ShowQuote()
		{
			if (this._talkShowing)
			{
				this.HideQuote();
				base.StartCoroutine("ShowQuoteRunner");
				return;
			}
			base.StartCoroutine("ShowQuoteRunner");
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x00034741 File Offset: 0x00032941
		private IEnumerator ShowQuoteRunner()
		{
			this._talkShowing = true;
			this._quotedSomething = true;
			this.talkRoot.localPosition = new Vector3((float)Random.Range(-1000, 1000), 1080f);
			this.talkActor.localPosition = Vector3.zero;
			this.talkRoot.gameObject.SetActive(true);
			this.talkImage.DOFade(1f, 0.2f).From(0f, true, false).SetAutoKill(true);
			this.talkText.DOFade(1f, 0.2f).From(0f, true, false).SetAutoKill(true);
			Sequence sequence = DOTween.Sequence();
			sequence.Append(this.talkActor.DOLocalMoveY(-10f, 0.5f, false).SetEase(Ease.OutSine).From(0f, true, false));
			sequence.Append(this.talkActor.DOLocalMoveY(0f, 0.5f, false).SetEase(Ease.InSine).From(-10f, true, false));
			sequence.Append(this.talkActor.DOLocalMoveY(10f, 0.5f, false).SetEase(Ease.OutSine).From(0f, true, false));
			sequence.Append(this.talkActor.DOLocalMoveY(0f, 0.5f, false).SetEase(Ease.InSine).From(10f, true, false)).SetAutoKill(true);
			yield return sequence.SetUpdate(true).WaitForCompletion();
			this.talkImage.DOFade(0f, 0.2f).From(1f, true, false).SetAutoKill(true);
			this.talkText.DOFade(0f, 0.2f).From(1f, true, false).OnComplete(new TweenCallback(this.HideQuote))
				.SetAutoKill(true);
			yield break;
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x00034750 File Offset: 0x00032950
		private void HideQuote()
		{
			base.StopCoroutine("ShowQuoteRunner");
			this.talkActor.DOKill(false);
			this.talkImage.DOKill(false);
			this.talkText.DOKill(false);
			this._talkShowing = false;
		}

		// Token: 0x06000A5D RID: 2653 RVA: 0x0003478B File Offset: 0x0003298B
		public void UI_EnterTalk()
		{
			this.HideQuote();
		}

		// Token: 0x040007BB RID: 1979
		[SerializeField]
		private RectTransform root;

		// Token: 0x040007BC RID: 1980
		[SerializeField]
		private Image box;

		// Token: 0x040007BD RID: 1981
		[SerializeField]
		private GameObject shopBoard;

		// Token: 0x040007BE RID: 1982
		[SerializeField]
		private Image cover;

		// Token: 0x040007BF RID: 1983
		[SerializeField]
		private Image effect;

		// Token: 0x040007C0 RID: 1984
		[SerializeField]
		private List<ShopCard> shopCardList;

		// Token: 0x040007C1 RID: 1985
		[SerializeField]
		private List<ShopExhibit> shopExhibitList;

		// Token: 0x040007C2 RID: 1986
		[SerializeField]
		private Button returnButton;

		// Token: 0x040007C3 RID: 1987
		[SerializeField]
		private GameObject cardServiceRoot;

		// Token: 0x040007C4 RID: 1988
		[SerializeField]
		private GameObject soldOutRoot;

		// Token: 0x040007C5 RID: 1989
		[SerializeField]
		private Button cardServiceButton;

		// Token: 0x040007C6 RID: 1990
		[SerializeField]
		private Button upgradeButton;

		// Token: 0x040007C7 RID: 1991
		[SerializeField]
		private Button removeButton;

		// Token: 0x040007C8 RID: 1992
		[SerializeField]
		private TextMeshProUGUI upgradePrice;

		// Token: 0x040007C9 RID: 1993
		[SerializeField]
		private TextMeshProUGUI removePrice;

		// Token: 0x040007CA RID: 1994
		[SerializeField]
		private RectTransform talkRoot;

		// Token: 0x040007CB RID: 1995
		[SerializeField]
		private Image talkImage;

		// Token: 0x040007CC RID: 1996
		[SerializeField]
		private RectTransform talkActor;

		// Token: 0x040007CD RID: 1997
		[SerializeField]
		private TextMeshProUGUI talkText;

		// Token: 0x040007CE RID: 1998
		private bool _quotedSomething;

		// Token: 0x040007CF RID: 1999
		public static readonly Color EnoughColor = Color.white;

		// Token: 0x040007D0 RID: 2000
		public static readonly Color NotEnoughColor = new Color(1f, 0.5f, 0.5f);

		// Token: 0x040007D1 RID: 2001
		public static readonly Color Discount = Color.green;

		// Token: 0x040007D2 RID: 2002
		private IList<string> _boughtQuotes;

		// Token: 0x040007D3 RID: 2003
		private IList<string> _cantAffordQuotes;

		// Token: 0x040007D4 RID: 2004
		private IList<string> _welcomeQuotes;

		// Token: 0x040007D7 RID: 2007
		private CardWidget _bufferedCardWidget;

		// Token: 0x040007D8 RID: 2008
		private bool _interactable;

		// Token: 0x040007D9 RID: 2009
		private bool _showDetailCardService;

		// Token: 0x040007DA RID: 2010
		private const float ShakeS = 200f;

		// Token: 0x040007DB RID: 2011
		private const int ShakeV = 20;

		// Token: 0x040007DC RID: 2012
		private const float StartScale = 1.4f;

		// Token: 0x040007DD RID: 2013
		private const float DownTime = 0.4f;

		// Token: 0x040007DE RID: 2014
		private const float VibrationTime = 0.2f;

		// Token: 0x040007DF RID: 2015
		private const float CoverTime = 0.4f;

		// Token: 0x040007E0 RID: 2016
		private const float TalkAmp = 10f;

		// Token: 0x040007E1 RID: 2017
		private const float TalkQuarterTime = 0.5f;

		// Token: 0x040007E2 RID: 2018
		private const float FadeTime = 0.2f;

		// Token: 0x040007E3 RID: 2019
		private bool _talkShowing;
	}
}
