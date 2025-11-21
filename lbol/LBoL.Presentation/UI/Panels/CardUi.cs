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
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200008D RID: 141
	public class CardUi : MonoBehaviour
	{
		// Token: 0x17000134 RID: 308
		// (get) Token: 0x0600072C RID: 1836 RVA: 0x00021880 File Offset: 0x0001FA80
		// (set) Token: 0x0600072D RID: 1837 RVA: 0x0002189F File Offset: 0x0001FA9F
		private BattleController Battle
		{
			get
			{
				BattleController battleController;
				if (!this._battle.TryGetTarget(ref battleController))
				{
					return null;
				}
				return battleController;
			}
			set
			{
				this._battle.SetTarget(value);
			}
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x000218AD File Offset: 0x0001FAAD
		public IEnumerable<HandCard> GetUnpendingHands()
		{
			return Enumerable.Where<HandCard>(this._handWidgets, (HandCard hand) => !hand.PendingUse);
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x000218DC File Offset: 0x0001FADC
		public void SetPendingCardsAlpha(float alpha)
		{
			foreach (HandCard handCard in this._pendingUseWidgets)
			{
				handCard.CanvasGroup.alpha = alpha;
			}
			foreach (CardWidget cardWidget in this._followPlayWidgets)
			{
				cardWidget.CanvasGroup.alpha = alpha;
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x06000730 RID: 1840 RVA: 0x00021978 File Offset: 0x0001FB78
		public int UnpendingHandCount
		{
			get
			{
				return Enumerable.Count<HandCard>(this._handWidgets, (HandCard hand) => !hand.PendingUse);
			}
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x000219A4 File Offset: 0x0001FBA4
		private CardWidget CreateCardWidget(Card card, Transform parent)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardPrefab, parent);
			cardWidget.gameObject.name = "Card: " + card.Id;
			cardWidget.Card = card;
			return cardWidget;
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x000219D4 File Offset: 0x0001FBD4
		private CardWidget CreateCardWidget(Card card)
		{
			return this.CreateCardWidget(card, this.cardHandParent);
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x000219E3 File Offset: 0x0001FBE3
		private HandCard CreateHandWidget(Card card)
		{
			return this.ConvertToHand(this.CreateCardWidget(card));
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x000219F4 File Offset: 0x0001FBF4
		private HandCard ConvertToHand(CardWidget cardWidget)
		{
			HandCard handCard = Object.Instantiate<HandCard>(this.handCardPrefab, cardWidget.transform.parent);
			handCard.gameObject.name = "Hand:" + cardWidget.Card.Id;
			Transform transform = handCard.transform;
			Transform transform2 = cardWidget.transform;
			transform.localPosition = transform2.localPosition;
			transform.localScale = transform2.localScale;
			transform.localRotation = transform2.localRotation;
			handCard.CardWidget = cardWidget;
			transform2.SetParent(handCard.cardRoot);
			transform2.localPosition = Vector3.zero;
			transform2.localScale = Vector3.one;
			cardWidget.ShowManaHand = true;
			handCard.NormalParent = this.cardHandParent;
			handCard.HoveredParent = this.cardHoveredParent;
			handCard.ActiveHandParent = this._playBoard.ActiveHandParent;
			handCard.SpecialReactingPosition = Vector3.zero;
			handCard.SpecialReactingRotation = Quaternion.identity;
			if (handCard.transform.parent != this.cardHandParent)
			{
				handCard.transform.SetParent(this.cardHandParent);
			}
			return handCard;
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x00021B04 File Offset: 0x0001FD04
		public HandCard FindHandWidget(Card card)
		{
			return Enumerable.FirstOrDefault<HandCard>(this._handWidgets, (HandCard hand) => hand.Card == card);
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x00021B38 File Offset: 0x0001FD38
		public CardWidget FindFollowPlayWidget(Card card)
		{
			return Enumerable.FirstOrDefault<CardWidget>(this._followPlayWidgets, (CardWidget c) => c.Card == card);
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x00021B6C File Offset: 0x0001FD6C
		public CardWidget FindViewWidget(Card card)
		{
			return Enumerable.FirstOrDefault<CardWidget>(this._viewWidget, (CardWidget c) => c.Card == card);
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x00021BA0 File Offset: 0x0001FDA0
		private HandCard ExtractHandWidget(Card card)
		{
			HandCard handCard = this.FindHandWidget(card);
			if (handCard != null)
			{
				if (handCard.PendingUse)
				{
					if (!this._pendingUseWidgets.Remove(handCard))
					{
						Debug.LogWarning("[CardUi] Removing pending use " + card.DebugName + " from pendingWidgets failed");
					}
					else
					{
						handCard.CanvasGroup.alpha = 1f;
					}
				}
				if (!this._handWidgets.Remove(handCard))
				{
					Debug.LogWarning("[CardUi] Removing " + card.DebugName + " from handWidgets failed");
				}
			}
			return handCard;
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x00021C2C File Offset: 0x0001FE2C
		private static void FastRemoveHand(HandCard hand)
		{
			hand.transform.DOLocalMoveY(-100f, 0.2f, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>().SetLink(hand.gameObject)
				.OnComplete(delegate
				{
					Object.Destroy(hand.gameObject);
				});
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x00021C88 File Offset: 0x0001FE88
		private static void FastRemoveCard(CardWidget card)
		{
			card.transform.DOLocalMoveY(-100f, 0.2f, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>().SetLink(card.gameObject)
				.OnComplete(delegate
				{
					Object.Destroy(card.gameObject);
				});
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x00021CE4 File Offset: 0x0001FEE4
		public void CancelUse(Card card)
		{
			HandCard handCard = this.FindHandWidget(card);
			if (handCard != null)
			{
				handCard.IsActiveHand = false;
				handCard.PendingUse = false;
				if (!this._pendingUseWidgets.Remove(handCard))
				{
					Debug.LogError("[CardUi] Remove hand " + card.DebugName + " from pendingUseWidgets failed.");
				}
				else
				{
					handCard.CanvasGroup.alpha = 1f;
				}
				this.AdjustPendingCardsLocation();
				this.AdjustCardsPosition(false);
				handCard.CancelUse();
			}
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x00021D60 File Offset: 0x0001FF60
		public void CardReturnToHand(Card card)
		{
			HandCard handCard = this.FindHandWidget(card);
			if (handCard != null)
			{
				handCard.SetToInactiveWithoutAudio();
				handCard.PendingUse = false;
				if (!this._pendingUseWidgets.Remove(handCard))
				{
					Debug.LogError("[CardUi] Remove hand " + card.DebugName + " from pendingUseWidgets failed.");
				}
				else
				{
					handCard.CanvasGroup.alpha = 1f;
				}
				this.AdjustPendingCardsLocation();
				this.AdjustCardsPosition(true);
				handCard.ReturnToHand();
			}
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x00021DD8 File Offset: 0x0001FFD8
		public void PlaySummonEffect(Card card)
		{
			HandCard handCard = this.FindHandWidget(card);
			if (handCard != null)
			{
				handCard.Summon();
				return;
			}
			CardWidget cardWidget = this.FindFollowPlayWidget(card);
			if (cardWidget != null)
			{
				cardWidget.Summon();
			}
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x00021E14 File Offset: 0x00020014
		private IEnumerator InternalViewDrawCard(Card card, CardTransitionType transitionType)
		{
			if (transitionType == CardTransitionType.SpecialEnd)
			{
				HandCard endingHand = this.FindHandWidget(card);
				if (endingHand)
				{
					yield return new WaitForSecondsRealtime(0.3f);
					endingHand.SpecialReacting = false;
					this.AdjustCardsPosition(false);
					this.RefreshAllCardsEdge();
				}
				yield break;
			}
			if (transitionType != CardTransitionType.Normal && transitionType != CardTransitionType.SpecialBegin)
			{
				Debug.LogError(string.Format("[{0}] Cannot recognize transition type of {1}: {2}", "CardUi", "DrawCardAction", transitionType));
			}
			this.RefreshDrawCount();
			AudioManager.PlayUi("CardDraw", false);
			HandCard handCard = this.CreateHandWidget(card);
			Transform transform = handCard.transform;
			transform.localPosition = this.cardDrawPoint.localPosition;
			transform.localScale = this.cardDrawPoint.localScale;
			this._handWidgets.Add(handCard);
			if (transitionType == CardTransitionType.Normal)
			{
				this.AdjustCardsPosition(false);
				this.RefreshAllCardsEdge();
			}
			else
			{
				handCard.SpecialReacting = true;
			}
			this.RefreshAll();
			yield break;
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x00021E31 File Offset: 0x00020031
		private IEnumerator SimpleCardFlyEffect(CardZone from, CardZone to, float time = 0.2f)
		{
			Vector3 vector;
			switch (from)
			{
			case CardZone.Draw:
				vector = this.DrawLocalPosition;
				goto IL_007F;
			case CardZone.Discard:
				vector = this.DiscardLocalPosition;
				goto IL_007F;
			case CardZone.Exile:
				vector = this.ExileLocalPosition;
				goto IL_007F;
			}
			throw new ArgumentOutOfRangeException("from", from, null);
			IL_007F:
			Vector3 vector2 = vector;
			switch (to)
			{
			case CardZone.Draw:
				vector = this.DrawLocalPosition;
				goto IL_00DA;
			case CardZone.Discard:
				vector = this.DiscardLocalPosition;
				goto IL_00DA;
			case CardZone.Exile:
				vector = this.ExileLocalPosition;
				goto IL_00DA;
			}
			throw new ArgumentOutOfRangeException("from", from, null);
			IL_00DA:
			Vector3 vector3 = vector;
			Transform parent = Object.Instantiate<Transform>(this.cardFlyHelperPrefab, this.cardEffectLayer);
			parent.localPosition = vector2;
			CardFlyBrief clone = Object.Instantiate<CardFlyBrief>(this.cardFlyBrief, parent);
			parent.DOLocalMove(vector3, time, false).SetEase(Ease.InSine).SetLink(parent.gameObject)
				.OnComplete(delegate
				{
					clone.CloseCard();
					Object.Destroy(parent.gameObject, 0.5f);
				});
			yield return new WaitForSecondsRealtime(0.05f);
			yield break;
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x00021E55 File Offset: 0x00020055
		private IEnumerator ShowCardMoveRunner(Card card, int index, int total)
		{
			if (index > 10)
			{
				yield return this.SimpleCardFlyEffect(CardZone.Draw, CardZone.Discard, 0.2f);
			}
			else
			{
				Vector3 drawLocalPosition = this.DrawLocalPosition;
				Vector3 endPosition = this.DiscardLocalPosition;
				float num = 600f;
				int num2 = Math.Min(10, total);
				if (total > 5)
				{
					num = 2400f / (float)num2;
				}
				CardWidget cardWidget = this.CreateCardWidget(card, this.cardEffectLayer);
				Transform trans = cardWidget.transform;
				trans.localPosition = drawLocalPosition;
				trans.localScale = new Vector3(0.2f, 0.2f);
				AudioManager.PlayUi("CardAppear", false);
				DOTween.Sequence().Join(trans.DOLocalMove(new Vector3(num * ((float)index - (float)(num2 - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine)
					.Join(trans.DOScale(Vector3.one, 0.3f))
					.AppendInterval(0.3f)
					.SetUpdate(true)
					.OnComplete(delegate
					{
						AudioManager.PlayUi("CardFly", false);
						Transform parent = Object.Instantiate<Transform>(this.cardFlyHelperPrefab, this.cardEffectLayer);
						parent.localPosition = trans.localPosition;
						trans.SetParent(parent);
						GameObject gameObject = Object.Instantiate<GameObject>(this.cardFlyTrail, parent);
						int num3 = Random.Range(200, 400);
						DOTween.Sequence().Join(parent.DOLocalMove(endPosition, 0.4f, false).SetEase(Ease.InSine)).Join(trans.DOLocalRotate(new Vector3(0f, 0f, -500f), 0.2f, RotateMode.FastBeyond360))
							.Join(trans.DOScale(new Vector3(0.2f, 0.2f, 1f), 0.2f))
							.Join(trans.DOLocalMoveY((float)num3, 0.2f, false).SetEase(Ease.OutSine).SetRelative(true)
								.SetLoops(2, LoopType.Yoyo))
							.Join(gameObject.transform.DOLocalMoveY((float)num3, 0.2f, false).SetEase(Ease.OutSine).SetRelative(true)
								.SetLoops(2, LoopType.Yoyo))
							.SetUpdate(true)
							.OnComplete(delegate
							{
								Object.Destroy(cardWidget.gameObject);
								CardUi.ImageBlink(this.discardButton.image);
								this.RefreshDiscardCount();
								Object.Destroy(parent.gameObject, 0.2f);
							});
					});
				yield return new WaitForSecondsRealtime(0.05f);
			}
			yield break;
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x00021E79 File Offset: 0x00020079
		private IEnumerator ViewDrawCard(DrawCardAction action)
		{
			return this.InternalViewDrawCard(action.Args.Card, action.TransitionType);
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x00021E92 File Offset: 0x00020092
		private IEnumerator ViewDrawSelectedCard(DrawSelectedCardAction action)
		{
			return this.InternalViewDrawCard(action.Args.Card, action.TransitionType);
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x00021EAB File Offset: 0x000200AB
		private IEnumerator ViewDiscard(DiscardAction action)
		{
			Card card = action.Args.Card;
			CardZone sourceZone = action.SourceZone;
			if (action.TransitionType == CardTransitionType.Normal)
			{
				if (sourceZone == CardZone.Hand || sourceZone == CardZone.PlayArea)
				{
					HandCard handCard = this.ExtractHandWidget(card);
					if (handCard != null)
					{
						yield return this.MoveCardToDiscard(handCard, action.Cause != ActionCause.TurnEnd);
					}
					else
					{
						Debug.LogError("[CardUi] Discarding hand card " + card.DebugName + ": widget not found.");
					}
				}
				else
				{
					yield return this.SimpleCardFlyEffect(action.SourceZone, CardZone.Discard, 0.2f);
				}
			}
			else if (action.TransitionType == CardTransitionType.SpecialBegin)
			{
				if (sourceZone == CardZone.Hand || sourceZone == CardZone.PlayArea)
				{
					HandCard handCard2 = this.ExtractHandWidget(card);
					if (handCard2 != null)
					{
						this._specialReactingWidgets.Add(handCard2);
						handCard2.SpecialReacting = true;
						yield return new WaitForSecondsRealtime(0.3f);
					}
					else
					{
						Debug.LogError("[CardUi] Discarding hand card " + card.DebugName + ": widget not found.");
					}
				}
				else
				{
					Debug.Log("[CardUi] Discarding '" + card.DebugName + "' special reacting, visual not implemented");
				}
			}
			else if (action.TransitionType == CardTransitionType.SpecialEnd)
			{
				HandCard handCard3 = Enumerable.FirstOrDefault<HandCard>(this._specialReactingWidgets, (HandCard w) => w.Card == card);
				if (handCard3 != null)
				{
					this._specialReactingWidgets.Remove(handCard3);
					yield return this.MoveCardToDiscard(handCard3, action.Cause != ActionCause.TurnEnd);
				}
				else
				{
					Debug.Log("[CardUi] Discarding '" + card.DebugName + "' special reacted: widget not found.");
				}
			}
			this.RefreshAll();
			yield break;
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x00021EC1 File Offset: 0x000200C1
		private IEnumerator MoveCardToDrawZone(HandCard hand)
		{
			CardUi.<>c__DisplayClass35_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass35_0();
			CS$<>8__locals1.hand = hand;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.hand != null)
			{
				this._playBoard.CancelTargetSelectingIfCardIs(CS$<>8__locals1.hand);
				base.StartCoroutine(CS$<>8__locals1.<MoveCardToDrawZone>g__PlayDiscardEffect|0());
				this.AdjustCardsPosition(false);
				this.RefreshAllCardsEdge();
			}
			yield return new WaitForSecondsRealtime(0.05f);
			this.RefreshAll();
			this.RefreshDrawCount();
			yield break;
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x00021ED7 File Offset: 0x000200D7
		private IEnumerator MoveCardToDrawZone(CardWidget widget)
		{
			CardUi.<>c__DisplayClass36_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass36_0();
			CS$<>8__locals1.widget = widget;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.widget != null)
			{
				if (this._followPlayWidgets.Contains(CS$<>8__locals1.widget))
				{
					this._followPlayWidgets.Remove(CS$<>8__locals1.widget);
				}
				base.StartCoroutine(CS$<>8__locals1.<MoveCardToDrawZone>g__PlayDiscardEffect|0());
			}
			yield return new WaitForSecondsRealtime(0.05f);
			this.RefreshDrawCount();
			yield break;
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00021EED File Offset: 0x000200ED
		private IEnumerator MoveCardToDiscard(HandCard hand, bool adjustCardsPosition)
		{
			CardUi.<>c__DisplayClass37_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass37_0();
			CS$<>8__locals1.hand = hand;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.hand != null)
			{
				this._playBoard.CancelTargetSelectingIfCardIs(CS$<>8__locals1.hand);
				base.StartCoroutine(CS$<>8__locals1.<MoveCardToDiscard>g__PlayDiscardEffect|0());
				this.AdjustPendingCardsLocation();
				if (adjustCardsPosition)
				{
					this.AdjustCardsPosition(false);
				}
				else
				{
					this.DelayedAdjustCardsPosition(0.4f);
				}
				this.RefreshAllCardsEdge();
			}
			yield return new WaitForSecondsRealtime(0.05f);
			this.RefreshAll();
			this.RefreshDiscardCount();
			yield break;
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x00021F0A File Offset: 0x0002010A
		private IEnumerator MoveCardToDiscard(CardWidget widget)
		{
			CardUi.<>c__DisplayClass38_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass38_0();
			CS$<>8__locals1.widget = widget;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.widget != null)
			{
				if (this._followPlayWidgets.Contains(CS$<>8__locals1.widget))
				{
					this._followPlayWidgets.Remove(CS$<>8__locals1.widget);
				}
				base.StartCoroutine(CS$<>8__locals1.<MoveCardToDiscard>g__PlayDiscardEffect|0());
			}
			yield return new WaitForSecondsRealtime(0.05f);
			this.RefreshDiscardCount();
			yield break;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x00021F20 File Offset: 0x00020120
		private IEnumerator ViewExileCard(ExileCardAction action)
		{
			Card card = action.Args.Card;
			CardZone sourceZone = action.SourceZone;
			if (action.TransitionType == CardTransitionType.Normal)
			{
				if (sourceZone == CardZone.Hand || sourceZone == CardZone.PlayArea)
				{
					HandCard handCard = this.ExtractHandWidget(action.Args.Card);
					if (handCard != null)
					{
						this._playBoard.CancelTargetSelectingIfCardIs(handCard);
						handCard.IsDisappearing = true;
						handCard.Exile();
						this.AdjustPendingCardsLocation();
						this.DelayedAdjustCardsPosition(0.4f);
						this.RefreshAllCardsEdge();
						yield return new WaitForSecondsRealtime(0.05f);
					}
					else
					{
						Debug.LogError(string.Format("[CardUi] Exiling '{0}' from {1}: widget not found.", card.DebugName, sourceZone));
					}
				}
				else if (sourceZone == CardZone.FollowArea)
				{
					CardWidget cardWidget2 = this.FindFollowPlayWidget(card);
					if (cardWidget2 != null)
					{
						cardWidget2.Exile();
						this._followPlayWidgets.Remove(cardWidget2);
						yield return new WaitForSecondsRealtime(0.05f);
					}
					else
					{
						Debug.LogError(string.Format("[CardUi] Exiling '{0}' from {1}: widget not found.", card.DebugName, sourceZone));
					}
				}
				else
				{
					Sequence sequence = DOTween.Sequence();
					HandCard cardWidget = this.CreateHandWidget(card);
					Transform transform = cardWidget.transform;
					Vector3 vector;
					if (sourceZone != CardZone.Draw)
					{
						if (sourceZone != CardZone.Discard)
						{
							vector = CameraController.ScenePositionToWorldPositionInUI(Vector3.zero);
						}
						else
						{
							vector = this.DiscardPosition;
						}
					}
					else
					{
						vector = this.DrawPosition;
					}
					Vector3 vector2 = vector;
					transform.position = vector2;
					transform.localScale = new Vector3(0.2f, 0.2f);
					sequence.Join(transform.DOScale(Vector3.one, 0.3f));
					cardWidget.IsDisappearing = true;
					cardWidget.CardWidget.ShowManaHand = false;
					if (action.ManyCardAction != null)
					{
						int i = action.ManyCardAction.Cards.IndexOf(card);
						int num = action.ManyCardAction.Cards.Length;
						int num2 = 600;
						float num3 = 0.3f;
						if (num > 11)
						{
							num3 = 0.075f;
							int num4 = 0;
							for (i = 0; i < 5; i++)
							{
								num4 += Random.Range(-3, 3);
							}
							transform.localEulerAngles = new Vector3(0f, 0f, (float)num4);
							sequence.Join(transform.DOLocalMove(new Vector3((float)Random.Range(-1500, 1500), (float)Random.Range(-600, 600)), 0.3f, false)).SetEase(Ease.OutSine);
						}
						else if (num > 4)
						{
							num2 = 600 - (num - 4) * 40;
							num3 = 0.15f;
							sequence.Join(transform.DOLocalMove(new Vector3((float)num2 * ((float)i - (float)(num - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
						}
						else
						{
							sequence.Join(transform.DOLocalMove(new Vector3((float)num2 * ((float)i - (float)(num - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
						}
						yield return new WaitForSecondsRealtime(num3);
					}
					else
					{
						sequence.Join(transform.DOLocalMove(Vector3.zero, 0.3f, false)).SetEase(Ease.OutSine);
						yield return sequence.AppendInterval(0.3f).SetUpdate(true).WaitForCompletion();
					}
					cardWidget.Exile();
					cardWidget = null;
				}
				this.RefreshAll();
				this.RefreshAllZoneCount();
			}
			else if (action.TransitionType == CardTransitionType.SpecialBegin)
			{
				if (sourceZone == CardZone.Hand || sourceZone == CardZone.PlayArea)
				{
					HandCard handCard2 = this.ExtractHandWidget(action.Args.Card);
					if (handCard2 != null)
					{
						this._playBoard.CancelTargetSelectingIfCardIs(handCard2);
						handCard2.SpecialReacting = true;
						this._specialReactingWidgets.Add(handCard2);
						yield return new WaitForSecondsRealtime(0.3f);
					}
				}
				else
				{
					Debug.Log(string.Format("[CardUi] Exiling '{0}' from {1} special reacting, visual not implemented", card.DebugName, sourceZone));
				}
				this.RefreshAll();
				this.RefreshAllZoneCount();
			}
			else if (action.TransitionType == CardTransitionType.SpecialEnd)
			{
				if (sourceZone == CardZone.Hand || sourceZone == CardZone.PlayArea)
				{
					HandCard cardWidget = Enumerable.FirstOrDefault<HandCard>(this._specialReactingWidgets, (HandCard w) => w.Card == action.Args.Card);
					if (cardWidget != null)
					{
						cardWidget.IsDisappearing = true;
						cardWidget.SpecialReacting = false;
						cardWidget.Exile();
						this.AdjustPendingCardsLocation();
						this.AdjustCardsPosition(false);
						this.RefreshAllCardsEdge();
						yield return new WaitForSecondsRealtime(0.05f);
						this._specialReactingWidgets.Remove(cardWidget);
					}
					else
					{
						Debug.LogError(string.Format("[CardUi] Exiling '{0}' from {1}: widget not found.", card.DebugName, sourceZone));
					}
					cardWidget = null;
				}
				else
				{
					Debug.Log(string.Format("[CardUi] Exiling '{0}' from {1} special reacted, visual not implemented", action.Args.Card.DebugName, sourceZone));
				}
				this.RefreshAll();
			}
			yield break;
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x00021F36 File Offset: 0x00020136
		private IEnumerator ViewRemoveCard(RemoveCardAction action)
		{
			Card card = action.Args.Card;
			HandCard handCard = this.ExtractHandWidget(card);
			if (handCard != null)
			{
				this._playBoard.CancelTargetSelectingIfCardIs(handCard);
				handCard.Remove();
				this.AdjustPendingCardsLocation();
				this.AdjustCardsPosition(false);
				this.RefreshAllCardsEdge();
				yield return new WaitForSecondsRealtime(0.1f);
			}
			else
			{
				CardWidget cardWidget = this.FindFollowPlayWidget(card);
				if (cardWidget != null)
				{
					cardWidget.Remove();
					this._followPlayWidgets.Remove(cardWidget);
					yield return new WaitForSecondsRealtime(0.05f);
				}
			}
			this.RefreshAll();
			this.RefreshAllZoneCount();
			yield break;
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x00021F4C File Offset: 0x0002014C
		private IEnumerator ViewTransformCard(TransformCardAction action)
		{
			switch (action.Args.DestinationCard.Zone)
			{
			case CardZone.Draw:
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.pileTransformEffect, this.drawButton.transform);
				gameObject.GetComponentInChildren<ParticleSystem>().Play(true);
				Object.Destroy(gameObject, 3f);
				break;
			}
			case CardZone.Hand:
			{
				HandCard handCard = this.FindHandWidget(action.Args.SourceCard);
				if (handCard != null)
				{
					this._playBoard.CancelTargetSelectingIfCardIs(handCard);
					handCard.CardWidget.Card = action.Args.DestinationCard;
					handCard.TransformEffect();
					this.AdjustPendingCardsLocation();
					this.AdjustCardsPosition(false);
					this.RefreshAllCardsEdge();
					yield return new WaitForSecondsRealtime(0.1f);
				}
				this.RefreshAll();
				this.RefreshAllZoneCount();
				break;
			}
			case CardZone.Discard:
			{
				GameObject gameObject2 = Object.Instantiate<GameObject>(this.pileTransformEffect, this.discardButton.transform);
				gameObject2.GetComponentInChildren<ParticleSystem>().Play(true);
				Object.Destroy(gameObject2, 3f);
				break;
			}
			}
			yield break;
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00021F62 File Offset: 0x00020162
		private IEnumerator ViewRetain(RetainAction action)
		{
			Card card = action.Args.Card;
			if (action.TransitionType != CardTransitionType.Normal)
			{
				if (action.TransitionType == CardTransitionType.SpecialBegin)
				{
					HandCard handCard = this.FindHandWidget(card);
					if (handCard == null)
					{
						Debug.LogError("[CardUi] Cannot get retaining card " + card.DebugName + " from hand");
					}
					handCard.SpecialReacting = true;
					this._specialReactingWidgets.Add(handCard);
					yield return new WaitForSecondsRealtime(0.3f);
				}
				else if (action.TransitionType == CardTransitionType.SpecialEnd)
				{
					HandCard widget = Enumerable.FirstOrDefault<HandCard>(this._specialReactingWidgets, (HandCard w) => w.Card == action.Args.Card);
					if (widget != null)
					{
						yield return new WaitForSecondsRealtime(0.3f);
						widget.SpecialReacting = false;
						this._specialReactingWidgets.Remove(widget);
					}
					widget = null;
				}
			}
			yield break;
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00021F78 File Offset: 0x00020178
		private IEnumerator ViewReshuffle(ReshuffleAction action)
		{
			if (GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("EmptyDrawZone"))
			{
				yield return UiManager.GetPanel<HintPanel>().ShowAsync(new HintPayload
				{
					HintKey = "EmptyDrawZone"
				});
			}
			if (this.DiscardCount > 0)
			{
				AudioManager.PlayUi("CardReshuffle", false);
				int count = Math.Min(this.DiscardCount, 10);
				int num2;
				for (int i = 0; i < count; i = num2 + 1)
				{
					AudioManager.PlayUi("CardFly", false);
					Transform parent = Object.Instantiate<Transform>(this.cardFlyHelperPrefab, this.cardEffectLayer);
					parent.localPosition = this.DiscardLocalPosition;
					CardFlyBrief clone = Object.Instantiate<CardFlyBrief>(this.cardFlyBrief, parent);
					Transform transform = clone.transform;
					parent.DOLocalMove(this.DrawLocalPosition, 0.5f, false).SetEase(Ease.InSine).OnComplete(delegate
					{
						clone.CloseCard();
						Object.Destroy(parent.gameObject, 0.5f);
					});
					int num = Random.Range(100, 300);
					transform.DOLocalMoveY((float)num, 0.25f, false).SetEase(Ease.OutSine).SetRelative(true)
						.SetLoops(2, LoopType.Yoyo);
					yield return new WaitForSecondsRealtime(0.05f);
					num2 = i;
				}
				yield return new WaitForSecondsRealtime(0.3f);
				this.RefreshAllZoneCount();
			}
			this.RefreshAll();
			yield break;
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x00021F87 File Offset: 0x00020187
		private IEnumerator ViewAddCardsToHand(AddCardsToHandAction action)
		{
			CardUi.<>c__DisplayClass45_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass45_0();
			CS$<>8__locals1.<>4__this = this;
			AddCardsType presentationType = action.PresentationType;
			float num;
			if (presentationType != AddCardsType.Normal)
			{
				if (presentationType != AddCardsType.OneByOne)
				{
					throw new ArgumentOutOfRangeException();
				}
				num = 0.25f;
			}
			else
			{
				num = 0.05f;
			}
			CS$<>8__locals1.interval = num;
			Vector3? vector = this._playBoard.FindActionSourceWorldPosition(action.Source);
			List<Card> list = Enumerable.ToList<Card>(action.Args.Cards);
			CS$<>8__locals1.list = new List<CardWidget>();
			Vector3 drawLocalPosition = this.DrawLocalPosition;
			CS$<>8__locals1.endPosition = this.DiscardLocalPosition;
			Sequence sequence = DOTween.Sequence();
			int count = list.Count;
			float num2 = 600f;
			if (count > 5)
			{
				num2 = 2400f / (float)count;
			}
			AudioManager.PlayUi("CardAppear", false);
			for (int i = 0; i < count; i++)
			{
				Card card = list[i];
				CardWidget cardWidget = this.CreateCardWidget(card, this._rectTransform);
				CS$<>8__locals1.list.Add(cardWidget);
				Transform transform = cardWidget.transform;
				if (vector != null)
				{
					Vector3 valueOrDefault = vector.GetValueOrDefault();
					transform.position = valueOrDefault;
				}
				else
				{
					transform.localPosition = drawLocalPosition;
				}
				cardWidget.gameObject.SetActive(false);
				transform.localScale = new Vector3(0.2f, 0.2f);
				float num3 = CS$<>8__locals1.interval * (float)i;
				sequence.InsertCallback(num3, delegate
				{
					cardWidget.gameObject.SetActive(true);
				});
				sequence.Insert(num3, transform.DOLocalMove(new Vector3(num2 * ((float)i - (float)(count - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
				sequence.Insert(num3, transform.DOScale(Vector3.one, 0.3f));
			}
			yield return sequence.AppendInterval(0.3f).SetUpdate(true).WaitForCompletion();
			CS$<>8__locals1.failList = new List<CardWidget>();
			foreach (CardWidget cardWidget2 in CS$<>8__locals1.list)
			{
				if (cardWidget2.Card.Zone == CardZone.Hand)
				{
					this._handWidgets.Add(this.ConvertToHand(cardWidget2));
				}
				else
				{
					CS$<>8__locals1.failList.Add(cardWidget2);
				}
			}
			this.AdjustCardsPosition(false);
			this.RefreshAll();
			this.RefreshAllCardsEdge();
			CardUi.CheckKickerPrefer(action.Args.Cards);
			if (CS$<>8__locals1.failList.Count > 0)
			{
				base.StartCoroutine(CS$<>8__locals1.<ViewAddCardsToHand>g__EndRunner|0());
			}
			yield break;
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00021F9D File Offset: 0x0002019D
		private IEnumerator ViewAddCardsToDrawZone(AddCardsToDrawZoneAction action)
		{
			CardUi.<>c__DisplayClass53_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass53_0();
			CS$<>8__locals1.<>4__this = this;
			AddCardsType addCardsType = action.PresentationType;
			float num;
			if (addCardsType != AddCardsType.Normal)
			{
				if (addCardsType != AddCardsType.OneByOne)
				{
					throw new ArgumentOutOfRangeException();
				}
				num = 0.25f;
			}
			else
			{
				num = 0.05f;
			}
			CS$<>8__locals1.interval = num;
			Vector3? vector = this._playBoard.FindActionSourceWorldPosition(action.Source);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Reverse<Card>(action.Args.Cards));
			CS$<>8__locals1.cwList = new List<CardWidget>();
			CS$<>8__locals1.startPosition = this.DrawLocalPosition;
			Sequence sequence = DOTween.Sequence();
			AudioManager.PlayUi("CardAppear", false);
			int count = list.Count;
			float num2 = 600f;
			int num3 = Math.Min(10, count);
			if (count > 5)
			{
				num2 = 2400f / (float)num3;
			}
			for (int i = 0; i < num3; i++)
			{
				Card card = list[i];
				CardWidget cardWidget = this.CreateCardWidget(card, this.cardEffectLayer);
				CS$<>8__locals1.cwList.Add(cardWidget);
				Transform transform = cardWidget.transform;
				if (vector != null)
				{
					Vector3 valueOrDefault = vector.GetValueOrDefault();
					transform.position = valueOrDefault;
				}
				else
				{
					transform.localPosition = CS$<>8__locals1.startPosition;
				}
				cardWidget.gameObject.SetActive(false);
				transform.localScale = new Vector3(0.2f, 0.2f);
				float num4 = CS$<>8__locals1.interval * (float)i;
				sequence.InsertCallback(num4, delegate
				{
					cardWidget.gameObject.SetActive(true);
				});
				sequence.Insert(num4, transform.DOLocalMove(new Vector3(num2 * ((float)i - (float)(num3 - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
				sequence.Insert(num4, transform.DOScale(Vector3.one, 0.3f));
			}
			yield return sequence.AppendInterval(0.3f).SetUpdate(true).WaitForCompletion();
			this.RefreshAll();
			CardUi.CheckKickerPrefer(action.Args.Cards);
			addCardsType = action.PresentationType;
			if (addCardsType != AddCardsType.Normal)
			{
				if (addCardsType != AddCardsType.OneByOne)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return CS$<>8__locals1.<ViewAddCardsToDrawZone>g__EndRunner|0();
			}
			else
			{
				base.StartCoroutine(CS$<>8__locals1.<ViewAddCardsToDrawZone>g__EndRunner|0());
			}
			yield break;
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00021FB3 File Offset: 0x000201B3
		private IEnumerator ViewAddCardsToDiscard(AddCardsToDiscardAction action)
		{
			CardUi.<>c__DisplayClass54_0 CS$<>8__locals1 = new CardUi.<>c__DisplayClass54_0();
			CS$<>8__locals1.<>4__this = this;
			AddCardsType addCardsType = action.PresentationType;
			float num;
			if (addCardsType != AddCardsType.Normal)
			{
				if (addCardsType != AddCardsType.OneByOne)
				{
					throw new ArgumentOutOfRangeException();
				}
				num = 0.25f;
			}
			else
			{
				num = 0.05f;
			}
			CS$<>8__locals1.interval = num;
			Vector3? vector = this._playBoard.FindActionSourceWorldPosition(action.Source);
			List<Card> list = Enumerable.ToList<Card>(action.Args.Cards);
			CS$<>8__locals1.list = new List<CardWidget>();
			Vector3 drawLocalPosition = this.DrawLocalPosition;
			CS$<>8__locals1.endPosition = this.DiscardLocalPosition;
			Sequence sequence = DOTween.Sequence();
			int count = list.Count;
			float num2 = 600f;
			int num3 = Math.Min(10, count);
			if (count > 5)
			{
				num2 = 2400f / (float)num3;
			}
			AudioManager.PlayUi("CardAppear", false);
			for (int i = 0; i < num3; i++)
			{
				Card card = list[i];
				CardWidget cardWidget = this.CreateCardWidget(card, this.cardEffectLayer);
				CS$<>8__locals1.list.Add(cardWidget);
				Transform transform = cardWidget.transform;
				if (vector != null)
				{
					Vector3 valueOrDefault = vector.GetValueOrDefault();
					transform.position = valueOrDefault;
				}
				else
				{
					transform.localPosition = drawLocalPosition;
				}
				cardWidget.gameObject.SetActive(false);
				transform.localScale = new Vector3(0.2f, 0.2f);
				float num4 = CS$<>8__locals1.interval * (float)i;
				sequence.InsertCallback(num4, delegate
				{
					cardWidget.gameObject.SetActive(true);
				});
				sequence.Insert(num4, transform.DOLocalMove(new Vector3(num2 * ((float)i - (float)(num3 - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
				sequence.Insert(num4, transform.DOScale(Vector3.one, 0.3f));
			}
			yield return sequence.AppendInterval(0.3f).SetUpdate(true).WaitForCompletion();
			this.RefreshAll();
			CardUi.CheckKickerPrefer(action.Args.Cards);
			addCardsType = action.PresentationType;
			if (addCardsType != AddCardsType.Normal)
			{
				if (addCardsType != AddCardsType.OneByOne)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return CS$<>8__locals1.<ViewAddCardsToDiscard>g__EndRunner|0();
			}
			else
			{
				base.StartCoroutine(CS$<>8__locals1.<ViewAddCardsToDiscard>g__EndRunner|0());
			}
			yield break;
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x00021FC9 File Offset: 0x000201C9
		private IEnumerator ViewAddCardsToDeck(AddCardsToDeckAction action)
		{
			Vector3? vector = this._playBoard.FindActionSourceWorldPosition(action.Source);
			List<Card> cards = Enumerable.ToList<Card>(action.Args.Cards);
			List<CardWidget> list = new List<CardWidget>();
			Sequence sequence = DOTween.Sequence();
			float num = 0.05f;
			int count = cards.Count;
			float num2 = 600f;
			int num3 = Math.Min(10, count);
			if (count > 5)
			{
				num2 = 2400f / (float)num3;
			}
			AudioManager.PlayUi("CardAppear", false);
			for (int i = 0; i < num3; i++)
			{
				Card card = cards[i];
				CardWidget cardWidget = this.CreateCardWidget(card, this.cardEffectLayer);
				list.Add(cardWidget);
				Transform transform = cardWidget.transform;
				if (vector != null)
				{
					Vector3 valueOrDefault = vector.GetValueOrDefault();
					transform.position = valueOrDefault;
				}
				cardWidget.gameObject.SetActive(false);
				transform.localScale = new Vector3(0.2f, 0.2f);
				float num4 = num * (float)i;
				sequence.InsertCallback(num4, delegate
				{
					cardWidget.gameObject.SetActive(true);
				});
				sequence.Insert(num4, transform.DOLocalMove(new Vector3(num2 * ((float)i - (float)(num3 - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
				sequence.Insert(num4, transform.DOScale(Vector3.one, 0.3f));
			}
			yield return sequence.AppendInterval(0.3f).SetUpdate(true).WaitForCompletion();
			this.RefreshAll();
			CardUi.CheckKickerPrefer(action.Args.Cards);
			UiManager.GetPanel<GameRunVisualPanel>().PlayAddToDeckEffect(cards, list, 0f);
			yield break;
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x00021FDF File Offset: 0x000201DF
		private IEnumerator ViewAddCardsToExile(AddCardsToExileAction action)
		{
			AddCardsType presentationType = action.PresentationType;
			float num;
			if (presentationType != AddCardsType.Normal)
			{
				if (presentationType != AddCardsType.OneByOne)
				{
					throw new ArgumentOutOfRangeException();
				}
				num = 0.25f;
			}
			else
			{
				num = 0.05f;
			}
			float interval = num;
			Vector3? vector = this._playBoard.FindActionSourceWorldPosition(action.Source);
			List<Card> list2 = Enumerable.ToList<Card>(action.Args.Cards);
			List<CardWidget> list = new List<CardWidget>();
			Vector3 drawLocalPosition = this.DrawLocalPosition;
			Vector3 discardLocalPosition = this.DiscardLocalPosition;
			Sequence sequence = DOTween.Sequence();
			int count = list2.Count;
			float num2 = 600f;
			int num3 = Math.Min(10, count);
			if (count > 5)
			{
				num2 = 2400f / (float)num3;
			}
			AudioManager.PlayUi("CardAppear", false);
			for (int i = 0; i < num3; i++)
			{
				Card card = list2[i];
				CardWidget cardWidget = this.CreateCardWidget(card, this.cardEffectLayer);
				list.Add(cardWidget);
				Transform transform = cardWidget.transform;
				if (vector != null)
				{
					Vector3 valueOrDefault = vector.GetValueOrDefault();
					transform.position = valueOrDefault;
				}
				else
				{
					transform.localPosition = drawLocalPosition;
				}
				cardWidget.gameObject.SetActive(false);
				transform.localScale = new Vector3(0.2f, 0.2f);
				float num4 = interval * (float)i;
				sequence.InsertCallback(num4, delegate
				{
					cardWidget.gameObject.SetActive(true);
				});
				sequence.Insert(num4, transform.DOLocalMove(new Vector3(num2 * ((float)i - (float)(num3 - 1) / 2f), 0f), 0.3f, false)).SetEase(Ease.OutSine);
				sequence.Insert(num4, transform.DOScale(Vector3.one, 0.3f));
			}
			yield return sequence.AppendInterval(0.3f).SetUpdate(true).WaitForCompletion();
			this.RefreshAll();
			Sequence sequence2 = DOTween.Sequence();
			using (List<CardWidget>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CardWidget widget = enumerator.Current;
					sequence2.AppendCallback(delegate
					{
						widget.Exile();
					}).AppendInterval(interval);
				}
			}
			sequence2.SetUpdate(true);
			this.RefreshAll();
			this.RefreshAllZoneCount();
			CardUi.CheckKickerPrefer(action.Args.Cards);
			yield break;
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x00021FF5 File Offset: 0x000201F5
		private IEnumerator ViewMoveCardToDrawZone(MoveCardToDrawZoneAction action)
		{
			CardTransitionType transitionType = action.TransitionType;
			if (transitionType != CardTransitionType.Normal && transitionType != CardTransitionType.SpecialBegin)
			{
				yield break;
			}
			switch (action.Args.SourceZone)
			{
			case CardZone.None:
				throw new InvalidOperationException(string.Format("Trying to move card from invalid source zone {0}.", action.Args.SourceZone));
			case CardZone.Draw:
				break;
			case CardZone.Hand:
			case CardZone.PlayArea:
				yield return this.MoveCardToDrawZone(this.ExtractHandWidget(action.Args.Card));
				break;
			case CardZone.Discard:
				this.RefreshDiscardCount();
				yield return this.SimpleCardFlyEffect(CardZone.Discard, CardZone.Draw, 0.2f);
				this.RefreshDrawCount();
				break;
			case CardZone.Exile:
				this.RefreshExileCount();
				yield return this.SimpleCardFlyEffect(CardZone.Exile, CardZone.Draw, 0.2f);
				this.RefreshDrawCount();
				break;
			case CardZone.FollowArea:
				yield return this.MoveCardToDrawZone(this.FindFollowPlayWidget(action.Args.Card));
				break;
			default:
				throw new InvalidOperationException("Trying to move card from invalid source zone.");
			}
			this.RefreshAll();
			yield break;
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x0002200B File Offset: 0x0002020B
		private IEnumerator ViewMoveCard(MoveCardAction action)
		{
			CardMovingEventArgs args = action.Args;
			if (args.DestinationZone == CardZone.Hand && args.SourceZone == CardZone.PlayArea)
			{
				this.CardReturnToHand(args.Card);
			}
			else
			{
				CardZone sourceZone = args.SourceZone;
				if (sourceZone == CardZone.Hand || sourceZone == CardZone.PlayArea)
				{
					CardTransitionType transitionType = action.TransitionType;
					if (transitionType == CardTransitionType.Normal)
					{
						HandCard handCard = this.ExtractHandWidget(args.Card);
						if (handCard == null)
						{
							Debug.LogError("[CardUi] Cannot find hand widget for " + args.Card.DebugName);
							yield break;
						}
						this._playBoard.CancelTargetSelectingIfCardIs(handCard);
						if (args.DestinationZone == CardZone.Discard)
						{
							yield return this.MoveCardToDiscard(handCard, args.Cause != ActionCause.TurnEnd);
						}
						else
						{
							CardUi.FastRemoveHand(handCard);
						}
					}
					else if (transitionType == CardTransitionType.SpecialBegin)
					{
						HandCard handCard2 = this.ExtractHandWidget(args.Card);
						if (handCard2 == null)
						{
							Debug.LogError("[CardUi] Cannot find hand widget for " + args.Card.DebugName);
							yield break;
						}
						this._specialReactingWidgets.Add(handCard2);
						handCard2.SpecialReacting = true;
					}
					else if (transitionType == CardTransitionType.SpecialEnd)
					{
						HandCard hand = Enumerable.FirstOrDefault<HandCard>(this._specialReactingWidgets, (HandCard w) => w.Card == args.Card);
						if (hand == null)
						{
							Debug.LogError("[CardUi] Cannot find moving widget for " + args.Card.DebugName);
							yield break;
						}
						yield return new WaitForSecondsRealtime(0.3f);
						this._specialReactingWidgets.Remove(hand);
						hand.SpecialReacting = false;
						if (args.DestinationZone == CardZone.Discard)
						{
							yield return this.MoveCardToDiscard(hand, args.Cause != ActionCause.TurnEnd);
						}
						else
						{
							CardUi.FastRemoveHand(hand);
						}
						hand = null;
					}
					else
					{
						Debug.LogError(string.Format("[{0}] Cannot recognize transition type of {1}: {2}", "CardUi", "DrawCardAction", transitionType));
					}
				}
				else if (args.SourceZone == CardZone.FollowArea)
				{
					CardWidget cardWidget = this.FindFollowPlayWidget(args.Card);
					if (args.DestinationZone == CardZone.Discard)
					{
						this._followPlayWidgets.Remove(cardWidget);
						yield return this.MoveCardToDiscard(cardWidget);
					}
					else if (args.DestinationZone == CardZone.Hand)
					{
						this._followPlayWidgets.Remove(cardWidget);
						HandCard handCard3 = this.ConvertToHand(cardWidget);
						this._handWidgets.Add(handCard3);
						this.AdjustCardsPosition(false);
						this.RefreshAllCardsEdge();
					}
					else
					{
						CardUi.FastRemoveCard(cardWidget);
					}
				}
				else if (args.DestinationZone == CardZone.Hand)
				{
					CardTransitionType transitionType2 = action.TransitionType;
					if (transitionType2 == CardTransitionType.Normal || transitionType2 == CardTransitionType.SpecialBegin)
					{
						HandCard handCard4 = this.CreateHandWidget(args.Card);
						Vector3 vector;
						switch (args.SourceZone)
						{
						case CardZone.Draw:
							vector = this.DrawPosition;
							goto IL_0493;
						case CardZone.Discard:
							vector = this.DiscardPosition;
							goto IL_0493;
						case CardZone.Exile:
							vector = this.ExilePosition;
							goto IL_0493;
						}
						throw new ArgumentOutOfRangeException(string.Format("[CardUi] Cannot view card move from {0} to {1}", args.SourceZone, args.DestinationZone));
						IL_0493:
						Vector3 vector2 = vector;
						Transform transform = handCard4.transform;
						transform.position = vector2;
						transform.localScale = Vector3.zero;
						this._handWidgets.Add(handCard4);
						this.AdjustCardsPosition(false);
						this.RefreshAllCardsEdge();
					}
				}
				else if (action.DreamCardsAction != null)
				{
					yield return this.ShowCardMoveRunner(args.Card, action.DreamCardsAction.Cards.IndexOf(args.Card), action.DreamCardsAction.Cards.Count);
				}
				else
				{
					yield return this.SimpleCardFlyEffect(args.SourceZone, args.DestinationZone, 0.2f);
				}
			}
			this.RefreshAll();
			this.RefreshAllZoneCount();
			yield break;
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00022021 File Offset: 0x00020221
		private IEnumerator ViewUpgradeCard(UpgradeCardAction action)
		{
			Card card = action.Card;
			bool flag = false;
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			switch (card.Zone)
			{
			case CardZone.Draw:
				flag = true;
				vector = this.DrawLocalPosition;
				vector2 = vector + this._rightOffset;
				break;
			case CardZone.Hand:
			{
				HandCard handCard = this.FindHandWidget(card);
				if (handCard != null)
				{
					Object @object = Object.Instantiate<GameObject>(this.cardUpgrade, handCard.transform);
					AudioManager.PlayUi("UpgradeCardTemp", false);
					Object.Destroy(@object, 2f);
				}
				break;
			}
			case CardZone.Discard:
				flag = true;
				vector = this.DiscardLocalPosition;
				vector2 = vector + this._leftOffset;
				break;
			case CardZone.Exile:
				flag = true;
				vector = this.ExileLocalPosition;
				vector2 = vector + this._leftOffset;
				break;
			}
			if (flag)
			{
				this.ViewCard(card, vector, vector2, true);
			}
			yield return new WaitForSecondsRealtime(0.5f);
			this.RefreshAll();
			yield break;
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00022037 File Offset: 0x00020237
		private IEnumerator ViewUpgradeCards(UpgradeCardsAction action)
		{
			List<Card> list = new List<Card>();
			List<Card> list2 = new List<Card>();
			List<Card> list3 = new List<Card>();
			foreach (Card card in action.Cards)
			{
				switch (card.Zone)
				{
				case CardZone.Draw:
					if (list.Count < 5)
					{
						list.Add(card);
					}
					break;
				case CardZone.Hand:
				{
					HandCard handCard = this.FindHandWidget(card);
					if (handCard != null)
					{
						Object @object = Object.Instantiate<GameObject>(this.cardUpgrade, handCard.transform);
						AudioManager.PlayUi("UpgradeCardTemp", false);
						Object.Destroy(@object, 2f);
					}
					break;
				}
				case CardZone.Discard:
					if (list2.Count < 5)
					{
						list2.Add(card);
					}
					break;
				case CardZone.Exile:
					if (list3.Count < 5)
					{
						list3.Add(card);
					}
					break;
				}
			}
			if (list.Count > 0)
			{
				foreach (ValueTuple<int, Card> valueTuple in list.WithIndices<Card>())
				{
					int item = valueTuple.Item1;
					Card item2 = valueTuple.Item2;
					this.ViewCard(item2, this.DrawLocalPosition, this.DrawLocalPosition + this._rightOffset + (float)item * new Vector3(100f, 0f, 0f), true);
				}
			}
			if (list2.Count > 0)
			{
				foreach (ValueTuple<int, Card> valueTuple2 in list2.WithIndices<Card>())
				{
					int item3 = valueTuple2.Item1;
					Card item4 = valueTuple2.Item2;
					this.ViewCard(item4, this.DiscardLocalPosition, this.DiscardLocalPosition + this._leftOffset + (float)item3 * new Vector3(-100f, 0f, 0f), true);
				}
			}
			if (list3.Count > 0)
			{
				foreach (ValueTuple<int, Card> valueTuple3 in list3.WithIndices<Card>())
				{
					int item5 = valueTuple3.Item1;
					Card item6 = valueTuple3.Item2;
					this.ViewCard(item6, this.ExileLocalPosition, this.ExileLocalPosition + this._leftOffset + (float)item5 * new Vector3(-100f, 0f, 0f), true);
				}
			}
			yield return new WaitForSecondsRealtime(0.5f);
			this.RefreshAll();
			yield break;
		}

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000756 RID: 1878 RVA: 0x0002204D File Offset: 0x0002024D
		public List<Vector2> CardBaseV2List { get; } = new List<Vector2>();

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000757 RID: 1879 RVA: 0x00022055 File Offset: 0x00020255
		public List<float> CardBaseRotateList { get; } = new List<float>();

		// Token: 0x06000758 RID: 1880 RVA: 0x0002205D File Offset: 0x0002025D
		public void ConfirmUseCard(HandCard hand)
		{
			hand.PendingUse = true;
			this._pendingUseWidgets.Add(hand);
			this.AdjustPendingCardsLocation();
			this.AdjustCardsPosition(false);
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00022080 File Offset: 0x00020280
		private void AdjustPendingCardsLocation()
		{
			if (this._pendingUseWidgets.Count > 0)
			{
				foreach (HandCard handCard in this._pendingUseWidgets)
				{
					handCard.transform.SetParent(base.transform);
				}
				for (int i = this._pendingUseWidgets.Count - 1; i >= 0; i--)
				{
					this._pendingUseWidgets[i].transform.SetParent(this.cardPendingUseCache);
				}
				float num = 50f;
				if (this._pendingUseWidgets.Count > 1)
				{
					num = Mathf.Min(50f, 200f / (float)(this._pendingUseWidgets.Count - 1));
				}
				for (int j = 0; j < this._pendingUseWidgets.Count; j++)
				{
					this._pendingUseWidgets[j].PendingUsePosition = this.cardPendingUsePoint.localPosition + new Vector3(-num * (float)j, 0f);
				}
			}
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x0002219C File Offset: 0x0002039C
		private void AdjustCardsPosition(bool reOrder = false)
		{
			float num = this._rectTransform.rect.width / this.curvatureRatio;
			if (reOrder)
			{
				this.ReOrderHands();
			}
			List<HandCard> list = Enumerable.ToList<HandCard>(this.GetUnpendingHands());
			float num2 = (float)list.Count / 2f - 0.5f;
			float num3 = (float)this.deltaX;
			int num4 = list.Count;
			if (num4 < 12)
			{
				if (num4 == 11)
				{
					num3 *= 0.91f;
				}
			}
			else
			{
				num3 *= 0.84f;
			}
			for (int i = 0; i < list.Count; i++)
			{
				float num5 = (float)i - num2;
				HandCard handCard = list[i];
				float num6 = num5 * num3;
				float num7 = (float.IsInfinity(num) ? 0f : ((num.Square() - num6.Square()).Sqrt() - num));
				this.CardBaseV2List[i] = new Vector2(num6, num7) + this.handOffset;
				this.CardBaseRotateList[i] = -this.deltaRotate * num5;
				handCard.NormalPosition = this.CardBaseV2List[i];
				handCard.NormalRotation = Quaternion.Euler(0f, 0f, this.CardBaseRotateList[i]);
				List<Vector2> cardBaseV2List = this.CardBaseV2List;
				num4 = i;
				cardBaseV2List[num4] += CardUi.HalfScreenV2;
				handCard.HoveredPosition = new Vector3(num6, (float)this.hoveredY) + new Vector3(this.handOffset.x, this.handOffset.y);
				handCard.HoveredRotation = Quaternion.identity;
			}
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x0002234C File Offset: 0x0002054C
		private void ReOrderHands()
		{
			this._handWidgets = Enumerable.ToList<HandCard>(Enumerable.OrderBy<HandCard, int>(this._handWidgets, (HandCard hand) => this.Battle.HandZone.IndexOf(hand.Card)));
			foreach (HandCard handCard in this._handWidgets)
			{
				handCard.MoveToParentWhenReordering(this.cardHandReorderCache);
				handCard.MoveToParentWhenReordering(this.cardHandParent);
			}
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x000223D0 File Offset: 0x000205D0
		private void DelayedAdjustCardsPosition(float delay = 0.4f)
		{
			float num = Time.unscaledTime + delay;
			float? nextAdjustCardTime = this._nextAdjustCardTime;
			float num2;
			if (nextAdjustCardTime != null)
			{
				float valueOrDefault = nextAdjustCardTime.GetValueOrDefault();
				num2 = Mathf.Max(valueOrDefault, num);
			}
			else
			{
				num2 = num;
			}
			this._nextAdjustCardTime = new float?(num2);
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x00022414 File Offset: 0x00020614
		public void RefreshAll()
		{
			foreach (ValueTuple<int, HandCard> valueTuple in this._handWidgets.WithIndices<HandCard>())
			{
				int item = valueTuple.Item1;
				HandCard item2 = valueTuple.Item2;
				item2.HandIndex = item;
				item2.RefreshStatus();
			}
			this.RefreshAllCardsEdge();
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x0002247C File Offset: 0x0002067C
		public void RefreshAllCardsEdge()
		{
			bool flag = true;
			foreach (HandCard handCard in this._handWidgets)
			{
				bool flag3;
				bool flag2 = this._playBoard.UseCardVerify(handCard.Card, false, out flag3);
				handCard.SetUsableStatus(flag2, flag3);
				if (handCard.CardWidget.Edge != CardWidget.EdgeStatus.None)
				{
					flag = false;
				}
			}
			this._playBoard.SetEndTurnParticle(flag);
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x00022504 File Offset: 0x00020704
		public void StartCardsEndNotify()
		{
			foreach (HandCard handCard in this._handWidgets)
			{
				if (handCard.CanUse)
				{
					handCard.CardWidget.SetEndingNotify();
				}
			}
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x06000760 RID: 1888 RVA: 0x00022564 File Offset: 0x00020764
		// (set) Token: 0x06000761 RID: 1889 RVA: 0x0002256C File Offset: 0x0002076C
		public int DrawCount
		{
			get
			{
				return this._drawCount;
			}
			set
			{
				this._drawCount = value;
				this.drawText.text = value.ToString();
			}
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000762 RID: 1890 RVA: 0x00022587 File Offset: 0x00020787
		// (set) Token: 0x06000763 RID: 1891 RVA: 0x0002258F File Offset: 0x0002078F
		public int DrawUpperCount
		{
			get
			{
				return this._drawUpperCount;
			}
			set
			{
				this._drawUpperCount = value;
				this.drawUpperText.text = value.ToString();
			}
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x000225AC File Offset: 0x000207AC
		private void RefreshDrawCount()
		{
			this.DrawCount = this.Battle.DrawZone.Count;
			if (Enumerable.Any<Card>(this.Battle.DrawZone, (Card card) => card.IsFollowCard))
			{
				this.DrawUpperCount = Enumerable.Count<Card>(this.Battle.DrawZone, (Card card) => card.IsFollowCard);
				this.drawUpperText.color = this._followColor;
			}
			else if (Enumerable.Any<Card>(this.Battle.DrawZone, (Card card) => card.IsDreamCard))
			{
				this.DrawUpperCount = Enumerable.Count<Card>(this.Battle.DrawZone, (Card card) => card.IsDreamCard);
				this.drawUpperText.color = this._dreamColor;
			}
			else
			{
				this.DrawUpperCount = 0;
			}
			this.drawUpperRoot.Active(this.DrawUpperCount > 0);
			if (this.DrawUpperCount <= 0)
			{
				this.HideDrawFastView();
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000765 RID: 1893 RVA: 0x000226EE File Offset: 0x000208EE
		// (set) Token: 0x06000766 RID: 1894 RVA: 0x000226F6 File Offset: 0x000208F6
		public int DiscardCount
		{
			get
			{
				return this._discardCount;
			}
			set
			{
				this._discardCount = value;
				this.discardText.text = value.ToString();
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000767 RID: 1895 RVA: 0x00022711 File Offset: 0x00020911
		// (set) Token: 0x06000768 RID: 1896 RVA: 0x00022719 File Offset: 0x00020919
		public int DiscardUpperCount
		{
			get
			{
				return this._discardUpperCount;
			}
			set
			{
				this._discardUpperCount = value;
				this.discardUpperText.text = value.ToString();
			}
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00022734 File Offset: 0x00020934
		private void RefreshDiscardCount()
		{
			this.DiscardCount = this.Battle.DiscardZone.Count;
			this.DiscardUpperCount = Enumerable.Count<Card>(this.Battle.DiscardZone, (Card card) => card.IsDreamCard);
			this.discardUpperRoot.Active(this.DiscardUpperCount > 0);
			if (this.DiscardUpperCount <= 0)
			{
				this.HideDiscardFastView();
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x0600076A RID: 1898 RVA: 0x000227AF File Offset: 0x000209AF
		// (set) Token: 0x0600076B RID: 1899 RVA: 0x000227B8 File Offset: 0x000209B8
		public int ExileCount
		{
			get
			{
				return this._exileCount;
			}
			set
			{
				this._exileCount = value;
				this.exileText.text = value.ToString();
				if (value > 0)
				{
					this.exileButton.gameObject.SetActive(true);
					this.exileText.gameObject.SetActive(true);
					return;
				}
				this.exileButton.gameObject.SetActive(false);
				this.exileText.gameObject.SetActive(false);
			}
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00022827 File Offset: 0x00020A27
		private void RefreshExileCount()
		{
			this.ExileCount = this.Battle.ExileZone.Count;
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x0002283F File Offset: 0x00020A3F
		private void RefreshAllZoneCount()
		{
			this.RefreshDrawCount();
			this.RefreshDiscardCount();
			this.RefreshExileCount();
		}

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x0600076E RID: 1902 RVA: 0x00022853 File Offset: 0x00020A53
		public Vector3 DrawPosition
		{
			get
			{
				return this.drawRoot.transform.position;
			}
		}

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x0600076F RID: 1903 RVA: 0x00022865 File Offset: 0x00020A65
		public Vector3 DiscardPosition
		{
			get
			{
				return this.discardRoot.transform.position;
			}
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000770 RID: 1904 RVA: 0x00022877 File Offset: 0x00020A77
		public Vector3 ExilePosition
		{
			get
			{
				return this.exileRoot.transform.position;
			}
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000771 RID: 1905 RVA: 0x00022889 File Offset: 0x00020A89
		public Vector3 DrawLocalPosition
		{
			get
			{
				return this.drawRoot.transform.localPosition;
			}
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000772 RID: 1906 RVA: 0x0002289B File Offset: 0x00020A9B
		public Vector3 DiscardLocalPosition
		{
			get
			{
				return this.discardRoot.transform.localPosition;
			}
		}

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000773 RID: 1907 RVA: 0x000228AD File Offset: 0x00020AAD
		public Vector3 ExileLocalPosition
		{
			get
			{
				return this.exileRoot.transform.localPosition;
			}
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x000228C0 File Offset: 0x00020AC0
		private static void ImageBlink(Component image)
		{
			Transform trans = image.transform;
			trans.DOKill(false);
			trans.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.05f).From(Vector3.one, true, false).OnComplete(delegate
			{
				trans.DOScale(Vector3.one, 0.05f);
			})
				.SetUpdate(true)
				.SetAutoKill(true);
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x0002293C File Offset: 0x00020B3C
		public void ShowDrawFastView()
		{
			if (Enumerable.Any<Card>(this.Battle.DrawZone, (Card card) => card.IsFollowCard))
			{
				this.fastDrawViewer.Show(Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.DrawZoneIndexOrder, (Card card) => card.IsFollowCard)));
				return;
			}
			if (Enumerable.Any<Card>(this.Battle.DrawZone, (Card card) => card.IsDreamCard))
			{
				this.fastDrawViewer.Show(Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.DrawZoneIndexOrder, (Card card) => card.IsDreamCard)));
			}
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00022A2C File Offset: 0x00020C2C
		public void ShowDiscardFastView()
		{
			if (Enumerable.Any<Card>(this.Battle.DiscardZone, (Card card) => card.IsDreamCard))
			{
				this.fastDiscardViewer.Show(Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.DiscardZone, (Card card) => card.IsDreamCard)));
			}
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00022AA9 File Offset: 0x00020CA9
		public void HideDrawFastView()
		{
			this.fastDrawViewer.Hide();
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00022AB6 File Offset: 0x00020CB6
		public void HideDiscardFastView()
		{
			this.fastDiscardViewer.Hide();
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00022AC3 File Offset: 0x00020CC3
		private void HideFastViewers()
		{
			this.HideDrawFastView();
			this.HideDiscardFastView();
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00022AD1 File Offset: 0x00020CD1
		public IEnumerator ViewCardFromZone(Card card, CardZone zone)
		{
			Vector3 vector;
			switch (zone)
			{
			case CardZone.Draw:
				vector = this.DrawLocalPosition;
				goto IL_009F;
			case CardZone.Discard:
				vector = this.DiscardLocalPosition;
				goto IL_009F;
			case CardZone.Exile:
				vector = this.ExileLocalPosition;
				goto IL_009F;
			}
			throw new ArgumentOutOfRangeException("zone", zone, string.Format("Cannot view cards from zone {0}", zone));
			IL_009F:
			Vector3 vector2 = vector;
			switch (zone)
			{
			case CardZone.Draw:
				vector = vector2 + this._rightOffset;
				goto IL_0120;
			case CardZone.Discard:
				vector = vector2 + this._leftOffset;
				goto IL_0120;
			case CardZone.Exile:
				vector = vector2 + this._leftOffset;
				goto IL_0120;
			}
			throw new ArgumentOutOfRangeException("zone", zone, string.Format("Cannot view cards from zone {0}", zone));
			IL_0120:
			Vector3 vector3 = vector;
			CardWidget widget = this.CreateCardWidget(card);
			this._viewWidget.Add(widget);
			Transform transform = widget.transform;
			transform.SetParent(base.transform);
			transform.localPosition = vector2;
			widget.Card = card;
			CanvasGroup canvasGroup = widget.CanvasGroup;
			canvasGroup.interactable = false;
			DOTween.Sequence().Join(transform.DOScale(1f, 0.1f).From(0f, true, false)).Join(transform.DOLocalMove(vector3, 0.1f, false).SetEase(Ease.OutCubic))
				.Insert(0.8f, canvasGroup.DOFade(0f, 0.1f).SetLink(widget.gameObject))
				.Insert(0.8f, transform.DOLocalMoveX(50f, 0.1f, false).SetRelative(true).SetEase(Ease.OutCubic))
				.SetUpdate(true)
				.SetLink(widget.gameObject)
				.OnComplete(delegate
				{
					this._viewWidget.Remove(widget);
					Object.Destroy(widget.gameObject);
				});
			yield return new WaitForSecondsRealtime(0.5f);
			yield break;
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x00022AF0 File Offset: 0x00020CF0
		private void ViewCard(Card card, Vector3 startPosition, Vector3 stopPosition, bool playUpgradeEffect)
		{
			CardWidget widget = this.CreateCardWidget(card);
			this._viewWidget.Add(widget);
			Transform transform = widget.transform;
			transform.SetParent(base.transform);
			transform.localPosition = startPosition;
			widget.Card = card;
			CanvasGroup canvasGroup = widget.CanvasGroup;
			canvasGroup.interactable = false;
			if (playUpgradeEffect)
			{
				Object @object = Object.Instantiate<GameObject>(this.cardUpgrade, transform);
				AudioManager.PlayUi("UpgradeCardTemp", false);
				Object.Destroy(@object, 2f);
			}
			DOTween.Sequence().Join(transform.DOScale(1f, 0.1f).From(0f, true, false)).Join(transform.DOLocalMove(stopPosition, 0.1f, false).SetEase(Ease.OutCubic))
				.Insert(0.8f, canvasGroup.DOFade(0f, 0.1f).SetLink(widget.gameObject))
				.Insert(0.8f, transform.DOLocalMoveX(50f, 0.1f, false).SetRelative(true).SetEase(Ease.OutCubic))
				.SetUpdate(true)
				.SetLink(widget.gameObject)
				.OnComplete(delegate
				{
					this._viewWidget.Remove(widget);
					Object.Destroy(widget.gameObject);
				});
		}

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x0600077C RID: 1916 RVA: 0x00022C44 File Offset: 0x00020E44
		private Vector3 PlayCardOffset
		{
			get
			{
				return new Vector3(100f * (float)(this._followPlayWidgets.Count - 1), 0f, 0f);
			}
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x00022C69 File Offset: 0x00020E69
		private IEnumerator ViewPlayCard(PlayCardAction action)
		{
			Card card = action.Args.Card;
			float num = 0.1f;
			this.RefreshAllZoneCount();
			HandCard handCard = this.ExtractHandWidget(card);
			CardWidget cardWidget = ((handCard != null) ? handCard.CardWidget : this.CreateCardWidget(card));
			this._followPlayWidgets.Add(cardWidget);
			Transform transform = cardWidget.transform;
			transform.SetParent(base.transform);
			if (card.IsPlayTwiceToken)
			{
				transform.localPosition = this.cardPendingUsePoint.localPosition;
				if (card.PlayTwiceSourceCard != null)
				{
					HandCard handCard2 = Enumerable.FirstOrDefault<HandCard>(this._handWidgets, (HandCard hand) => hand.Card == card.PlayTwiceSourceCard);
					if (handCard2 != null)
					{
						transform.localPosition = handCard2.transform.localPosition;
					}
				}
			}
			else if (handCard == null)
			{
				CardZone sourceZone = action.SourceZone;
				if (sourceZone != CardZone.Discard)
				{
					if (sourceZone != CardZone.Exile)
					{
						transform.localPosition = this.DrawLocalPosition;
					}
					else
					{
						transform.localPosition = this.ExileLocalPosition;
						num = 0.2f;
					}
				}
				else
				{
					transform.localPosition = this.DiscardLocalPosition;
					num = 0.2f;
				}
			}
			cardWidget.Card = card;
			cardWidget.CanvasGroup.interactable = false;
			if (handCard != null)
			{
				transform.localRotation = Quaternion.identity;
			}
			DOTween.Sequence().Join(transform.DOScale(1f, num).From(0f, true, false)).Join(transform.DOLocalMove(this.cardFollowPlayPoint.localPosition + this.PlayCardOffset, num, false))
				.SetEase(Ease.OutCubic)
				.SetUpdate(true)
				.SetLink(cardWidget.gameObject);
			yield return new WaitForSecondsRealtime(0.3f);
			yield break;
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x00022C80 File Offset: 0x00020E80
		private void Awake()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
			this._playBoard = base.GetComponentInParent<PlayBoard>();
			this.drawButton.onClick.AddListener(new UnityAction(this._playBoard.ShowDrawZone));
			SimpleTooltipSource.CreateWithTooltipKeyAndArgs(this.drawButton.gameObject, "DrawZone", new object[] { 5 }).WithPosition(TooltipDirection.Top, TooltipAlignment.Min);
			this.discardButton.onClick.AddListener(new UnityAction(this._playBoard.ShowDiscardZone));
			SimpleTooltipSource.CreateWithTooltipKey(this.discardButton.gameObject, "DiscardZone").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.exileButton.onClick.AddListener(new UnityAction(this._playBoard.ShowExileZone));
			SimpleTooltipSource.CreateWithTooltipKey(this.exileButton.gameObject, "ExileZone").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			for (int i = 0; i < 12; i++)
			{
				this.CardBaseV2List.Add(Vector2.zero);
				this.CardBaseRotateList.Add(0f);
			}
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x00022D9C File Offset: 0x00020F9C
		private void Update()
		{
			float? nextAdjustCardTime = this._nextAdjustCardTime;
			if (nextAdjustCardTime != null)
			{
				float valueOrDefault = nextAdjustCardTime.GetValueOrDefault();
				if (Time.unscaledTime > valueOrDefault)
				{
					this.AdjustCardsPosition(false);
					this._nextAdjustCardTime = default(float?);
				}
			}
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00022DDC File Offset: 0x00020FDC
		public void EnterBattle(BattleController battle)
		{
			this.Battle = battle;
			this.RefreshAllZoneCount();
			foreach (Card card in battle.HandZone)
			{
				this._handWidgets.Add(this.CreateHandWidget(card));
			}
			battle.ActionViewer.Register<DrawCardAction>(new BattleActionViewer<DrawCardAction>(this.ViewDrawCard), null);
			battle.ActionViewer.Register<DrawSelectedCardAction>(new BattleActionViewer<DrawSelectedCardAction>(this.ViewDrawSelectedCard), null);
			battle.ActionViewer.Register<DiscardAction>(new BattleActionViewer<DiscardAction>(this.ViewDiscard), null);
			battle.ActionViewer.Register<ExileCardAction>(new BattleActionViewer<ExileCardAction>(this.ViewExileCard), null);
			battle.ActionViewer.Register<RemoveCardAction>(new BattleActionViewer<RemoveCardAction>(this.ViewRemoveCard), null);
			battle.ActionViewer.Register<TransformCardAction>(new BattleActionViewer<TransformCardAction>(this.ViewTransformCard), null);
			battle.ActionViewer.Register<RetainAction>(new BattleActionViewer<RetainAction>(this.ViewRetain), null);
			battle.ActionViewer.Register<ReshuffleAction>(new BattleActionViewer<ReshuffleAction>(this.ViewReshuffle), null);
			battle.ActionViewer.Register<AddCardsToHandAction>(new BattleActionViewer<AddCardsToHandAction>(this.ViewAddCardsToHand), null);
			battle.ActionViewer.Register<AddCardsToDrawZoneAction>(new BattleActionViewer<AddCardsToDrawZoneAction>(this.ViewAddCardsToDrawZone), null);
			battle.ActionViewer.Register<AddCardsToDiscardAction>(new BattleActionViewer<AddCardsToDiscardAction>(this.ViewAddCardsToDiscard), null);
			battle.ActionViewer.Register<AddCardsToDeckAction>(new BattleActionViewer<AddCardsToDeckAction>(this.ViewAddCardsToDeck), null);
			battle.ActionViewer.Register<AddCardsToExileAction>(new BattleActionViewer<AddCardsToExileAction>(this.ViewAddCardsToExile), null);
			battle.ActionViewer.Register<MoveCardToDrawZoneAction>(new BattleActionViewer<MoveCardToDrawZoneAction>(this.ViewMoveCardToDrawZone), null);
			battle.ActionViewer.Register<MoveCardAction>(new BattleActionViewer<MoveCardAction>(this.ViewMoveCard), null);
			battle.ActionViewer.Register<UpgradeCardAction>(new BattleActionViewer<UpgradeCardAction>(this.ViewUpgradeCard), null);
			battle.ActionViewer.Register<UpgradeCardsAction>(new BattleActionViewer<UpgradeCardsAction>(this.ViewUpgradeCards), null);
			battle.ActionViewer.Register<PlayCardAction>(new BattleActionViewer<PlayCardAction>(this.ViewPlayCard), null);
			battle.CardUsingCanceled.AddHandler(new GameEventHandler<CardUsingEventArgs>(this.OnCardUsingCanceled), GameEventPriority.Lowest);
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00023010 File Offset: 0x00021210
		private void OnCardUsingCanceled(CardUsingEventArgs args)
		{
			this.CancelUse(args.Card);
			UiManager.GetPanel<BattleManaPanel>().RefundFront(default(ManaGroup?));
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x0002303C File Offset: 0x0002123C
		public void LeaveBattle(BattleController battle)
		{
			battle.ActionViewer.Unregister<DrawCardAction>(new BattleActionViewer<DrawCardAction>(this.ViewDrawCard));
			battle.ActionViewer.Unregister<DrawSelectedCardAction>(new BattleActionViewer<DrawSelectedCardAction>(this.ViewDrawSelectedCard));
			battle.ActionViewer.Unregister<DiscardAction>(new BattleActionViewer<DiscardAction>(this.ViewDiscard));
			battle.ActionViewer.Unregister<ExileCardAction>(new BattleActionViewer<ExileCardAction>(this.ViewExileCard));
			battle.ActionViewer.Unregister<RemoveCardAction>(new BattleActionViewer<RemoveCardAction>(this.ViewRemoveCard));
			battle.ActionViewer.Unregister<TransformCardAction>(new BattleActionViewer<TransformCardAction>(this.ViewTransformCard));
			battle.ActionViewer.Unregister<RetainAction>(new BattleActionViewer<RetainAction>(this.ViewRetain));
			battle.ActionViewer.Unregister<ReshuffleAction>(new BattleActionViewer<ReshuffleAction>(this.ViewReshuffle));
			battle.ActionViewer.Unregister<AddCardsToHandAction>(new BattleActionViewer<AddCardsToHandAction>(this.ViewAddCardsToHand));
			battle.ActionViewer.Unregister<AddCardsToDrawZoneAction>(new BattleActionViewer<AddCardsToDrawZoneAction>(this.ViewAddCardsToDrawZone));
			battle.ActionViewer.Unregister<AddCardsToDiscardAction>(new BattleActionViewer<AddCardsToDiscardAction>(this.ViewAddCardsToDiscard));
			battle.ActionViewer.Unregister<AddCardsToDeckAction>(new BattleActionViewer<AddCardsToDeckAction>(this.ViewAddCardsToDeck));
			battle.ActionViewer.Unregister<AddCardsToExileAction>(new BattleActionViewer<AddCardsToExileAction>(this.ViewAddCardsToExile));
			battle.ActionViewer.Unregister<MoveCardToDrawZoneAction>(new BattleActionViewer<MoveCardToDrawZoneAction>(this.ViewMoveCardToDrawZone));
			battle.ActionViewer.Unregister<MoveCardAction>(new BattleActionViewer<MoveCardAction>(this.ViewMoveCard));
			battle.ActionViewer.Unregister<UpgradeCardAction>(new BattleActionViewer<UpgradeCardAction>(this.ViewUpgradeCard));
			battle.ActionViewer.Unregister<UpgradeCardsAction>(new BattleActionViewer<UpgradeCardsAction>(this.ViewUpgradeCards));
			battle.ActionViewer.Unregister<PlayCardAction>(new BattleActionViewer<PlayCardAction>(this.ViewPlayCard));
			battle.CardUsingCanceled.RemoveHandler(new GameEventHandler<CardUsingEventArgs>(this.OnCardUsingCanceled), GameEventPriority.Lowest);
			this.ClearAll();
			this.Battle = null;
		}

		// Token: 0x06000783 RID: 1923 RVA: 0x00023210 File Offset: 0x00021410
		private void ClearAll()
		{
			this.DOKill(true);
			foreach (HandCard handCard in this._handWidgets)
			{
				Object.Destroy(handCard.gameObject);
			}
			this._handWidgets.Clear();
			foreach (CardWidget cardWidget in this._followPlayWidgets)
			{
				Object.Destroy(cardWidget.gameObject);
			}
			this._followPlayWidgets.Clear();
			foreach (CardWidget cardWidget2 in this._viewWidget)
			{
				Object.Destroy(cardWidget2.gameObject);
			}
			this._viewWidget.Clear();
			foreach (object obj in this.cardEffectLayer)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
			this.HideFastViewers();
		}

		// Token: 0x06000784 RID: 1924 RVA: 0x00023368 File Offset: 0x00021568
		private static void CheckKickerPrefer(IEnumerable<Card> cards)
		{
			UiManager.GetPanel<BattleManaPanel>().CheckKickerPrefer(cards);
		}

		// Token: 0x040004B2 RID: 1202
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);

		// Token: 0x040004B3 RID: 1203
		private List<HandCard> _handWidgets = new List<HandCard>();

		// Token: 0x040004B4 RID: 1204
		private readonly List<CardWidget> _followPlayWidgets = new List<CardWidget>();

		// Token: 0x040004B5 RID: 1205
		private readonly List<CardWidget> _viewWidget = new List<CardWidget>();

		// Token: 0x040004B6 RID: 1206
		private readonly List<HandCard> _pendingUseWidgets = new List<HandCard>();

		// Token: 0x040004B7 RID: 1207
		private readonly List<HandCard> _specialReactingWidgets = new List<HandCard>();

		// Token: 0x040004B8 RID: 1208
		private RectTransform _rectTransform;

		// Token: 0x040004B9 RID: 1209
		private PlayBoard _playBoard;

		// Token: 0x040004BA RID: 1210
		private float? _nextAdjustCardTime;

		// Token: 0x040004BB RID: 1211
		private const float ReshuffleTime = 0.5f;

		// Token: 0x040004BC RID: 1212
		private const int AddCardXDistance = 600;

		// Token: 0x040004BD RID: 1213
		private const float AddCardInTime = 0.3f;

		// Token: 0x040004BE RID: 1214
		private const float AddCardStopTime = 0.3f;

		// Token: 0x040004BF RID: 1215
		private const float AddCardOutTime = 0.4f;

		// Token: 0x040004C0 RID: 1216
		private const float RotateTime = 0.2f;

		// Token: 0x040004C1 RID: 1217
		private const float AddCardInterval = 0.05f;

		// Token: 0x040004C2 RID: 1218
		private const float AddCardIntervalSlow = 0.25f;

		// Token: 0x040004C5 RID: 1221
		private const float PendingUseInterval = 50f;

		// Token: 0x040004C6 RID: 1222
		private const float PendingUseIntervalFlatMax = 200f;

		// Token: 0x040004C7 RID: 1223
		private static readonly Vector2 HalfScreenV2 = new Vector2(1920f, 1080f);

		// Token: 0x040004C8 RID: 1224
		private int _drawCount;

		// Token: 0x040004C9 RID: 1225
		private int _drawUpperCount;

		// Token: 0x040004CA RID: 1226
		private readonly Color _followColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		// Token: 0x040004CB RID: 1227
		private readonly Color _dreamColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		// Token: 0x040004CC RID: 1228
		private int _discardCount;

		// Token: 0x040004CD RID: 1229
		private int _discardUpperCount;

		// Token: 0x040004CE RID: 1230
		private int _exileCount;

		// Token: 0x040004CF RID: 1231
		private const float FadeInDuration = 0.1f;

		// Token: 0x040004D0 RID: 1232
		private const float FadeOutDelay = 0.8f;

		// Token: 0x040004D1 RID: 1233
		private const float WaitTime = 0.5f;

		// Token: 0x040004D2 RID: 1234
		private const float FadeOutX = 50f;

		// Token: 0x040004D3 RID: 1235
		private readonly Vector3 _rightOffset = new Vector3(300f, 400f, 0f);

		// Token: 0x040004D4 RID: 1236
		private readonly Vector3 _leftOffset = new Vector3(-300f, 400f, 0f);

		// Token: 0x040004D5 RID: 1237
		private const float ViewCardXInterval = 100f;

		// Token: 0x040004D6 RID: 1238
		[SerializeField]
		private Transform drawRoot;

		// Token: 0x040004D7 RID: 1239
		[SerializeField]
		private Button drawButton;

		// Token: 0x040004D8 RID: 1240
		[SerializeField]
		private TextMeshProUGUI drawText;

		// Token: 0x040004D9 RID: 1241
		[SerializeField]
		private CardZoneUpperCountWidget drawUpperRoot;

		// Token: 0x040004DA RID: 1242
		[SerializeField]
		private TextMeshProUGUI drawUpperText;

		// Token: 0x040004DB RID: 1243
		[SerializeField]
		private Transform discardRoot;

		// Token: 0x040004DC RID: 1244
		[SerializeField]
		private Button discardButton;

		// Token: 0x040004DD RID: 1245
		[SerializeField]
		private TextMeshProUGUI discardText;

		// Token: 0x040004DE RID: 1246
		[SerializeField]
		private CardZoneUpperCountWidget discardUpperRoot;

		// Token: 0x040004DF RID: 1247
		[SerializeField]
		private TextMeshProUGUI discardUpperText;

		// Token: 0x040004E0 RID: 1248
		[SerializeField]
		private Transform exileRoot;

		// Token: 0x040004E1 RID: 1249
		[SerializeField]
		private Button exileButton;

		// Token: 0x040004E2 RID: 1250
		[SerializeField]
		private TextMeshProUGUI exileText;

		// Token: 0x040004E3 RID: 1251
		[SerializeField]
		private RectTransform cardHandParent;

		// Token: 0x040004E4 RID: 1252
		[SerializeField]
		private RectTransform cardHandReorderCache;

		// Token: 0x040004E5 RID: 1253
		[SerializeField]
		private RectTransform cardHoveredParent;

		// Token: 0x040004E6 RID: 1254
		[SerializeField]
		private RectTransform cardDrawPoint;

		// Token: 0x040004E7 RID: 1255
		[SerializeField]
		private RectTransform cardPendingUsePoint;

		// Token: 0x040004E8 RID: 1256
		[SerializeField]
		private RectTransform cardPendingUseCache;

		// Token: 0x040004E9 RID: 1257
		[SerializeField]
		private RectTransform cardFollowPlayPoint;

		// Token: 0x040004EA RID: 1258
		[SerializeField]
		private Transform cardEffectLayer;

		// Token: 0x040004EB RID: 1259
		[SerializeField]
		private CardWidget cardPrefab;

		// Token: 0x040004EC RID: 1260
		[SerializeField]
		private HandCard handCardPrefab;

		// Token: 0x040004ED RID: 1261
		[SerializeField]
		private Transform cardFlyHelperPrefab;

		// Token: 0x040004EE RID: 1262
		[SerializeField]
		private CardFlyBrief cardFlyBrief;

		// Token: 0x040004EF RID: 1263
		[SerializeField]
		private GameObject cardFlyTrail;

		// Token: 0x040004F0 RID: 1264
		[SerializeField]
		private GameObject cardUpgrade;

		// Token: 0x040004F1 RID: 1265
		[SerializeField]
		private GameObject pileTransformEffect;

		// Token: 0x040004F2 RID: 1266
		[SerializeField]
		[Range(0f, 1f)]
		private float curvatureRatio = 0.8f;

		// Token: 0x040004F3 RID: 1267
		[SerializeField]
		[Range(0f, 45f)]
		private float deltaRotate = 4f;

		// Token: 0x040004F4 RID: 1268
		[SerializeField]
		private int deltaX = 250;

		// Token: 0x040004F5 RID: 1269
		[SerializeField]
		private int hoveredY = 280;

		// Token: 0x040004F6 RID: 1270
		[SerializeField]
		private Vector2 handOffset = new Vector2(0f, -900f);

		// Token: 0x040004F7 RID: 1271
		[SerializeField]
		private FastDeckViewer fastDrawViewer;

		// Token: 0x040004F8 RID: 1272
		[SerializeField]
		private FastDeckViewer fastDiscardViewer;
	}
}
