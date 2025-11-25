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
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	public class GapOptionsPanel : UiPanel<GapStation>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
		public override void OnLocaleChanged()
		{
			this._headerInitial = "Gap.HeaderInitial".Localize(true);
			this._headers = "Gap.Headers".LocalizeStrings(true);
			foreach (GapOptionWidget gapOptionWidget in this._options)
			{
				gapOptionWidget.OnLocalizeChanged();
			}
		}
		public void Awake()
		{
			this.info.alpha = 0f;
			this._headerPos = this.header.transform.localPosition;
		}
		protected override void OnShowing(GapStation gapStation)
		{
			this._gapStation = gapStation;
			this.header.text = this._headerInitial;
			this._headerChanged = false;
			this._optionActive = true;
			List<GapOption> list = Enumerable.ToList<GapOption>(this._gapStation.GapOptions);
			Vector3 vector = new Vector3(this.optionPadding.x + (float)(list.Count * 10), this.optionPadding.y);
			bool flag = list.Count > 4;
			foreach (ValueTuple<int, GapOption> valueTuple in list.WithIndices<GapOption>())
			{
				int item = valueTuple.Item1;
				GapOption item2 = valueTuple.Item2;
				Sprite sprite;
				if (!this.spriteTable.TryGetValue(item2.Type, out sprite))
				{
					Debug.LogError(string.Format("Cannot find sprite for GapOptionType {0}", item2.Type));
				}
				GapOptionWidget gapOptionWidget = Object.Instantiate<GapOptionWidget>(this.template, this.optionsLayout);
				this._options.Add(gapOptionWidget);
				gapOptionWidget.Parent = this;
				gapOptionWidget.SetOption(item2, sprite);
				Vector3 vector2;
				if (flag)
				{
					vector2 = this.defaultOptionPos - new Vector3(500f, 0f, 0f) + vector * (float)(item % 4) + new Vector3(900f, -200f) * (float)(item / 4);
					gapOptionWidget.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
				}
				else
				{
					vector2 = this.defaultOptionPos + vector * (float)item;
				}
				gapOptionWidget.transform.DOLocalMove(vector2, 1f, false).From(vector2 - new Vector3(4000f, 0f, 0f), true, false).SetEase(Ease.OutCubic);
				gapOptionWidget.transform.SetAsFirstSibling();
			}
		}
		protected override void OnHided()
		{
			this._gapStation = null;
			this.optionsLayout.DestroyChildren();
			this._options.Clear();
		}
		public IEnumerator WaitUntilOptionSelected()
		{
			this._selected = false;
			return new WaitUntil(() => this._selected);
		}
		public void SelectedAndHide()
		{
			this._selected = true;
			base.Hide();
		}
		public void StartHoverOption(GapOption option)
		{
			if (!this._headerChanged)
			{
				this._headerChanged = true;
				int num = Random.Range(0, this._headers.Count);
				this.header.text = this._headers[num];
			}
			this.info.text = option.Description;
			this.info.DOFade(1f, 0.1f);
		}
		public void EndHoverOption()
		{
			this.info.DOFade(0f, 0.1f);
		}
		public void OptionClicked(GapOption option)
		{
			if (!this._optionActive)
			{
				return;
			}
			switch (option.Type)
			{
			case GapOptionType.DrinkTea:
				this.DrinkTea(option);
				return;
			case GapOptionType.UpgradeCard:
				this.UpgradeCard(option);
				return;
			case GapOptionType.FindExhibit:
				this.FindExhibit();
				return;
			case GapOptionType.GetMoney:
				this.GetMoney();
				return;
			case GapOptionType.RemoveCard:
				this.RemoveCard();
				return;
			case GapOptionType.GetRareCard:
				this.InternalGetRareCard(option);
				return;
			case GapOptionType.UpgradeBaota:
				this.UpgradeBaota();
				return;
			default:
				Debug.LogWarning("Option " + option.Name + " doesn't work now!");
				this.SelectedAndHide();
				return;
			}
		}
		private void DrinkTea(GapOption option)
		{
			this._gapStation.DrinkTea((DrinkTea)option);
			this.SelectedAndHide();
		}
		private void UpgradeCard(GapOption option)
		{
			base.StartCoroutine(this.UpgradeCardRunner((UpgradeCard)option));
		}
		private IEnumerator UpgradeCardRunner(UpgradeCard upgradeCard)
		{
			ShowCardsPanel upgradeCardPanel = UiManager.GetPanel<ShowCardsPanel>();
			ShowCardsPayload showCardsPayload = new ShowCardsPayload();
			showCardsPayload.Name = "Game.Deck".Localize(true);
			showCardsPayload.Description = "Cards.UpgradeTips".Localize(true);
			showCardsPayload.Cards = Enumerable.ToList<Card>(Enumerable.Where<Card>(this._gapStation.GameRun.BaseDeck, (Card card) => card.CanUpgrade));
			showCardsPayload.CanCancel = true;
			showCardsPayload.InteractionType = InteractionType.Upgrade;
			showCardsPayload.Price = upgradeCard.Price;
			showCardsPayload.Money = base.GameRun.Money;
			showCardsPayload.PayCards = Enumerable.ToList<Card>(Enumerable.Where<Card>(this._gapStation.GameRun.BaseDeck, (Card card) => card.CanUpgrade && !card.IsBasic));
			ShowCardsPayload showCardsPayload2 = showCardsPayload;
			yield return upgradeCardPanel.ShowAsync(showCardsPayload2);
			if (!upgradeCardPanel.IsCanceled)
			{
				Card selectedCard = upgradeCardPanel.SelectedCard;
				this._gapStation.GameRun.UpgradeDeckCards(new Card[] { selectedCard }, false);
				if (upgradeCard.Price > 0 && !selectedCard.IsBasic)
				{
					this._gapStation.GameRun.ConsumeMoney(upgradeCard.Price);
					AudioManager.PlayUi("Bought", false);
				}
				this.SelectedAndHide();
			}
			yield break;
		}
		private void FindExhibit()
		{
			base.StartCoroutine(this.CoFindExhibit());
		}
		private IEnumerator CoFindExhibit()
		{
			this._optionActive = false;
			yield return this._gapStation.FindExhibitRunner();
			this.SelectedAndHide();
			yield break;
		}
		private void GetMoney()
		{
			GetMoney getMoney = Enumerable.FirstOrDefault<GetMoney>(Enumerable.OfType<GetMoney>(this._gapStation.GapOptions));
			if (getMoney != null)
			{
				this._gapStation.GameRun.GainMoney(getMoney.Value, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Gap
				});
			}
			this.SelectedAndHide();
		}
		private void RemoveCard()
		{
			base.StartCoroutine(this.RemoveCardRunner());
		}
		private IEnumerator RemoveCardRunner()
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
				this._gapStation.GameRun.RemoveDeckCard(selectedCard, true);
				this.SelectedAndHide();
				AudioManager.PlayUi("Jingshen", false);
			}
			yield break;
		}
		private void InternalGetRareCard(GapOption option)
		{
			base.StartCoroutine(this.GetRareCardRunner(option));
		}
		private IEnumerator GetRareCardRunner(GapOption option)
		{
			foreach (GapOptionWidget gapOptionWidget in this._options)
			{
				gapOptionWidget.Active = false;
			}
			int value = ((GetRareCard)option).Value;
			Card[] array = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), value, true, false, null);
			base.GameRun.UpgradeNewDeckCardOnFlags(array);
			SelectCardPanel panel = UiManager.GetPanel<SelectCardPanel>();
			yield return panel.ShowMiniSelect(new SelectCardPayload
			{
				Name = option.Name,
				Cards = array,
				Min = 1,
				Max = 1,
				CanSkip = true,
				IsAddCardToDeck = true
			});
			if (!panel.IsCanceled)
			{
				List<Card> selectedCards = panel.SelectedCards;
				Card card = ((selectedCards != null) ? Enumerable.FirstOrDefault<Card>(selectedCards) : null);
				if (card != null)
				{
					base.GameRun.AddDeckCard(card, true, new VisualSourceData
					{
						SourceType = VisualSourceType.CardSelect
					});
					AudioManager.PlayUi("Faxiang", false);
				}
			}
			this.SelectedAndHide();
			yield break;
		}
		private void UpgradeBaota()
		{
			Baota exhibit = base.GameRun.Player.GetExhibit<Baota>();
			if (exhibit != null)
			{
				exhibit.GapOption();
				AudioManager.PlayUi("Baota", false);
			}
			else
			{
				Debug.LogError("[GapOptionPanel] Cannot upgrade Baota without Baota");
			}
			this.SelectedAndHide();
		}
		public Vector3 GetOptionPosition(int index)
		{
			GapOptionWidget gapOptionWidget = this._options.TryGetValue(index);
			if (gapOptionWidget == null)
			{
				return base.transform.TransformPoint(Vector3.zero);
			}
			return gapOptionWidget.transform.position;
		}
		[SerializeField]
		private TextMeshProUGUI header;
		[SerializeField]
		private TextMeshProUGUI info;
		[SerializeField]
		private GapOptionWidget template;
		[SerializeField]
		private Transform optionsLayout;
		[SerializeField]
		private AssociationList<GapOptionType, Sprite> spriteTable;
		[SerializeField]
		private Vector3 defaultOptionPos;
		[SerializeField]
		private Vector3 optionPadding;
		private bool _headerChanged;
		private Vector3 _headerPos;
		private GapStation _gapStation;
		private string _headerInitial;
		private IList<string> _headers;
		private readonly List<GapOptionWidget> _options = new List<GapOptionWidget>();
		private bool _selected;
		private bool _optionActive;
	}
}
