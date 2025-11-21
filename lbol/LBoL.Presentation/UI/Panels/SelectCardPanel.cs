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
	// Token: 0x020000B1 RID: 177
	public class SelectCardPanel : UiPanel<SelectCardPayload>, IInputActionHandler
	{
		// Token: 0x17000191 RID: 401
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x000318D5 File Offset: 0x0002FAD5
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x060009CC RID: 2508 RVA: 0x000318D8 File Offset: 0x0002FAD8
		// (set) Token: 0x060009CD RID: 2509 RVA: 0x000318E0 File Offset: 0x0002FAE0
		public int Min { get; private set; }

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x060009CE RID: 2510 RVA: 0x000318E9 File Offset: 0x0002FAE9
		// (set) Token: 0x060009CF RID: 2511 RVA: 0x000318F1 File Offset: 0x0002FAF1
		public int Max { get; private set; }

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x060009D0 RID: 2512 RVA: 0x000318FA File Offset: 0x0002FAFA
		// (set) Token: 0x060009D1 RID: 2513 RVA: 0x00031902 File Offset: 0x0002FB02
		public bool Sortable { get; set; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x060009D2 RID: 2514 RVA: 0x0003190B File Offset: 0x0002FB0B
		// (set) Token: 0x060009D3 RID: 2515 RVA: 0x00031913 File Offset: 0x0002FB13
		public List<Card> SelectedCards { get; private set; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x060009D4 RID: 2516 RVA: 0x0003191C File Offset: 0x0002FB1C
		// (set) Token: 0x060009D5 RID: 2517 RVA: 0x00031924 File Offset: 0x0002FB24
		public bool IsCanceled { get; private set; }

		// Token: 0x060009D6 RID: 2518 RVA: 0x00031930 File Offset: 0x0002FB30
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.miniSkipButton.onClick.AddListener(new UnityAction(this.Skip));
			this.miniCancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.cardParent.transform.DestroyChildren();
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x000319C0 File Offset: 0x0002FBC0
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<SelectHandInteraction>(new InteractionViewer<SelectHandInteraction>(this.ViewSelectHand));
			base.GameRun.InteractionViewer.Register<SelectCardInteraction>(new InteractionViewer<SelectCardInteraction>(this.ViewSelectCard));
			base.GameRun.InteractionViewer.Register<MiniSelectCardInteraction>(new InteractionViewer<MiniSelectCardInteraction>(this.ViewMiniSelect));
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x00031A24 File Offset: 0x0002FC24
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<SelectHandInteraction>(new InteractionViewer<SelectHandInteraction>(this.ViewSelectHand));
			base.GameRun.InteractionViewer.Unregister<SelectCardInteraction>(new InteractionViewer<SelectCardInteraction>(this.ViewSelectCard));
			base.GameRun.InteractionViewer.Unregister<MiniSelectCardInteraction>(new InteractionViewer<MiniSelectCardInteraction>(this.ViewMiniSelect));
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x00031A88 File Offset: 0x0002FC88
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

		// Token: 0x060009DA RID: 2522 RVA: 0x00031CA0 File Offset: 0x0002FEA0
		protected override void OnHiding()
		{
			UiManager.GetPanel<PlayBoard>().SetCursorVisible();
			this._payload = null;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00031CBC File Offset: 0x0002FEBC
		protected override void OnHided()
		{
			foreach (SelectCardWidget selectCardWidget in this._selectCardWidgets)
			{
				Object.Destroy(selectCardWidget.gameObject);
			}
			this._selectCardWidgets.Clear();
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x00031D1C File Offset: 0x0002FF1C
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

		// Token: 0x060009DD RID: 2525 RVA: 0x00031D58 File Offset: 0x0002FF58
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

		// Token: 0x060009DE RID: 2526 RVA: 0x00031E58 File Offset: 0x00030058
		private void Cancel()
		{
			this.IsCanceled = true;
			base.Hide();
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x00031E67 File Offset: 0x00030067
		private void Skip()
		{
			base.Hide();
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x00031E6F File Offset: 0x0003006F
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

		// Token: 0x060009E1 RID: 2529 RVA: 0x00031E93 File Offset: 0x00030093
		public IEnumerator ShowAsync(SelectCardPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x00031EA9 File Offset: 0x000300A9
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

		// Token: 0x060009E3 RID: 2531 RVA: 0x00031EBF File Offset: 0x000300BF
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

		// Token: 0x060009E4 RID: 2532 RVA: 0x00031ED5 File Offset: 0x000300D5
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

		// Token: 0x060009E5 RID: 2533 RVA: 0x00031EEB File Offset: 0x000300EB
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

		// Token: 0x060009E6 RID: 2534 RVA: 0x00031F04 File Offset: 0x00030104
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

		// Token: 0x060009E7 RID: 2535 RVA: 0x00031FA4 File Offset: 0x000301A4
		private void SetTitle(string titleText)
		{
			this.titleTmp.text = titleText;
			this.titleRoot.gameObject.SetActive(true);
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x00031FC3 File Offset: 0x000301C3
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

		// Token: 0x0400073A RID: 1850
		[SerializeField]
		private RectTransform cardParent;

		// Token: 0x0400073B RID: 1851
		[SerializeField]
		private TextMeshProUGUI titleTmp;

		// Token: 0x0400073C RID: 1852
		[SerializeField]
		private RectTransform titleRoot;

		// Token: 0x0400073D RID: 1853
		[SerializeField]
		private CardWidget cardTemplate;

		// Token: 0x0400073E RID: 1854
		[SerializeField]
		private GameObject selectParticle;

		// Token: 0x0400073F RID: 1855
		[SerializeField]
		private RectTransform normalSelectCardRoot;

		// Token: 0x04000740 RID: 1856
		[SerializeField]
		private Button confirmButton;

		// Token: 0x04000741 RID: 1857
		[SerializeField]
		private Button cancelButton;

		// Token: 0x04000742 RID: 1858
		[SerializeField]
		private RectTransform miniSelectCardRoot;

		// Token: 0x04000743 RID: 1859
		[SerializeField]
		private CanvasGroup miniCg;

		// Token: 0x04000744 RID: 1860
		[SerializeField]
		private Button miniSkipButton;

		// Token: 0x04000745 RID: 1861
		[SerializeField]
		private Button miniCancelButton;

		// Token: 0x04000746 RID: 1862
		[SerializeField]
		private GameObject selectCardLayout;

		// Token: 0x04000747 RID: 1863
		[SerializeField]
		private MinimizedButtonWidget minimizedButton;

		// Token: 0x04000748 RID: 1864
		private SelectCardPayload _payload;

		// Token: 0x04000749 RID: 1865
		private readonly List<SelectCardWidget> _selectCardWidgets = new List<SelectCardWidget>();

		// Token: 0x0400074A RID: 1866
		private readonly List<int> _selectIndexOrder = new List<int>();

		// Token: 0x0400074B RID: 1867
		private CardWidget _bufferedCardWidget;

		// Token: 0x04000751 RID: 1873
		private const int CardsPerRow = 5;

		// Token: 0x04000752 RID: 1874
		private const float NormalHigh1 = 1200f;

		// Token: 0x04000753 RID: 1875
		private const float NormalHigh2 = 1600f;

		// Token: 0x04000754 RID: 1876
		private const float TitleNormalY1 = -400f;

		// Token: 0x04000755 RID: 1877
		private const float TitleNormalY2 = -200f;

		// Token: 0x04000756 RID: 1878
		private const float TitleMiniY = -300f;

		// Token: 0x04000757 RID: 1879
		private const float XStart = -2300f;

		// Token: 0x04000758 RID: 1880
		private readonly float[] _xDistance = new float[] { 800f, 800f, 800f, 750f, 700f };

		// Token: 0x04000759 RID: 1881
		private const float StartDelay = 0.1f;

		// Token: 0x0400075A RID: 1882
		private const float FlyInTime = 0.45f;

		// Token: 0x0400075B RID: 1883
		private const float PauseTime = 0.1f;

		// Token: 0x0400075C RID: 1884
		private const float FlipTime = 0.35f;

		// Token: 0x0400075D RID: 1885
		private const float CreateInterval = 0.05f;

		// Token: 0x0400075E RID: 1886
		private const float FlipInterval = 0.05f;

		// Token: 0x0400075F RID: 1887
		private const float Scale = 1.1f;
	}
}
