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
	public class ShopPanel : UiPanel<ShopStation>
	{
		private static int MaxCardCount
		{
			get
			{
				return 10;
			}
		}
		private static int MaxExhibitCount
		{
			get
			{
				return 3;
			}
		}
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
		private ShopStation ShopStation { get; set; }
		public bool LockedByInteractionMinimized { get; set; }
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
		public override void OnLocaleChanged()
		{
			this._welcomeQuotes = "Shop.WelcomeQuotes".LocalizeStrings(true);
			this._boughtQuotes = "Shop.BoughtQuotes".LocalizeStrings(true);
			this._cantAffordQuotes = "Shop.CantAffordQuotes".LocalizeStrings(true);
			this.SetCardServicePrice();
		}
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
		protected override void OnShowing(ShopStation shopStation)
		{
			this.ShopStation = shopStation;
			this._quotedSomething = false;
			this.SetShop();
			this._interactable = false;
			GameMaster.ShowPoseAnimation = false;
		}
		protected override void OnShown()
		{
			UiManager.GetPanel<VnPanel>().SetNextButton(false, new int?(0), null);
			base.StartCoroutine(this.CoShowAnimation());
		}
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
		protected override void OnHiding()
		{
			this.HideQuote();
		}
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
		private void Close()
		{
			this.cover.transform.localPosition = Vector3.zero;
			this.box.gameObject.SetActive(false);
			this.shopBoard.SetActive(false);
			this.cover.gameObject.SetActive(true);
			this.effect.gameObject.SetActive(false);
			this.talkRoot.gameObject.SetActive(false);
		}
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
		public Vector3 GetExhibitPosition(int index)
		{
			return this.shopExhibitList[index].transform.position;
		}
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
		private void OnReturnButtonClicked()
		{
			if (!this._interactable || this.LockedByInteractionMinimized)
			{
				return;
			}
			AudioManager.PlayUi("WoodClick", false);
			base.Hide();
		}
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
		private void QuoteWelcome()
		{
			int num = Random.Range(0, this._welcomeQuotes.Count);
			this.talkText.text = this._welcomeQuotes[num];
			this.ShowQuote();
		}
		private void QuoteBought()
		{
			int num = Random.Range(0, this._boughtQuotes.Count);
			this.talkText.text = this._boughtQuotes[num];
			this.ShowQuote();
		}
		public void QuoteCantAfford()
		{
			int num = Random.Range(0, this._cantAffordQuotes.Count);
			this.talkText.text = this._cantAffordQuotes[num];
			this.ShowQuote();
		}
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
		private void HideQuote()
		{
			base.StopCoroutine("ShowQuoteRunner");
			this.talkActor.DOKill(false);
			this.talkImage.DOKill(false);
			this.talkText.DOKill(false);
			this._talkShowing = false;
		}
		public void UI_EnterTalk()
		{
			this.HideQuote();
		}
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private Image box;
		[SerializeField]
		private GameObject shopBoard;
		[SerializeField]
		private Image cover;
		[SerializeField]
		private Image effect;
		[SerializeField]
		private List<ShopCard> shopCardList;
		[SerializeField]
		private List<ShopExhibit> shopExhibitList;
		[SerializeField]
		private Button returnButton;
		[SerializeField]
		private GameObject cardServiceRoot;
		[SerializeField]
		private GameObject soldOutRoot;
		[SerializeField]
		private Button cardServiceButton;
		[SerializeField]
		private Button upgradeButton;
		[SerializeField]
		private Button removeButton;
		[SerializeField]
		private TextMeshProUGUI upgradePrice;
		[SerializeField]
		private TextMeshProUGUI removePrice;
		[SerializeField]
		private RectTransform talkRoot;
		[SerializeField]
		private Image talkImage;
		[SerializeField]
		private RectTransform talkActor;
		[SerializeField]
		private TextMeshProUGUI talkText;
		private bool _quotedSomething;
		public static readonly Color EnoughColor = Color.white;
		public static readonly Color NotEnoughColor = new Color(1f, 0.5f, 0.5f);
		public static readonly Color Discount = Color.green;
		private IList<string> _boughtQuotes;
		private IList<string> _cantAffordQuotes;
		private IList<string> _welcomeQuotes;
		private CardWidget _bufferedCardWidget;
		private bool _interactable;
		private bool _showDetailCardService;
		private const float ShakeS = 200f;
		private const int ShakeV = 20;
		private const float StartScale = 1.4f;
		private const float DownTime = 0.4f;
		private const float VibrationTime = 0.2f;
		private const float CoverTime = 0.4f;
		private const float TalkAmp = 10f;
		private const float TalkQuarterTime = 0.5f;
		private const float FadeTime = 0.2f;
		private bool _talkShowing;
	}
}
