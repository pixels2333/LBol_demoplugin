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
	// Token: 0x0200009A RID: 154
	public class GapOptionsPanel : UiPanel<GapStation>
	{
		// Token: 0x17000156 RID: 342
		// (get) Token: 0x060007FE RID: 2046 RVA: 0x00025E38 File Offset: 0x00024038
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x00025E3C File Offset: 0x0002403C
		public override void OnLocaleChanged()
		{
			this._headerInitial = "Gap.HeaderInitial".Localize(true);
			this._headers = "Gap.Headers".LocalizeStrings(true);
			foreach (GapOptionWidget gapOptionWidget in this._options)
			{
				gapOptionWidget.OnLocalizeChanged();
			}
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x00025EB0 File Offset: 0x000240B0
		public void Awake()
		{
			this.info.alpha = 0f;
			this._headerPos = this.header.transform.localPosition;
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x00025ED8 File Offset: 0x000240D8
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

		// Token: 0x06000802 RID: 2050 RVA: 0x000260E0 File Offset: 0x000242E0
		protected override void OnHided()
		{
			this._gapStation = null;
			this.optionsLayout.DestroyChildren();
			this._options.Clear();
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x000260FF File Offset: 0x000242FF
		public IEnumerator WaitUntilOptionSelected()
		{
			this._selected = false;
			return new WaitUntil(() => this._selected);
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00026119 File Offset: 0x00024319
		public void SelectedAndHide()
		{
			this._selected = true;
			base.Hide();
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x00026128 File Offset: 0x00024328
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

		// Token: 0x06000806 RID: 2054 RVA: 0x00026194 File Offset: 0x00024394
		public void EndHoverOption()
		{
			this.info.DOFade(0f, 0.1f);
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x000261AC File Offset: 0x000243AC
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

		// Token: 0x06000808 RID: 2056 RVA: 0x00026241 File Offset: 0x00024441
		private void DrinkTea(GapOption option)
		{
			this._gapStation.DrinkTea((DrinkTea)option);
			this.SelectedAndHide();
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x0002625A File Offset: 0x0002445A
		private void UpgradeCard(GapOption option)
		{
			base.StartCoroutine(this.UpgradeCardRunner((UpgradeCard)option));
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x0002626F File Offset: 0x0002446F
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

		// Token: 0x0600080B RID: 2059 RVA: 0x00026285 File Offset: 0x00024485
		private void FindExhibit()
		{
			base.StartCoroutine(this.CoFindExhibit());
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00026294 File Offset: 0x00024494
		private IEnumerator CoFindExhibit()
		{
			this._optionActive = false;
			yield return this._gapStation.FindExhibitRunner();
			this.SelectedAndHide();
			yield break;
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x000262A4 File Offset: 0x000244A4
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

		// Token: 0x0600080E RID: 2062 RVA: 0x000262F3 File Offset: 0x000244F3
		private void RemoveCard()
		{
			base.StartCoroutine(this.RemoveCardRunner());
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x00026302 File Offset: 0x00024502
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

		// Token: 0x06000810 RID: 2064 RVA: 0x00026311 File Offset: 0x00024511
		private void InternalGetRareCard(GapOption option)
		{
			base.StartCoroutine(this.GetRareCardRunner(option));
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x00026321 File Offset: 0x00024521
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

		// Token: 0x06000812 RID: 2066 RVA: 0x00026338 File Offset: 0x00024538
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

		// Token: 0x06000813 RID: 2067 RVA: 0x0002637C File Offset: 0x0002457C
		public Vector3 GetOptionPosition(int index)
		{
			GapOptionWidget gapOptionWidget = this._options.TryGetValue(index);
			if (gapOptionWidget == null)
			{
				return base.transform.TransformPoint(Vector3.zero);
			}
			return gapOptionWidget.transform.position;
		}

		// Token: 0x04000590 RID: 1424
		[SerializeField]
		private TextMeshProUGUI header;

		// Token: 0x04000591 RID: 1425
		[SerializeField]
		private TextMeshProUGUI info;

		// Token: 0x04000592 RID: 1426
		[SerializeField]
		private GapOptionWidget template;

		// Token: 0x04000593 RID: 1427
		[SerializeField]
		private Transform optionsLayout;

		// Token: 0x04000594 RID: 1428
		[SerializeField]
		private AssociationList<GapOptionType, Sprite> spriteTable;

		// Token: 0x04000595 RID: 1429
		[SerializeField]
		private Vector3 defaultOptionPos;

		// Token: 0x04000596 RID: 1430
		[SerializeField]
		private Vector3 optionPadding;

		// Token: 0x04000597 RID: 1431
		private bool _headerChanged;

		// Token: 0x04000598 RID: 1432
		private Vector3 _headerPos;

		// Token: 0x04000599 RID: 1433
		private GapStation _gapStation;

		// Token: 0x0400059A RID: 1434
		private string _headerInitial;

		// Token: 0x0400059B RID: 1435
		private IList<string> _headers;

		// Token: 0x0400059C RID: 1436
		private readonly List<GapOptionWidget> _options = new List<GapOptionWidget>();

		// Token: 0x0400059D RID: 1437
		private bool _selected;

		// Token: 0x0400059E RID: 1438
		private bool _optionActive;
	}
}
