using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.Panels
{
	public class SelectCardPanel : UiPanel<SelectCardPayload>, IInputActionHandler
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		public int Min { get; private set; }
		public int Max { get; private set; }
		public bool Sortable { get; set; }
		public List<Card> SelectedCards { get; private set; }
		public bool IsCanceled { get; private set; }
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.miniSkipButton.onClick.AddListener(new UnityAction(this.Skip));
			this.miniCancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.cardParent.transform.DestroyChildren();
		}
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<SelectHandInteraction>(new InteractionViewer<SelectHandInteraction>(this.ViewSelectHand));
			base.GameRun.InteractionViewer.Register<SelectCardInteraction>(new InteractionViewer<SelectCardInteraction>(this.ViewSelectCard));
			base.GameRun.InteractionViewer.Register<MiniSelectCardInteraction>(new InteractionViewer<MiniSelectCardInteraction>(this.ViewMiniSelect));
		}
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<SelectHandInteraction>(new InteractionViewer<SelectHandInteraction>(this.ViewSelectHand));
			base.GameRun.InteractionViewer.Unregister<SelectCardInteraction>(new InteractionViewer<SelectCardInteraction>(this.ViewSelectCard));
			base.GameRun.InteractionViewer.Unregister<MiniSelectCardInteraction>(new InteractionViewer<MiniSelectCardInteraction>(this.ViewMiniSelect));
		}
		protected override void OnShowing(SelectCardPayload payload)
		{
			this._payload = payload;
			this.minimizedButton.Init();
			foreach (SelectCardWidget selectCardWidget in this._selectCardWidgets)
			{
				Object.Destroy(selectCardWidget.gameObject);
			}
			this._selectCardWidgets.Clear();
			this._selectIndexOrder.Clear();
			this.cancelButton.gameObject.SetActive(payload.CanCancel);
			this.miniCancelButton.gameObject.SetActive(payload.CanCancel);
			this.Min = payload.Min;
			this.Max = payload.Max;
			this.Sortable = payload.Sortable;
			this.confirmButton.interactable = 0 >= this.Min;
			this.SelectedCards = null;
			this.IsCanceled = false;
			foreach (Card card in payload.Cards)
			{
				CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardTemplate, this.cardParent);
				cardWidget.Card = card;
				if (base.GameRun.Player.HasExhibit<ZhinengYinxiang>())
				{
					int? cardInstanceId = base.GameRun.Player.GetExhibit<ZhinengYinxiang>().CardInstanceId;
					if (cardInstanceId != null && cardInstanceId.Value == card.InstanceId)
					{
						cardWidget.ShowSticker = true;
					}
				}
				cardWidget.gameObject.AddComponent<ShowingCard>().SetScale(1f, 1.1f);
				SelectCardWidget selectCardWidget2 = cardWidget.gameObject.AddComponent<SelectCardWidget>();
				selectCardWidget2.SelectParticle = Object.Instantiate<GameObject>(this.selectParticle, selectCardWidget2.transform);
				selectCardWidget2.SelectParticle.SetActive(false);
				selectCardWidget2.SelectedChanged += delegate(object sender, EventArgs args)
				{
					SelectCardWidget widget = sender as SelectCardWidget;
					if (widget != null)
					{
						if (widget.IsSelected && !this._selectIndexOrder.Exists((int t) => t == this._selectCardWidgets.IndexOf(widget)))
						{
							this._selectIndexOrder.Add(this._selectCardWidgets.IndexOf(widget));
						}
						if (!widget.IsSelected)
						{
							this._selectIndexOrder.Remove(this._selectCardWidgets.IndexOf(widget));
						}
					}
					int num = Enumerable.Count<SelectCardWidget>(this._selectCardWidgets, (SelectCardWidget w) => w.IsSelected);
					if (num > this.Max)
					{
						this._selectCardWidgets[this._selectIndexOrder[0]].SetSelected(false, false);
						this._selectIndexOrder.RemoveAt(0);
					}
					num = Enumerable.Count<SelectCardWidget>(this._selectCardWidgets, (SelectCardWidget w) => w.IsSelected);
					this.confirmButton.interactable = num >= this.Min && num <= this.Max;
					if (this.Sortable)
					{
						foreach (ValueTuple<int, SelectCardWidget> valueTuple in this._selectCardWidgets.WithIndices<SelectCardWidget>())
						{
							int i = valueTuple.Item1;
							SelectCardWidget item = valueTuple.Item2;
							if (this._selectIndexOrder.Exists((int t) => t == i))
							{
								item.CardWidget.ShowDeckIndex(this._selectIndexOrder.IndexOf(i) + 1, false, false);
							}
							else
							{
								item.CardWidget.HideDeckIndex();
							}
						}
					}
				};
				this._selectCardWidgets.Add(selectCardWidget2);
			}
			this.cardParent.anchoredPosition = Vector2.zero;
			Cursor.visible = true;
			UiManager.PushActionHandler(this);
		}
		protected override void OnHiding()
		{
			UiManager.GetPanel<PlayBoard>().SetCursorVisible();
			this._payload = null;
			UiManager.PopActionHandler(this);
		}
		protected override void OnHided()
		{
			foreach (SelectCardWidget selectCardWidget in this._selectCardWidgets)
			{
				Object.Destroy(selectCardWidget.gameObject);
			}
			this._selectCardWidgets.Clear();
		}
		public bool SwitchMinimized()
		{
			if (!base.IsVisible)
			{
				return false;
			}
			CanvasGroup component = base.GetComponent<CanvasGroup>();
			if (!component || !component.interactable)
			{
				return false;
			}
			this.minimizedButton.SwitchMinimized();
			return true;
		}
		private void Confirm()
		{
			this.SelectedCards = new List<Card>();
			if (this.Sortable)
			{
				using (IEnumerator<SelectCardWidget> enumerator = Enumerable.Where<SelectCardWidget>(Enumerable.Select<int, SelectCardWidget>(this._selectIndexOrder, (int index) => this._selectCardWidgets.TryGetValue(index)), (SelectCardWidget selectWidget) => selectWidget && selectWidget.IsSelected).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						SelectCardWidget selectCardWidget = enumerator.Current;
						this.SelectedCards.Add(selectCardWidget.Card);
					}
					goto IL_00CA;
				}
			}
			foreach (SelectCardWidget selectCardWidget2 in this._selectCardWidgets)
			{
				if (selectCardWidget2.IsSelected)
				{
					this.SelectedCards.Add(selectCardWidget2.Card);
				}
			}
			IL_00CA:
			this.IsCanceled = false;
			base.Hide();
		}
		private void Cancel()
		{
			this.IsCanceled = true;
			base.Hide();
		}
		private void Skip()
		{
			base.Hide();
		}
		public void OnCancel()
		{
			if (this._payload != null)
			{
				if (this._payload.CanCancel)
				{
					this.Cancel();
					return;
				}
			}
			else
			{
				base.Hide();
			}
		}
		public IEnumerator ShowAsync(SelectCardPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}
		public IEnumerator ShowMiniSelect(SelectCardPayload payload)
		{
			SelectCardPanel.<>c__DisplayClass58_0 CS$<>8__locals1 = new SelectCardPanel.<>c__DisplayClass58_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.payload = payload;
			this.selectCardLayout.transform.DestroyChildren();
			this.miniSelectCardRoot.gameObject.SetActive(true);
			this.normalSelectCardRoot.gameObject.SetActive(false);
			this.titleRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -300f);
			this.miniSkipButton.gameObject.SetActive(CS$<>8__locals1.payload.CanSkip);
			this.SetTitle(CS$<>8__locals1.payload.Name);
			CS$<>8__locals1.cards = Enumerable.ToList<Card>(CS$<>8__locals1.payload.Cards);
			CS$<>8__locals1.total = CS$<>8__locals1.cards.Count;
			if (CS$<>8__locals1.total > 5)
			{
				CS$<>8__locals1.total = 5;
				CS$<>8__locals1.cards = Enumerable.ToList<Card>(Enumerable.Take<Card>(CS$<>8__locals1.cards, CS$<>8__locals1.total));
				Debug.LogError("Too many cards in reward.");
			}
			CS$<>8__locals1.slidePlayed = false;
			DOTween.Sequence().Insert(0f, this.miniCg.DOFade(1f, 0.2f).From(0f, true, false)).SetLink(this.miniSelectCardRoot.gameObject)
				.OnComplete(delegate
				{
					using (IEnumerator<ValueTuple<int, Card>> enumerator = CS$<>8__locals1.cards.WithIndices<Card>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							SelectCardPanel.<>c__DisplayClass58_1 CS$<>8__locals2 = new SelectCardPanel.<>c__DisplayClass58_1();
							CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
							ValueTuple<int, Card> valueTuple = enumerator.Current;
							CS$<>8__locals2.i = valueTuple.Item1;
							Card item = valueTuple.Item2;
							CardWidget widget = Object.Instantiate<CardWidget>(CS$<>8__locals1.<>4__this.cardTemplate, CS$<>8__locals1.<>4__this.selectCardLayout.transform);
							widget.Card = item;
							widget.name = "Card:" + item.Name;
							Transform transform = widget.transform;
							transform.localPosition = new Vector3(-2300f, 0f, 0f);
							transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
							transform.localScale = new Vector3(1.1f, 1.1f, 1f);
							Sequence sequence = DOTween.Sequence().AppendInterval(0.1f + (float)CS$<>8__locals2.i * 0.05f);
							TweenCallback tweenCallback;
							if ((tweenCallback = CS$<>8__locals1.<>9__2) == null)
							{
								tweenCallback = (CS$<>8__locals1.<>9__2 = delegate
								{
									if (CS$<>8__locals1.slidePlayed)
									{
										return;
									}
									AudioManager.PlayUi("CardSlide", false);
									CS$<>8__locals1.slidePlayed = true;
								});
							}
							UnityAction <>9__5;
							sequence.AppendCallback(tweenCallback).Append(transform.DOLocalMoveX(CS$<>8__locals1.<>4__this._xDistance[CS$<>8__locals1.total - 1] * ((float)CS$<>8__locals2.i - (float)(CS$<>8__locals1.total - 1) / 2f), 0.45f, false).SetEase(Ease.OutCubic)).AppendInterval(0.1f + (float)CS$<>8__locals2.i * 0.05f)
								.AppendCallback(delegate
								{
									AudioManager.PlayUi("CardFlip_" + (5 - CS$<>8__locals2.CS$<>8__locals1.total + CS$<>8__locals2.i + 1).ToString(), false);
								})
								.Append(transform.DOLocalRotate(Vector3.zero, 0.35f, RotateMode.Fast).SetEase(Ease.OutQuad))
								.SetAutoKill(true)
								.SetUpdate(true)
								.OnComplete(delegate
								{
									widget.gameObject.AddComponent<ShowingCard>().SetScale(1.1f);
									UnityEvent onClick = widget.gameObject.GetOrAddComponent<Button>().onClick;
									UnityAction unityAction;
									if ((unityAction = <>9__5) == null)
									{
										unityAction = (<>9__5 = delegate
										{
											SelectCardPanel <>4__this = CS$<>8__locals2.CS$<>8__locals1.<>4__this;
											List<Card> list = new List<Card>();
											list.Add(widget.Card);
											<>4__this.SelectedCards = list;
											if (CS$<>8__locals2.CS$<>8__locals1.payload.IsAddCardToDeck)
											{
												CS$<>8__locals2.CS$<>8__locals1.<>4__this._bufferedCardWidget = widget;
											}
											CS$<>8__locals2.CS$<>8__locals1.<>4__this.Hide();
										});
									}
									onClick.AddListener(unityAction);
								});
						}
					}
				});
			yield return this.ShowAsync(CS$<>8__locals1.payload);
			this.miniCg.DOFade(0f, 0.2f).From(1f, true, false).OnComplete(delegate
			{
				CS$<>8__locals1.<>4__this.miniSelectCardRoot.gameObject.SetActive(false);
			});
			yield break;
		}
		private IEnumerator ViewSelectHand(SelectHandInteraction interaction)
		{
			this.miniSelectCardRoot.gameObject.SetActive(false);
			this.normalSelectCardRoot.gameObject.SetActive(true);
			this.normalSelectCardRoot.sizeDelta = this.normalSelectCardRoot.sizeDelta.WithY((interaction.PendingCards.Count > 5) ? 1600f : 1200f);
			this.titleRoot.anchoredPosition = new Vector2(0f, (interaction.PendingCards.Count > 5) ? (-200f) : (-400f));
			this.confirmButton.gameObject.SetActive(true);
			this.SetTitle(interaction);
			yield return this.ShowAsync(new SelectCardPayload
			{
				Cards = interaction.PendingCards,
				Min = interaction.Min,
				Max = interaction.Max,
				Sortable = interaction.Sortable,
				CanCancel = interaction.CanCancel
			});
			if (this.IsCanceled)
			{
				interaction.Cancel();
			}
			else
			{
				interaction.SelectedCards = this.SelectedCards;
				this.SelectedCards = null;
			}
			yield break;
		}
		private IEnumerator ViewSelectCard(SelectCardInteraction interaction)
		{
			this.miniSelectCardRoot.gameObject.SetActive(false);
			this.normalSelectCardRoot.gameObject.SetActive(true);
			this.normalSelectCardRoot.sizeDelta = this.normalSelectCardRoot.sizeDelta.WithY((interaction.PendingCards.Count > 5) ? 1600f : 1200f);
			this.titleRoot.anchoredPosition = new Vector2(0f, (interaction.PendingCards.Count > 5) ? (-200f) : (-400f));
			this.confirmButton.gameObject.SetActive(true);
			this.SetTitle(interaction);
			yield return this.ShowAsync(new SelectCardPayload
			{
				Cards = interaction.PendingCards,
				Min = interaction.Min,
				Max = interaction.Max,
				Sortable = interaction.Sortable,
				CanCancel = interaction.CanCancel
			});
			if (this.IsCanceled)
			{
				interaction.Cancel();
			}
			else
			{
				interaction.SelectedCards = this.SelectedCards;
				this.SelectedCards = null;
			}
			yield break;
		}
		private IEnumerator ViewMiniSelect(MiniSelectCardInteraction interaction)
		{
			SelectCardPanel.<>c__DisplayClass70_0 CS$<>8__locals1 = new SelectCardPanel.<>c__DisplayClass70_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.interaction = interaction;
			this.selectCardLayout.transform.DestroyChildren();
			this.miniSelectCardRoot.gameObject.SetActive(true);
			this.normalSelectCardRoot.gameObject.SetActive(false);
			this.titleRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -300f);
			this.miniSkipButton.gameObject.SetActive(CS$<>8__locals1.interaction.CanSkip);
			this.SetTitle(CS$<>8__locals1.interaction);
			CS$<>8__locals1.cards = CS$<>8__locals1.interaction.PendingCards;
			CS$<>8__locals1.total = CS$<>8__locals1.cards.Count;
			if (CS$<>8__locals1.total > 5)
			{
				CS$<>8__locals1.total = 5;
				CS$<>8__locals1.cards = Enumerable.ToList<Card>(Enumerable.Take<Card>(CS$<>8__locals1.cards, CS$<>8__locals1.total));
				Debug.LogError("Too many cards in reward.");
			}
			if (CS$<>8__locals1.interaction.HasSlideInAnimation)
			{
				SelectCardPanel.<>c__DisplayClass70_1 CS$<>8__locals2 = new SelectCardPanel.<>c__DisplayClass70_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.slidePlayed = false;
				DOTween.Sequence().Insert(0f, this.miniCg.DOFade(1f, 0.2f).From(0f, true, false)).SetLink(this.miniSelectCardRoot.gameObject)
					.OnComplete(delegate
					{
						using (IEnumerator<ValueTuple<int, Card>> enumerator2 = CS$<>8__locals2.CS$<>8__locals1.cards.WithIndices<Card>().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								SelectCardPanel.<>c__DisplayClass70_2 CS$<>8__locals4 = new SelectCardPanel.<>c__DisplayClass70_2();
								CS$<>8__locals4.CS$<>8__locals2 = CS$<>8__locals2;
								ValueTuple<int, Card> valueTuple2 = enumerator2.Current;
								CS$<>8__locals4.i = valueTuple2.Item1;
								Card item3 = valueTuple2.Item2;
								CardWidget widget2 = Object.Instantiate<CardWidget>(CS$<>8__locals2.CS$<>8__locals1.<>4__this.cardTemplate, CS$<>8__locals2.CS$<>8__locals1.<>4__this.selectCardLayout.transform);
								widget2.Card = item3;
								widget2.name = "Card:" + item3.Name;
								Transform transform2 = widget2.transform;
								transform2.localPosition = new Vector3(-2300f, 0f, 0f);
								transform2.localRotation = Quaternion.Euler(0f, 180f, 0f);
								transform2.localScale = new Vector3(1.1f, 1.1f, 1f);
								Sequence sequence = DOTween.Sequence().AppendInterval(0.1f + (float)CS$<>8__locals4.i * 0.05f);
								TweenCallback tweenCallback;
								if ((tweenCallback = CS$<>8__locals2.<>9__2) == null)
								{
									tweenCallback = (CS$<>8__locals2.<>9__2 = delegate
									{
										if (CS$<>8__locals2.slidePlayed)
										{
											return;
										}
										AudioManager.PlayUi("CardSlide", false);
										CS$<>8__locals2.slidePlayed = true;
									});
								}
								UnityAction <>9__5;
								sequence.AppendCallback(tweenCallback).Append(transform2.DOLocalMoveX(CS$<>8__locals2.CS$<>8__locals1.<>4__this._xDistance[CS$<>8__locals2.CS$<>8__locals1.total - 1] * ((float)CS$<>8__locals4.i - (float)(CS$<>8__locals2.CS$<>8__locals1.total - 1) / 2f), 0.45f, false).SetEase(Ease.OutCubic)).AppendInterval(0.1f + (float)CS$<>8__locals4.i * 0.05f)
									.AppendCallback(delegate
									{
										AudioManager.PlayUi("CardFlip_" + (5 - CS$<>8__locals4.CS$<>8__locals2.CS$<>8__locals1.total + CS$<>8__locals4.i + 1).ToString(), false);
									})
									.Append(transform2.DOLocalRotate(Vector3.zero, 0.35f, RotateMode.Fast).SetEase(Ease.OutQuad))
									.SetAutoKill(true)
									.SetUpdate(true)
									.OnComplete(delegate
									{
										widget2.gameObject.AddComponent<ShowingCard>().SetScale(1.1f, 1.265f);
										UnityEvent onClick = widget2.gameObject.GetOrAddComponent<Button>().onClick;
										UnityAction unityAction;
										if ((unityAction = <>9__5) == null)
										{
											unityAction = (<>9__5 = delegate
											{
												CS$<>8__locals4.CS$<>8__locals2.CS$<>8__locals1.interaction.SelectedCard = widget2.Card;
												if (CS$<>8__locals4.CS$<>8__locals2.CS$<>8__locals1.interaction.IsAddCardToDeck)
												{
													CS$<>8__locals4.CS$<>8__locals2.CS$<>8__locals1.<>4__this._bufferedCardWidget = widget2;
												}
												CS$<>8__locals4.CS$<>8__locals2.CS$<>8__locals1.<>4__this.Hide();
											});
										}
										onClick.AddListener(unityAction);
									});
							}
						}
					});
			}
			else
			{
				DOTween.Sequence().Insert(0f, this.miniCg.DOFade(1f, 0.4f).From(0f, true, false));
				foreach (ValueTuple<int, Card> valueTuple in CS$<>8__locals1.cards.WithIndices<Card>())
				{
					int item = valueTuple.Item1;
					Card item2 = valueTuple.Item2;
					CardWidget widget = Object.Instantiate<CardWidget>(this.cardTemplate, this.selectCardLayout.transform);
					widget.Card = item2;
					Transform transform = widget.transform;
					float num = this._xDistance[CS$<>8__locals1.total - 1] * ((float)item - (float)(CS$<>8__locals1.total - 1) / 2f);
					transform.localPosition = new Vector3(num, 0f, 0f);
					widget.gameObject.AddComponent<ShowingCard>().SetScale(1.1f, 1.3f);
					widget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
					{
						CS$<>8__locals1.interaction.SelectedCard = widget.Card;
						if (CS$<>8__locals1.interaction.IsAddCardToDeck)
						{
							CS$<>8__locals1.<>4__this._bufferedCardWidget = widget;
						}
						CS$<>8__locals1.<>4__this.Hide();
					});
				}
			}
			yield return this.ShowAsync(new SelectCardPayload
			{
				Cards = CS$<>8__locals1.interaction.PendingCards,
				Min = 1,
				Max = 1,
				CanCancel = CS$<>8__locals1.interaction.CanCancel
			});
			this.miniCg.DOFade(0f, 0.2f).From(1f, true, false).OnComplete(delegate
			{
				CS$<>8__locals1.<>4__this.miniSelectCardRoot.gameObject.SetActive(false);
			});
			if (this.IsCanceled)
			{
				CS$<>8__locals1.interaction.Cancel();
			}
			yield break;
		}
		private void SetTitle(Interaction interaction)
		{
			string description = interaction.Description;
			GameEntity source = interaction.Source;
			if (description != null)
			{
				this.titleTmp.text = description;
				this.titleRoot.gameObject.SetActive(true);
				return;
			}
			if (source != null)
			{
				this.titleTmp.text = source.Name;
				Card card = source as Card;
				if (card != null && card.IsUpgraded)
				{
					TextMeshProUGUI textMeshProUGUI = this.titleTmp;
					textMeshProUGUI.text += "+";
				}
				this.titleRoot.gameObject.SetActive(true);
				return;
			}
			this.titleRoot.gameObject.SetActive(false);
		}
		private void SetTitle(string titleText)
		{
			this.titleTmp.text = titleText;
			this.titleRoot.gameObject.SetActive(true);
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
		[SerializeField]
		private RectTransform cardParent;
		[SerializeField]
		private TextMeshProUGUI titleTmp;
		[SerializeField]
		private RectTransform titleRoot;
		[SerializeField]
		private CardWidget cardTemplate;
		[SerializeField]
		private GameObject selectParticle;
		[SerializeField]
		private RectTransform normalSelectCardRoot;
		[SerializeField]
		private Button confirmButton;
		[SerializeField]
		private Button cancelButton;
		[SerializeField]
		private RectTransform miniSelectCardRoot;
		[SerializeField]
		private CanvasGroup miniCg;
		[SerializeField]
		private Button miniSkipButton;
		[SerializeField]
		private Button miniCancelButton;
		[SerializeField]
		private GameObject selectCardLayout;
		[SerializeField]
		private MinimizedButtonWidget minimizedButton;
		private SelectCardPayload _payload;
		private readonly List<SelectCardWidget> _selectCardWidgets = new List<SelectCardWidget>();
		private readonly List<int> _selectIndexOrder = new List<int>();
		private CardWidget _bufferedCardWidget;
		private const int CardsPerRow = 5;
		private const float NormalHigh1 = 1200f;
		private const float NormalHigh2 = 1600f;
		private const float TitleNormalY1 = -400f;
		private const float TitleNormalY2 = -200f;
		private const float TitleMiniY = -300f;
		private const float XStart = -2300f;
		private readonly float[] _xDistance = new float[] { 800f, 800f, 800f, 750f, 700f };
		private const float StartDelay = 0.1f;
		private const float FlyInTime = 0.45f;
		private const float PauseTime = 0.1f;
		private const float FlipTime = 0.35f;
		private const float CreateInterval = 0.05f;
		private const float FlipInterval = 0.05f;
		private const float Scale = 1.1f;
	}
}
