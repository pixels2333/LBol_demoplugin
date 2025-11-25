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
	public class ShowCardsPanel : UiPanel<ShowCardsPayload>, IInputActionHandler
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		public bool IsCanceled { get; private set; }
		public Card SelectedCard { get; private set; }
		public Card TransformCard { get; private set; }
		private ShowCardsPayload Payload { get; set; }
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
		public override void OnLocaleChanged()
		{
			this.deckHolder.OnLocaleChanged();
		}
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
		private void CardOrderToggle(bool on, ShowCardsPanel.OrderStatus orderStatus)
		{
			if (!on)
			{
				return;
			}
			this.SetCards(orderStatus, false);
		}
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
		private void CardFilterToggle(bool on, ShowCardsPanel.CardFilter filter)
		{
			this.SetCardFilterStatus(on ? filter : ShowCardsPanel.CardFilter.AllCards);
		}
		protected override void OnShown()
		{
			this.topHideButton.interactable = true;
		}
		protected override void OnHiding()
		{
			this.topHideButton.interactable = false;
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		protected override void OnHided()
		{
			this.deckHolder.Clear();
		}
		private void CancelHide()
		{
			if (this._canCancel)
			{
				this.IsCanceled = true;
				base.Hide();
			}
		}
		public void OnCancel()
		{
			if (this._canCancel)
			{
				this.CancelHide();
			}
		}
		void IInputActionHandler.OnToggleBaseDeck()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}
		void IInputActionHandler.OnToggleDrawZone()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}
		void IInputActionHandler.OnToggleDiscardZone()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}
		void IInputActionHandler.OnToggleExileZone()
		{
			if (this._showType == InteractionType.None)
			{
				base.Hide();
			}
		}
		public void RemoveCardsIfContains(IEnumerable<Card> cards)
		{
			this.deckHolder.RemoveCardsIfContains(cards);
		}
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<UpgradeCardInteraction>(new InteractionViewer<UpgradeCardInteraction>(this.ViewUpgradeCard));
			base.GameRun.InteractionViewer.Register<RemoveCardInteraction>(new InteractionViewer<RemoveCardInteraction>(this.ViewRemoveCard));
			base.GameRun.InteractionViewer.Register<TransformCardInteraction>(new InteractionViewer<TransformCardInteraction>(this.ViewTransformCard));
		}
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<UpgradeCardInteraction>(new InteractionViewer<UpgradeCardInteraction>(this.ViewUpgradeCard));
			base.GameRun.InteractionViewer.Unregister<RemoveCardInteraction>(new InteractionViewer<RemoveCardInteraction>(this.ViewRemoveCard));
			base.GameRun.InteractionViewer.Unregister<TransformCardInteraction>(new InteractionViewer<TransformCardInteraction>(this.ViewTransformCard));
		}
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
		public IEnumerator ShowAsync(ShowCardsPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}
		public void BeginShowEnemyMoveOrder()
		{
			this.ShowingCardOrder = true;
		}
		public void EndShowEnemyMoveOrder()
		{
			this.ShowingCardOrder = false;
		}
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
		private void RefreshCardOrder()
		{
			if (GameMaster.ShowCardOrder || this.ShowingCardOrder)
			{
				this.ShowCardsOrder();
				return;
			}
			this.HideCardsOrder();
		}
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
		private void HideCardsOrder()
		{
			foreach (CardWidget cardWidget in this.deckHolder.cards.Values)
			{
				cardWidget.HideDeckIndex();
			}
		}
		[SerializeField]
		private Button returnButton;
		[SerializeField]
		private DeckHolder deckHolder;
		[SerializeField]
		private Button topHideButton;
		[SerializeField]
		private Image portrait;
		[SerializeField]
		private CommonToggleWidget actualOrderToggle;
		[SerializeField]
		private CommonToggleWidget indexOrderToggle;
		[SerializeField]
		private CommonToggleWidget typeOrderToggle;
		[SerializeField]
		private CommonToggleWidget rarityOrderToggle;
		[SerializeField]
		private CommonToggleWidget followCardOnlyToggle;
		[SerializeField]
		private CommonToggleWidget dreamCardOnlyToggle;
		[SerializeField]
		private AssociationList<string, Sprite> characterPortraits;
		[SerializeField]
		private GameObject minimizedButton;
		private int _currentCharacterIndex;
		private bool _canCancel;
		private InteractionType _showType;
		private CanvasGroup _canvasGroup;
		private bool _showingCardOrder;
		private ShowCardsPanel.CardFilter _cardFilterStatus;
		private enum OrderStatus
		{
			Actual,
			Index,
			Type,
			Rarity
		}
		private enum CardFilter
		{
			AllCards,
			DreamCardsOnly,
			FollowCardsOnly
		}
	}
}
