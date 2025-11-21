using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200008C RID: 140
	public sealed class CardDetailPanel : UiPanel<CardDetailPayload>, IInputActionHandler, IScrollHandler, IEventSystemHandler
	{
		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000714 RID: 1812 RVA: 0x0002089E File Offset: 0x0001EA9E
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Topmost;
			}
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000715 RID: 1813 RVA: 0x000208A1 File Offset: 0x0001EAA1
		// (set) Token: 0x06000716 RID: 1814 RVA: 0x000208AC File Offset: 0x0001EAAC
		private CardWidget CenterCardStyle
		{
			get
			{
				return this._centerCardStyle;
			}
			set
			{
				this._centerCardStyle = value;
				this.cardStyleIllustratorText.text = this.GetIllustratorText((this._cardStyleDic[value] == "") ? value.Card.Config.Illustrator : this._cardStyleDic[value]);
			}
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x00020908 File Offset: 0x0001EB08
		public void Awake()
		{
			this.bg.onClick.AddListener(new UnityAction(base.Hide));
			this.tooltipTemplate.gameObject.SetActive(false);
			float num;
			float num2;
			this.tooltipParent.sizeDelta.Deconstruct(out num, out num2);
			this._maxTooltipWidth = num;
			this._maxTooltipHeight = num2;
			using (List<CardWidget>.Enumerator enumerator = this.relativeCardWidgets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CardWidget widget = enumerator.Current;
					widget.GetComponent<ShowingCardRelative>().AddListener(delegate
					{
						AudioManager.Card(3);
						this.ShowRelativeCard(widget);
					});
				}
			}
			this.cardCellTemplate.gameObject.SetActive(false);
			this.upgradeSwitch.AddListener(delegate(bool value)
			{
				this.SetData(value ? this._upgradedCard : this._card, true);
			});
			this.showCardStyleButton.onClick.AddListener(delegate
			{
				this.cardStylePanelRoot.gameObject.SetActive(true);
			});
			this.cardStylePanelBg.onClick.AddListener(delegate
			{
				this.cardStylePanelRoot.gameObject.SetActive(false);
			});
			this.cardStyleConfirmButton.onClick.AddListener(delegate
			{
				string text;
				if (this._cardStyleDic.TryGetValue(this._centerCardStyle, ref text))
				{
					GameMaster.SetPreferredCardIllustrator(this._card.Id, text);
				}
				this.cardStylePanelRoot.gameObject.SetActive(false);
				this.RefreshIllustrator();
			});
			this.cardStylePanelRoot.gameObject.SetActive(false);
			Object.Destroy(this.removeCardButton.gameObject);
			Object.Destroy(this.saveImageButton.gameObject);
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x00020A8C File Offset: 0x0001EC8C
		public override void OnLocaleChanged()
		{
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x00020A90 File Offset: 0x0001EC90
		protected override void OnShowing(CardDetailPayload payload)
		{
			foreach (CardWidget cardWidget in this.relativeCardWidgets)
			{
				cardWidget.transform.localScale = Vector3.one;
			}
			this._sourceCard = payload.Card;
			ValueTuple<Card, Card> detailInfoCard = this._sourceCard.GetDetailInfoCard();
			this._card = detailInfoCard.Item1;
			this._upgradedCard = detailInfoCard.Item2;
			this.SetData(this._sourceCard.IsUpgraded ? this._upgradedCard : this._card, false);
			this.StartCardAnim(payload.Rect, this.cardDetailWidget);
			this._preventRightClickHide = payload.PreventRightClickHide;
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x00020B6C File Offset: 0x0001ED6C
		protected override void OnShown()
		{
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00020B6E File Offset: 0x0001ED6E
		protected override void OnHiding()
		{
			AudioManager.Card(4);
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x00020B88 File Offset: 0x0001ED88
		protected override void OnHided()
		{
			this._sourceCard = null;
			this._card = null;
			this._upgradedCard = null;
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x00020BA0 File Offset: 0x0001EDA0
		private void StartCardAnim(Component from, Component to)
		{
			this.subWidgetGroup.DOKill(false);
			to.transform.position = from.transform.position;
			Vector2 vector = new Vector2(0f, 150f);
			to.transform.localScale = from.transform.localScale;
			to.GetComponent<RectTransform>().DOAnchorPos(vector, this.animDuration, false).SetEase(Ease.OutCubic)
				.SetAutoKill(true)
				.SetUpdate(true);
			to.transform.DOScale(new Vector3(2f, 2f, 1f), this.animDuration).SetEase(Ease.OutCubic).SetAutoKill(true)
				.SetUpdate(true);
			TweenerCore<float, float, FloatOptions> tweenerCore = this.subWidgetGroup.DOFade(1f, 0.4f).From(0f, true, false).SetDelay(this.animDuration)
				.SetAutoKill(true)
				.SetUpdate(true);
			tweenerCore.onPlay = (TweenCallback)Delegate.Combine(tweenerCore.onPlay, delegate
			{
			});
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x00020CC4 File Offset: 0x0001EEC4
		private void SetData(Card card, bool bySwitch)
		{
			if (card == null)
			{
				Debug.LogError("[CardDetailPanel] SetData(null)");
				return;
			}
			this.cardDetailWidget.Card = card;
			this.cardDetailWidget.name = "Card:" + card.Name;
			this.RefreshIllustrator();
			if (this._currentTooltip)
			{
				Object.Destroy(this._currentTooltip.gameObject);
			}
			this._currentTooltip = Object.Instantiate<EntityTooltipWidget>(this.tooltipTemplate, this.tooltipParent);
			this._currentTooltip.gameObject.SetActive(true);
			this._currentTooltip.SetCard(card, true);
			if (this._currentTooltip.RectTransform.sizeDelta.x * this._currentTooltip.RectTransform.localScale.x > this._maxTooltipWidth)
			{
				float num = Mathf.Max(1f, this._maxTooltipWidth / this._currentTooltip.RectTransform.sizeDelta.x);
				this._currentTooltip.RectTransform.localScale = new Vector3(num, num, 1f);
			}
			if (this._currentTooltip.RectTransform.sizeDelta.y * this._currentTooltip.RectTransform.localScale.y > this._maxTooltipHeight)
			{
				float num2 = Mathf.Max(1f, this._maxTooltipHeight / this._currentTooltip.RectTransform.sizeDelta.y);
				this._currentTooltip.RectTransform.localScale = new Vector3(num2, num2, 1f);
			}
			float num3 = this._currentTooltip.RectTransform.sizeDelta.x * this._currentTooltip.RectTransform.localScale.x - this.tooltipTemplate.RectTransform.sizeDelta.x * this.tooltipTemplate.RectTransform.localScale.x;
			if (num3 > 0f)
			{
				this._currentTooltip.RectTransform.localPosition += new Vector3(num3 / 2f, 0f, 0f);
			}
			this.relativeCellLayout.DestroyChildren();
			foreach (CardWidget cardWidget in this.relativeCardWidgets)
			{
				cardWidget.gameObject.SetActive(false);
			}
			List<Card> list = Enumerable.ToList<Card>(card.EnumerateRelativeCards());
			if (list.Count <= this.relativeCardWidgets.Count)
			{
				using (IEnumerator<ValueTuple<int, Card>> enumerator2 = list.WithIndices<Card>().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ValueTuple<int, Card> valueTuple = enumerator2.Current;
						int item = valueTuple.Item1;
						Card item2 = valueTuple.Item2;
						this.relativeCardWidgets[item].gameObject.SetActive(true);
						this.relativeCardWidgets[item].Card = item2;
						this.relativeCardWidgets[item].name = "RelativeCard:" + item2.Name;
					}
					goto IL_03AD;
				}
			}
			if (list.Count > 10)
			{
				list = Enumerable.ToList<Card>(Enumerable.Take<Card>(list, 10));
				Debug.LogWarning(string.Format("{0} has too many relative cards, showing {1} cards only.", card.DebugName, 10));
			}
			foreach (Card card2 in list)
			{
				RecordCardCell recordCardCell = Object.Instantiate<RecordCardCell>(this.cardCellTemplate, this.relativeCellLayout);
				recordCardCell.Card = card2;
				recordCardCell.name = "RelativeCard:" + card2.Name;
				recordCardCell.gameObject.SetActive(true);
			}
			IL_03AD:
			if (!bySwitch)
			{
				this.upgradeSwitch.gameObject.SetActive(card.IsUpgraded || card.CanUpgrade);
				this.upgradeSwitch.SetValueWithoutNotifier(card.IsUpgraded, true);
			}
			this._cardStyleDic.Clear();
			this.cardStyleParent.DestroyChildren();
			CardWidget defaultWidget = Object.Instantiate<CardWidget>(this.cardDetailWidget, this.cardStyleParent);
			defaultWidget.Card = card;
			this._cardStyleDic.Add(defaultWidget, "");
			this.CenterCardStyle = defaultWidget;
			defaultWidget.SetCardIllustrator("");
			GridLayoutGroup component = this.cardStyleParent.GetComponent<GridLayoutGroup>();
			this.cardStyleParent.anchoredPosition = new Vector2(-component.cellSize.x / 2f, 150f);
			defaultWidget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
			{
				this.OnCardStyleClick(defaultWidget);
			});
			defaultWidget.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			foreach (ValueTuple<int, string> valueTuple2 in card.Config.SubIllustrator.WithIndices<string>())
			{
				int item3 = valueTuple2.Item1;
				string item4 = valueTuple2.Item2;
				CardWidget widget = Object.Instantiate<CardWidget>(this.cardDetailWidget, this.cardStyleParent);
				widget.Card = card;
				this._cardStyleDic.Add(widget, item4);
				widget.SetCardIllustrator(item4);
				widget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
				{
					this.OnCardStyleClick(widget);
				});
				widget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			}
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x000212C0 File Offset: 0x0001F4C0
		private void OnCardStyleClick(CardWidget widget)
		{
			GridLayoutGroup component = this.cardStyleParent.GetComponent<GridLayoutGroup>();
			int item = Enumerable.FirstOrDefault<ValueTuple<int, KeyValuePair<CardWidget, string>>>(this._cardStyleDic.WithIndices<KeyValuePair<CardWidget, string>>(), ([TupleElementNames(new string[] { "index", "elem" })] ValueTuple<int, KeyValuePair<CardWidget, string>> pair) => pair.Item2.Key == widget).Item1;
			this.CenterCardStyle = widget;
			this.cardStyleParent.DOAnchorPosX(-component.cellSize.x / 2f - (float)item * (component.cellSize.x + component.spacing.x), 0.2f, false);
			foreach (CardWidget cardWidget in this._cardStyleDic.Keys)
			{
				cardWidget.transform.DOKill(false);
				cardWidget.transform.DOScale(1.2f, 0.2f);
			}
			widget.transform.DOScale(1.5f, 0.2f);
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x000213D4 File Offset: 0x0001F5D4
		private void RefreshIllustrator()
		{
			this.showCardStyleButton.gameObject.SetActive(this._card.Config.SubIllustrator.Count > 0);
			string preferredCardIllustrator = GameMaster.GetPreferredCardIllustrator(this._card);
			if (preferredCardIllustrator != null)
			{
				this.illustratorText.text = this.GetIllustratorText(preferredCardIllustrator);
				this.illustratorText.gameObject.SetActive(true);
				return;
			}
			if (this._card.Config.Illustrator.IsNullOrEmpty())
			{
				this.illustratorText.text = "";
				this.illustratorText.gameObject.SetActive(false);
				return;
			}
			this.illustratorText.text = ((!this._card.Config.Illustrator.IsNullOrEmpty()) ? this.GetIllustratorText(this._card.Config.Illustrator) : "");
			this.illustratorText.gameObject.SetActive(true);
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x000214C5 File Offset: 0x0001F6C5
		private string GetIllustratorText(string id)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CardDetail.Illustrator".Localize(true));
			stringBuilder.Append("<font=\"FZKTK SDF\" material=\"FZKTK SDF Name\">");
			stringBuilder.Append(id);
			return stringBuilder.ToString();
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x000214F8 File Offset: 0x0001F6F8
		public void ShowRelativeCard(RecordCardCell self)
		{
			this.tempCardRoot.DOKill(false);
			CardWidget tempCard = Object.Instantiate<CardWidget>(this.cardDetailWidget, this.tempCardRoot.transform);
			TweenerCore<float, float, FloatOptions> tweenerCore = this.tempCardRoot.DOFade(0f, this.animDuration).SetAutoKill(true).SetUpdate(true)
				.From(1f, true, false);
			tweenerCore.onKill = (TweenCallback)Delegate.Combine(tweenerCore.onKill, delegate
			{
				Object.Destroy(tempCard.gameObject);
			});
			ValueTuple<Card, Card> detailInfoCard = self.Card.GetDetailInfoCard();
			this._card = detailInfoCard.Item1;
			this._upgradedCard = detailInfoCard.Item2;
			this.SetData(self.Card.IsUpgraded ? this._upgradedCard : this._card, false);
			this.StartCardAnim(self.GetComponent<RectTransform>(), this.cardDetailWidget);
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x000215DC File Offset: 0x0001F7DC
		public void ShowRelativeCard(CardWidget self)
		{
			this.tempCardRoot.DOKill(false);
			CardWidget tempCard = Object.Instantiate<CardWidget>(this.cardDetailWidget, this.tempCardRoot.transform);
			TweenerCore<float, float, FloatOptions> tweenerCore = this.tempCardRoot.DOFade(0f, this.animDuration).SetAutoKill(true).SetUpdate(true)
				.From(1f, true, false);
			tweenerCore.onKill = (TweenCallback)Delegate.Combine(tweenerCore.onKill, delegate
			{
				Object.Destroy(tempCard.gameObject);
			});
			ValueTuple<Card, Card> detailInfoCard = self.Card.GetDetailInfoCard();
			this._card = detailInfoCard.Item1;
			this._upgradedCard = detailInfoCard.Item2;
			this.SetData(self.Card.IsUpgraded ? this._upgradedCard : this._card, false);
			this.StartCardAnim(self.GetComponent<RectTransform>(), this.cardDetailWidget);
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x000216BF File Offset: 0x0001F8BF
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x000216C7 File Offset: 0x0001F8C7
		void IInputActionHandler.OnRightClickCancel()
		{
			if (this._preventRightClickHide)
			{
				this._preventRightClickHide = false;
				return;
			}
			base.Hide();
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x000216E0 File Offset: 0x0001F8E0
		public void OnScroll(PointerEventData eventData)
		{
			if (!this.cardStylePanelRoot.gameObject.activeSelf)
			{
				return;
			}
			int index = Enumerable.FirstOrDefault<ValueTuple<int, KeyValuePair<CardWidget, string>>>(this._cardStyleDic.WithIndices<KeyValuePair<CardWidget, string>>(), ([TupleElementNames(new string[] { "index", "elem" })] ValueTuple<int, KeyValuePair<CardWidget, string>> pair) => pair.Item2.Key == this._centerCardStyle).Item1;
			if (eventData.scrollDelta.y < 0f && index < this._cardStyleDic.Count - 1)
			{
				ValueTuple<int, KeyValuePair<CardWidget, string>> valueTuple = Enumerable.FirstOrDefault<ValueTuple<int, KeyValuePair<CardWidget, string>>>(this._cardStyleDic.WithIndices<KeyValuePair<CardWidget, string>>(), ([TupleElementNames(new string[] { "index", "elem" })] ValueTuple<int, KeyValuePair<CardWidget, string>> pair) => pair.Item1 == index + 1);
				this.OnCardStyleClick(valueTuple.Item2.Key);
			}
			if (eventData.scrollDelta.y > 0f && index > 0)
			{
				ValueTuple<int, KeyValuePair<CardWidget, string>> valueTuple = Enumerable.FirstOrDefault<ValueTuple<int, KeyValuePair<CardWidget, string>>>(this._cardStyleDic.WithIndices<KeyValuePair<CardWidget, string>>(), ([TupleElementNames(new string[] { "index", "elem" })] ValueTuple<int, KeyValuePair<CardWidget, string>> pair) => pair.Item1 == index - 1);
				this.OnCardStyleClick(valueTuple.Item2.Key);
			}
		}

		// Token: 0x04000493 RID: 1171
		[Header("资源引用")]
		[SerializeField]
		private Button bg;

		// Token: 0x04000494 RID: 1172
		[SerializeField]
		private CardWidget cardDetailWidget;

		// Token: 0x04000495 RID: 1173
		[SerializeField]
		private TextMeshProUGUI illustratorText;

		// Token: 0x04000496 RID: 1174
		[SerializeField]
		private CanvasGroup subWidgetGroup;

		// Token: 0x04000497 RID: 1175
		[SerializeField]
		private RectTransform tooltipParent;

		// Token: 0x04000498 RID: 1176
		[SerializeField]
		private EntityTooltipWidget tooltipTemplate;

		// Token: 0x04000499 RID: 1177
		[SerializeField]
		private List<CardWidget> relativeCardWidgets;

		// Token: 0x0400049A RID: 1178
		[SerializeField]
		private RecordCardCell cardCellTemplate;

		// Token: 0x0400049B RID: 1179
		[SerializeField]
		private Transform relativeCellLayout;

		// Token: 0x0400049C RID: 1180
		[SerializeField]
		private CanvasGroup tempCardRoot;

		// Token: 0x0400049D RID: 1181
		[SerializeField]
		private SwitchWidget upgradeSwitch;

		// Token: 0x0400049E RID: 1182
		[SerializeField]
		private RectTransform cardStylePanelRoot;

		// Token: 0x0400049F RID: 1183
		[SerializeField]
		private Button cardStylePanelBg;

		// Token: 0x040004A0 RID: 1184
		[SerializeField]
		private TextMeshProUGUI cardStyleIllustratorText;

		// Token: 0x040004A1 RID: 1185
		[SerializeField]
		private Button cardStyleConfirmButton;

		// Token: 0x040004A2 RID: 1186
		[SerializeField]
		private Button showCardStyleButton;

		// Token: 0x040004A3 RID: 1187
		[SerializeField]
		private RectTransform cardStyleParent;

		// Token: 0x040004A4 RID: 1188
		[Header("动画参数")]
		[SerializeField]
		private float animDuration = 0.6f;

		// Token: 0x040004A5 RID: 1189
		[Header("Debug Only")]
		[SerializeField]
		private Button removeCardButton;

		// Token: 0x040004A6 RID: 1190
		[SerializeField]
		private Button saveImageButton;

		// Token: 0x040004A7 RID: 1191
		private EntityTooltipWidget _currentTooltip;

		// Token: 0x040004A8 RID: 1192
		private Card _sourceCard;

		// Token: 0x040004A9 RID: 1193
		private Card _card;

		// Token: 0x040004AA RID: 1194
		private Card _upgradedCard;

		// Token: 0x040004AB RID: 1195
		private CanvasGroup _canvasGroup;

		// Token: 0x040004AC RID: 1196
		private readonly Dictionary<CardWidget, string> _cardStyleDic = new Dictionary<CardWidget, string>();

		// Token: 0x040004AD RID: 1197
		private CardWidget _centerCardStyle;

		// Token: 0x040004AE RID: 1198
		private float _maxTooltipWidth;

		// Token: 0x040004AF RID: 1199
		private float _maxTooltipHeight;

		// Token: 0x040004B0 RID: 1200
		private const int MaxRelativeCardCount = 10;

		// Token: 0x040004B1 RID: 1201
		private bool _preventRightClickHide;
	}
}
