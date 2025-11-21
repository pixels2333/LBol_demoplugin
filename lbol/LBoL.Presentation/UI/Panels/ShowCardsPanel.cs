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
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000B6 RID: 182
	public class ShowCardsPanel : UiPanel<ShowCardsPayload>, IInputActionHandler
	{
		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000A61 RID: 2657 RVA: 0x000347DD File Offset: 0x000329DD
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000A62 RID: 2658 RVA: 0x000347E0 File Offset: 0x000329E0
		// (set) Token: 0x06000A63 RID: 2659 RVA: 0x000347E8 File Offset: 0x000329E8
		public bool IsCanceled { get; private set; }

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000A64 RID: 2660 RVA: 0x000347F1 File Offset: 0x000329F1
		// (set) Token: 0x06000A65 RID: 2661 RVA: 0x000347F9 File Offset: 0x000329F9
		public Card SelectedCard { get; private set; }

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000A66 RID: 2662 RVA: 0x00034802 File Offset: 0x00032A02
		// (set) Token: 0x06000A67 RID: 2663 RVA: 0x0003480A File Offset: 0x00032A0A
		public Card TransformCard { get; private set; }

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000A68 RID: 2664 RVA: 0x00034813 File Offset: 0x00032A13
		// (set) Token: 0x06000A69 RID: 2665 RVA: 0x0003481B File Offset: 0x00032A1B
		private ShowCardsPayload Payload { get; set; }

		// Token: 0x06000A6A RID: 2666 RVA: 0x00034824 File Offset: 0x00032A24
		public void Awake()
		{
			this.returnButton.onClick.AddListener(new UnityAction(this.CancelHide));
			this.topHideButton.onClick.AddListener(new UnityAction(this.CancelHide));
			this.deckHolder.Clear();
			this._canvasGroup = base.GetComponent<CanvasGroup>();
			this.actualOrderToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.CardOrderToggle(on, ShowCardsPanel.OrderStatus.Actual);
			});
			this.indexOrderToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.CardOrderToggle(on, ShowCardsPanel.OrderStatus.Index);
			});
			this.typeOrderToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.CardOrderToggle(on, ShowCardsPanel.OrderStatus.Type);
			});
			this.rarityOrderToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.CardOrderToggle(on, ShowCardsPanel.OrderStatus.Rarity);
			});
			this.dreamCardOnlyToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.CardFilterToggle(on, ShowCardsPanel.CardFilter.DreamCardsOnly);
			});
			this.followCardOnlyToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.CardFilterToggle(on, ShowCardsPanel.CardFilter.FollowCardsOnly);
			});
			SimpleTooltipSource.CreateWithGeneralKey(this.actualOrderToggle.gameObject, "Game.ActualOrder", "Game.ActualOrderExplain");
			SimpleTooltipSource.CreateWithGeneralKey(this.indexOrderToggle.gameObject, "Game.IndexOrder", "Game.IndexOrderExplain");
			SimpleTooltipSource.CreateWithGeneralKey(this.typeOrderToggle.gameObject, "Game.TypeOrder", "Game.TypeOrderExplain");
			SimpleTooltipSource.CreateWithGeneralKey(this.rarityOrderToggle.gameObject, "Game.RarityOrder", "Game.RarityOrderExplain");
		}

		// Token: 0x06000A6B RID: 2667 RVA: 0x000349B2 File Offset: 0x00032BB2
		public override void OnLocaleChanged()
		{
			this.deckHolder.OnLocaleChanged();
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x000349C0 File Offset: 0x00032BC0
		protected override void OnShowing(ShowCardsPayload payload)
		{
			this.Payload = payload;
			this.IsCanceled = false;
			int order = base.GameRun.Player.Config.Order;
			if (order != this._currentCharacterIndex)
			{
				this._currentCharacterIndex = order;
				this.portrait.sprite = this.characterPortraits[base.GameRun.Player.Config.Id];
			}
			this.portrait.rectTransform.DOLocalMoveX(-400f, 0.3f, false).From(-1400f, true, false).SetEase(Ease.OutCubic);
			this.deckHolder.SetTitle(payload.Name, payload.Description);
			if (this.Payload.HideActualOrder)
			{
				this.SetCards(ShowCardsPanel.OrderStatus.Index, true);
				this.indexOrderToggle.toggle.SetIsOnWithoutNotify(true);
				this.actualOrderToggle.toggle.interactable = false;
				this.actualOrderToggle.SetLock(false);
			}
			else
			{
				this.SetCards(ShowCardsPanel.OrderStatus.Actual, true);
				this.actualOrderToggle.toggle.SetIsOnWithoutNotify(true);
				this.actualOrderToggle.toggle.interactable = true;
				this.actualOrderToggle.SetLock(true);
			}
			this._canCancel = payload.CanCancel;
			this.minimizedButton.SetActive(!payload.CanCancel);
			this._showType = payload.InteractionType;
			this.returnButton.gameObject.SetActive(this._canCancel);
			this._canvasGroup.interactable = true;
			this.ResetCardFilters();
			UiManager.PushActionHandler(this);
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00034B48 File Offset: 0x00032D48
		private void SetCards(ShowCardsPanel.OrderStatus order, bool create)
		{
			if (this.Payload == null)
			{
				return;
			}
			List<Card> list;
			switch (order)
			{
			case ShowCardsPanel.OrderStatus.Actual:
				list = Enumerable.ToList<Card>(this.Payload.Cards);
				break;
			case ShowCardsPanel.OrderStatus.Index:
				list = Enumerable.ToList<Card>(Enumerable.OrderBy<Card, int>(this.Payload.Cards, (Card card) => card.Config.Index));
				break;
			case ShowCardsPanel.OrderStatus.Type:
				list = Enumerable.ToList<Card>(Enumerable.ThenBy<Card, int>(Enumerable.OrderBy<Card, CardType>(this.Payload.Cards, (Card card) => card.CardType), (Card card) => card.Config.Index));
				break;
			case ShowCardsPanel.OrderStatus.Rarity:
				list = Enumerable.ToList<Card>(Enumerable.ThenBy<Card, int>(Enumerable.OrderBy<Card, Rarity>(this.Payload.Cards, (Card card) => card.Config.Rarity), (Card card) => card.Config.Index));
				break;
			default:
				throw new ArgumentOutOfRangeException("order", order, null);
			}
			List<Card> list2 = list;
			if (create)
			{
				this.deckHolder.Clear();
				using (List<Card>.Enumerator enumerator = list2.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card = enumerator.Current;
						CardWidget cardWidget = this.deckHolder.AddCardWidget(card, true);
						if (base.GameRun.Player.HasExhibit<ZhinengYinxiang>())
						{
							int? cardInstanceId = base.GameRun.Player.GetExhibit<ZhinengYinxiang>().CardInstanceId;
							if (cardInstanceId != null && cardInstanceId.Value == card.InstanceId)
							{
								cardWidget.ShowSticker = true;
							}
						}
						switch (this.Payload.InteractionType)
						{
						case InteractionType.None:
							break;
						case InteractionType.Upgrade:
							if (this.Payload.PayCards != null && Enumerable.Contains<Card>(this.Payload.PayCards, card))
							{
								Action <>9__6;
								cardWidget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
								{
									UiDialog<UpgradeCardContent> dialog = UiManager.GetDialog<UpgradeCardDialog>();
									UpgradeCardContent upgradeCardContent = new UpgradeCardContent();
									upgradeCardContent.Card = card;
									Action action;
									if ((action = <>9__6) == null)
									{
										action = (<>9__6 = delegate
										{
											this.SelectedCard = card;
											this.Hide();
										});
									}
									upgradeCardContent.OnConfirm = action;
									upgradeCardContent.Price = this.Payload.Price;
									upgradeCardContent.Money = this.Payload.Money;
									dialog.Show(upgradeCardContent);
								});
							}
							else
							{
								Action <>9__8;
								cardWidget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
								{
									UiDialog<UpgradeCardContent> dialog2 = UiManager.GetDialog<UpgradeCardDialog>();
									UpgradeCardContent upgradeCardContent2 = new UpgradeCardContent();
									upgradeCardContent2.Card = card;
									Action action2;
									if ((action2 = <>9__8) == null)
									{
										action2 = (<>9__8 = delegate
										{
											this.SelectedCard = card;
											this.Hide();
										});
									}
									upgradeCardContent2.OnConfirm = action2;
									dialog2.Show(upgradeCardContent2);
								});
							}
							break;
						case InteractionType.Remove:
						{
							Action <>9__10;
							cardWidget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
							{
								UiDialog<RemoveCardContent> dialog3 = UiManager.GetDialog<RemoveCardDialog>();
								RemoveCardContent removeCardContent = new RemoveCardContent();
								removeCardContent.Card = card;
								Action action3;
								if ((action3 = <>9__10) == null)
								{
									action3 = (<>9__10 = delegate
									{
										this.SelectedCard = card;
										this.Hide();
									});
								}
								removeCardContent.OnConfirm = action3;
								dialog3.Show(removeCardContent);
							});
							break;
						}
						case InteractionType.Transform:
						{
							Action <>9__12;
							cardWidget.gameObject.GetOrAddComponent<Button>().onClick.AddListener(delegate
							{
								UiDialog<TransformCardContent> dialog4 = UiManager.GetDialog<TransformCardDialog>();
								TransformCardContent transformCardContent = new TransformCardContent();
								transformCardContent.Card = card;
								transformCardContent.TransformCard = this.TransformCard;
								Action action4;
								if ((action4 = <>9__12) == null)
								{
									action4 = (<>9__12 = delegate
									{
										this.SelectedCard = card;
										this.Hide();
									});
								}
								transformCardContent.OnConfirm = action4;
								dialog4.Show(transformCardContent);
							});
							break;
						}
						default:
							throw new ArgumentOutOfRangeException();
						}
					}
					goto IL_02E3;
				}
			}
			this.deckHolder.SetOrder(list2);
			IL_02E3:
			this.RefreshCardOrder();
		}

		// Token: 0x06000A6E RID: 2670 RVA: 0x00034E5C File Offset: 0x0003305C
		private void CardOrderToggle(bool on, ShowCardsPanel.OrderStatus orderStatus)
		{
			if (!on)
			{
				return;
			}
			this.SetCards(orderStatus, false);
		}

		// Token: 0x06000A6F RID: 2671 RVA: 0x00034E6C File Offset: 0x0003306C
		private void ResetCardFilters()
		{
			this._cardFilterStatus = ShowCardsPanel.CardFilter.AllCards;
			this.dreamCardOnlyToggle.toggle.SetIsOnWithoutNotify(false);
			this.followCardOnlyToggle.toggle.SetIsOnWithoutNotify(false);
			int num = Enumerable.Count<Card>(this.deckHolder.cards.Keys, (Card c) => c.IsDreamCard);
			this.dreamCardOnlyToggle.gameObject.SetActive(num > 0);
			num = Enumerable.Count<Card>(this.deckHolder.cards.Keys, (Card c) => c.IsFollowCard);
			this.followCardOnlyToggle.gameObject.SetActive(num > 0);
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x00034F34 File Offset: 0x00033134
		private void SetCardFilterStatus(ShowCardsPanel.CardFilter status)
		{
			this._cardFilterStatus = status;
			Card card;
			CardWidget cardWidget;
			switch (status)
			{
			case ShowCardsPanel.CardFilter.AllCards:
			{
				using (IEnumerator<KeyValuePair<Card, CardWidget>> enumerator = this.deckHolder.cards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<Card, CardWidget> keyValuePair = enumerator.Current;
						keyValuePair.Deconstruct(ref card, ref cardWidget);
						cardWidget.gameObject.SetActive(true);
					}
					return;
				}
				break;
			}
			case ShowCardsPanel.CardFilter.DreamCardsOnly:
				break;
			case ShowCardsPanel.CardFilter.FollowCardsOnly:
				goto IL_00B4;
			default:
				goto IL_0102;
			}
			using (IEnumerator<KeyValuePair<Card, CardWidget>> enumerator = this.deckHolder.cards.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<Card, CardWidget> keyValuePair = enumerator.Current;
					keyValuePair.Deconstruct(ref card, ref cardWidget);
					Card card2 = card;
					cardWidget.gameObject.SetActive(card2.IsDreamCard);
				}
				return;
			}
			IL_00B4:
			using (IEnumerator<KeyValuePair<Card, CardWidget>> enumerator = this.deckHolder.cards.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<Card, CardWidget> keyValuePair = enumerator.Current;
					keyValuePair.Deconstruct(ref card, ref cardWidget);
					Card card3 = card;
					cardWidget.gameObject.SetActive(card3.IsFollowCard);
				}
				return;
			}
			IL_0102:
			throw new ArgumentOutOfRangeException("status", status, null);
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x00035080 File Offset: 0x00033280
		private void CardFilterToggle(bool on, ShowCardsPanel.CardFilter filter)
		{
			this.SetCardFilterStatus(on ? filter : ShowCardsPanel.CardFilter.AllCards);
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x0003508F File Offset: 0x0003328F
		protected override void OnShown()
		{
			this.topHideButton.interactable = true;
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x0003509D File Offset: 0x0003329D
		protected override void OnHiding()
		{
			this.topHideButton.interactable = false;
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x000350BD File Offset: 0x000332BD
		protected override void OnHided()
		{
			this.deckHolder.Clear();
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x000350CA File Offset: 0x000332CA
		private void CancelHide()
		{
			if (this._canCancel)
			{
				this.IsCanceled = true;
				base.Hide();
			}
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x000350E1 File Offset: 0x000332E1
		public void OnCancel()
		{
			if (this._canCancel)
			{
				this.CancelHide();
			}
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x000350F1 File Offset: 0x000332F1
		void IInputActionHandler.OnToggleBaseDeck()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00035101 File Offset: 0x00033301
		void IInputActionHandler.OnToggleDrawZone()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x00035111 File Offset: 0x00033311
		void IInputActionHandler.OnToggleDiscardZone()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00035121 File Offset: 0x00033321
		void IInputActionHandler.OnToggleExileZone()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x00035131 File Offset: 0x00033331
		public void RemoveCardsIfContains(IEnumerable<Card> cards)
		{
			this.deckHolder.RemoveCardsIfContains(cards);
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00035140 File Offset: 0x00033340
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<UpgradeCardInteraction>(new InteractionViewer<UpgradeCardInteraction>(this.ViewUpgradeCard));
			base.GameRun.InteractionViewer.Register<RemoveCardInteraction>(new InteractionViewer<RemoveCardInteraction>(this.ViewRemoveCard));
			base.GameRun.InteractionViewer.Register<TransformCardInteraction>(new InteractionViewer<TransformCardInteraction>(this.ViewTransformCard));
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x000351A4 File Offset: 0x000333A4
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<UpgradeCardInteraction>(new InteractionViewer<UpgradeCardInteraction>(this.ViewUpgradeCard));
			base.GameRun.InteractionViewer.Unregister<RemoveCardInteraction>(new InteractionViewer<RemoveCardInteraction>(this.ViewRemoveCard));
			base.GameRun.InteractionViewer.Unregister<TransformCardInteraction>(new InteractionViewer<TransformCardInteraction>(this.ViewTransformCard));
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x00035205 File Offset: 0x00033405
		private IEnumerator ViewUpgradeCard(UpgradeCardInteraction interaction)
		{
			if (interaction.Description != null)
			{
				this.deckHolder.SetTitle("Game.Deck".Localize(true), interaction.Description);
			}
			else
			{
				GameEntity source = interaction.Source;
				if (source != null)
				{
					this.deckHolder.SetTitle("Game.Deck".Localize(true), source.Name + ": " + source.Description);
				}
			}
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.Deck".Localize(true),
				Description = (interaction.Description ?? "Cards.UpgradeTips".Localize(true)),
				Cards = Enumerable.ToList<Card>(interaction.PendingCards),
				CanCancel = interaction.CanCancel,
				InteractionType = InteractionType.Upgrade
			};
			showCardsPayload.CanCancel = interaction.CanCancel;
			yield return this.ShowAsync(showCardsPayload);
			if (this.IsCanceled)
			{
				interaction.Cancel();
			}
			else
			{
				interaction.SelectedCard = this.SelectedCard;
				this.SelectedCard = null;
			}
			yield break;
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x0003521B File Offset: 0x0003341B
		private IEnumerator ViewRemoveCard(RemoveCardInteraction interaction)
		{
			if (interaction.Description != null)
			{
				this.deckHolder.SetTitle("Game.Deck".Localize(true), interaction.Description);
			}
			else
			{
				GameEntity source = interaction.Source;
				if (source != null)
				{
					this.deckHolder.SetTitle("Game.Deck".Localize(true), source.Name + ": " + source.Description);
				}
			}
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.Deck".Localize(true),
				Description = (interaction.Description ?? "Cards.RemoveTips".Localize(true)),
				Cards = Enumerable.ToList<Card>(interaction.PendingCards),
				CanCancel = interaction.CanCancel,
				InteractionType = InteractionType.Remove
			};
			showCardsPayload.CanCancel = interaction.CanCancel;
			yield return this.ShowAsync(showCardsPayload);
			if (this.IsCanceled)
			{
				interaction.Cancel();
			}
			else
			{
				interaction.SelectedCard = this.SelectedCard;
				this.SelectedCard = null;
			}
			yield break;
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x00035231 File Offset: 0x00033431
		private IEnumerator ViewTransformCard(TransformCardInteraction interaction)
		{
			if (interaction.Description != null)
			{
				this.deckHolder.SetTitle("Game.Deck".Localize(true), interaction.Description);
			}
			else
			{
				GameEntity source = interaction.Source;
				if (source != null)
				{
					this.deckHolder.SetTitle("Game.Deck".Localize(true), source.Name + ": " + source.Description);
				}
			}
			this.TransformCard = interaction.TransformCard;
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.Deck".Localize(true),
				Description = (interaction.Description ?? "Cards.TransformTips".Localize(true)),
				Cards = Enumerable.ToList<Card>(interaction.PendingCards),
				CanCancel = interaction.CanCancel,
				InteractionType = InteractionType.Transform
			};
			showCardsPayload.CanCancel = interaction.CanCancel;
			yield return this.ShowAsync(showCardsPayload);
			if (this.IsCanceled)
			{
				interaction.Cancel();
			}
			else
			{
				interaction.SelectedCard = this.SelectedCard;
				this.SelectedCard = null;
			}
			yield break;
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x00035247 File Offset: 0x00033447
		public IEnumerator ShowAsync(ShowCardsPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x0003525D File Offset: 0x0003345D
		public void BeginShowEnemyMoveOrder()
		{
			this.ShowingCardOrder = true;
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x00035266 File Offset: 0x00033466
		public void EndShowEnemyMoveOrder()
		{
			this.ShowingCardOrder = false;
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000A84 RID: 2692 RVA: 0x0003526F File Offset: 0x0003346F
		// (set) Token: 0x06000A85 RID: 2693 RVA: 0x00035277 File Offset: 0x00033477
		private bool ShowingCardOrder
		{
			get
			{
				return this._showingCardOrder;
			}
			set
			{
				if (this._showingCardOrder != value)
				{
					this._showingCardOrder = value;
					this.RefreshCardOrder();
				}
			}
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x0003528F File Offset: 0x0003348F
		private void RefreshCardOrder()
		{
			if (GameMaster.ShowCardOrder || this.ShowingCardOrder)
			{
				this.ShowCardsOrder();
				return;
			}
			this.HideCardsOrder();
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x000352B0 File Offset: 0x000334B0
		private void ShowCardsOrder()
		{
			if (!this.Payload.HideActualOrder)
			{
				List<CardWidget> list = Enumerable.ToList<CardWidget>(this.deckHolder.cards.Values);
				foreach (ValueTuple<int, CardWidget> valueTuple in list.WithIndices<CardWidget>())
				{
					int item = valueTuple.Item1;
					CardWidget item2 = valueTuple.Item2;
					bool flag = false;
					bool flag2 = false;
					ShowCardZone cardZone = this.Payload.CardZone;
					if (cardZone != ShowCardZone.Draw)
					{
						if (cardZone == ShowCardZone.Discard)
						{
							flag = item == list.Count - 1;
							flag2 = item == 0;
						}
					}
					else
					{
						flag = item == 0;
						flag2 = item == list.Count - 1;
					}
					item2.ShowDeckIndex(item + 1, flag, flag2);
				}
			}
		}

		// Token: 0x06000A88 RID: 2696 RVA: 0x0003537C File Offset: 0x0003357C
		private void HideCardsOrder()
		{
			foreach (CardWidget cardWidget in this.deckHolder.cards.Values)
			{
				cardWidget.HideDeckIndex();
			}
		}

		// Token: 0x040007E4 RID: 2020
		[SerializeField]
		private Button returnButton;

		// Token: 0x040007E5 RID: 2021
		[SerializeField]
		private DeckHolder deckHolder;

		// Token: 0x040007E6 RID: 2022
		[SerializeField]
		private Button topHideButton;

		// Token: 0x040007E7 RID: 2023
		[SerializeField]
		private Image portrait;

		// Token: 0x040007E8 RID: 2024
		[SerializeField]
		private CommonToggleWidget actualOrderToggle;

		// Token: 0x040007E9 RID: 2025
		[SerializeField]
		private CommonToggleWidget indexOrderToggle;

		// Token: 0x040007EA RID: 2026
		[SerializeField]
		private CommonToggleWidget typeOrderToggle;

		// Token: 0x040007EB RID: 2027
		[SerializeField]
		private CommonToggleWidget rarityOrderToggle;

		// Token: 0x040007EC RID: 2028
		[SerializeField]
		private CommonToggleWidget followCardOnlyToggle;

		// Token: 0x040007ED RID: 2029
		[SerializeField]
		private CommonToggleWidget dreamCardOnlyToggle;

		// Token: 0x040007EE RID: 2030
		[SerializeField]
		private AssociationList<string, Sprite> characterPortraits;

		// Token: 0x040007EF RID: 2031
		[SerializeField]
		private GameObject minimizedButton;

		// Token: 0x040007F0 RID: 2032
		private int _currentCharacterIndex;

		// Token: 0x040007F1 RID: 2033
		private bool _canCancel;

		// Token: 0x040007F2 RID: 2034
		private InteractionType _showType;

		// Token: 0x040007F7 RID: 2039
		private CanvasGroup _canvasGroup;

		// Token: 0x040007F8 RID: 2040
		private bool _showingCardOrder;

		// Token: 0x040007F9 RID: 2041
		private ShowCardsPanel.CardFilter _cardFilterStatus;

		// Token: 0x020002C6 RID: 710
		private enum OrderStatus
		{
			// Token: 0x0400123E RID: 4670
			Actual,
			// Token: 0x0400123F RID: 4671
			Index,
			// Token: 0x04001240 RID: 4672
			Type,
			// Token: 0x04001241 RID: 4673
			Rarity
		}

		// Token: 0x020002C7 RID: 711
		private enum CardFilter
		{
			// Token: 0x04001243 RID: 4675
			AllCards,
			// Token: 0x04001244 RID: 4676
			DreamCardsOnly,
			// Token: 0x04001245 RID: 4677
			FollowCardsOnly
		}
	}
}
