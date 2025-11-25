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
	public class CardUi : MonoBehaviour
	{
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
		public IEnumerable<HandCard> GetUnpendingHands()
		{
			return Enumerable.Where<HandCard>(this._handWidgets, (HandCard hand) => !hand.PendingUse);
		}
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
		public int UnpendingHandCount
		{
			get
			{
				return Enumerable.Count<HandCard>(this._handWidgets, (HandCard hand) => !hand.PendingUse);
			}
		}
		private CardWidget CreateCardWidget(Card card, Transform parent)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardPrefab, parent);
			cardWidget.gameObject.name = "Card: " + card.Id;
			cardWidget.Card = card;
			return cardWidget;
		}
		private CardWidget CreateCardWidget(Card card)
		{
			return this.CreateCardWidget(card, this.cardHandParent);
		}
		private HandCard CreateHandWidget(Card card)
		{
			return this.ConvertToHand(this.CreateCardWidget(card));
		}
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
		public HandCard FindHandWidget(Card card)
		{
			return Enumerable.FirstOrDefault<HandCard>(this._handWidgets, (HandCard hand) => hand.Card == card);
		}
		public CardWidget FindFollowPlayWidget(Card card)
		{
			return Enumerable.FirstOrDefault<CardWidget>(this._followPlayWidgets, (CardWidget c) => c.Card == card);
		}
		public CardWidget FindViewWidget(Card card)
		{
			return Enumerable.FirstOrDefault<CardWidget>(this._viewWidget, (CardWidget c) => c.Card == card);
		}
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
		private static void FastRemoveHand(HandCard hand)
		{
			hand.transform.DOLocalMoveY(-100f, 0.2f, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>().SetLink(hand.gameObject)
				.OnComplete(delegate
				{
					Object.Destroy(hand.gameObject);
				});
		}
		private static void FastRemoveCard(CardWidget card)
		{
			card.transform.DOLocalMoveY(-100f, 0.2f, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>().SetLink(card.gameObject)
				.OnComplete(delegate
				{
					Object.Destroy(card.gameObject);
				});
		}
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
		private IEnumerator ViewDrawCard(DrawCardAction action)
		{
			return this.InternalViewDrawCard(action.Args.Card, action.TransitionType);
		}
		private IEnumerator ViewDrawSelectedCard(DrawSelectedCardAction action)
		{
			return this.InternalViewDrawCard(action.Args.Card, action.TransitionType);
		}
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
		public List<Vector2> CardBaseV2List { get; } = new List<Vector2>();
		public List<float> CardBaseRotateList { get; } = new List<float>();
		public void ConfirmUseCard(HandCard hand)
		{
			hand.PendingUse = true;
			this._pendingUseWidgets.Add(hand);
			this.AdjustPendingCardsLocation();
			this.AdjustCardsPosition(false);
		}
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
		private void ReOrderHands()
		{
			this._handWidgets = Enumerable.ToList<HandCard>(Enumerable.OrderBy<HandCard, int>(this._handWidgets, (HandCard hand) => this.Battle.HandZone.IndexOf(hand.Card)));
			foreach (HandCard handCard in this._handWidgets)
			{
				handCard.MoveToParentWhenReordering(this.cardHandReorderCache);
				handCard.MoveToParentWhenReordering(this.cardHandParent);
			}
		}
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
		private void RefreshExileCount()
		{
			this.ExileCount = this.Battle.ExileZone.Count;
		}
		private void RefreshAllZoneCount()
		{
			this.RefreshDrawCount();
			this.RefreshDiscardCount();
			this.RefreshExileCount();
		}
		public Vector3 DrawPosition
		{
			get
			{
				return this.drawRoot.transform.position;
			}
		}
		public Vector3 DiscardPosition
		{
			get
			{
				return this.discardRoot.transform.position;
			}
		}
		public Vector3 ExilePosition
		{
			get
			{
				return this.exileRoot.transform.position;
			}
		}
		public Vector3 DrawLocalPosition
		{
			get
			{
				return this.drawRoot.transform.localPosition;
			}
		}
		public Vector3 DiscardLocalPosition
		{
			get
			{
				return this.discardRoot.transform.localPosition;
			}
		}
		public Vector3 ExileLocalPosition
		{
			get
			{
				return this.exileRoot.transform.localPosition;
			}
		}
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
		public void ShowDiscardFastView()
		{
			if (Enumerable.Any<Card>(this.Battle.DiscardZone, (Card card) => card.IsDreamCard))
			{
				this.fastDiscardViewer.Show(Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.DiscardZone, (Card card) => card.IsDreamCard)));
			}
		}
		public void HideDrawFastView()
		{
			this.fastDrawViewer.Hide();
		}
		public void HideDiscardFastView()
		{
			this.fastDiscardViewer.Hide();
		}
		private void HideFastViewers()
		{
			this.HideDrawFastView();
			this.HideDiscardFastView();
		}
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
		private Vector3 PlayCardOffset
		{
			get
			{
				return new Vector3(100f * (float)(this._followPlayWidgets.Count - 1), 0f, 0f);
			}
		}
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
		private void OnCardUsingCanceled(CardUsingEventArgs args)
		{
			this.CancelUse(args.Card);
			UiManager.GetPanel<BattleManaPanel>().RefundFront(default(ManaGroup?));
		}
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
		private static void CheckKickerPrefer(IEnumerable<Card> cards)
		{
			UiManager.GetPanel<BattleManaPanel>().CheckKickerPrefer(cards);
		}
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);
		private List<HandCard> _handWidgets = new List<HandCard>();
		private readonly List<CardWidget> _followPlayWidgets = new List<CardWidget>();
		private readonly List<CardWidget> _viewWidget = new List<CardWidget>();
		private readonly List<HandCard> _pendingUseWidgets = new List<HandCard>();
		private readonly List<HandCard> _specialReactingWidgets = new List<HandCard>();
		private RectTransform _rectTransform;
		private PlayBoard _playBoard;
		private float? _nextAdjustCardTime;
		private const float ReshuffleTime = 0.5f;
		private const int AddCardXDistance = 600;
		private const float AddCardInTime = 0.3f;
		private const float AddCardStopTime = 0.3f;
		private const float AddCardOutTime = 0.4f;
		private const float RotateTime = 0.2f;
		private const float AddCardInterval = 0.05f;
		private const float AddCardIntervalSlow = 0.25f;
		private const float PendingUseInterval = 50f;
		private const float PendingUseIntervalFlatMax = 200f;
		private static readonly Vector2 HalfScreenV2 = new Vector2(1920f, 1080f);
		private int _drawCount;
		private int _drawUpperCount;
		private readonly Color _followColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		private readonly Color _dreamColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		private int _discardCount;
		private int _discardUpperCount;
		private int _exileCount;
		private const float FadeInDuration = 0.1f;
		private const float FadeOutDelay = 0.8f;
		private const float WaitTime = 0.5f;
		private const float FadeOutX = 50f;
		private readonly Vector3 _rightOffset = new Vector3(300f, 400f, 0f);
		private readonly Vector3 _leftOffset = new Vector3(-300f, 400f, 0f);
		private const float ViewCardXInterval = 100f;
		[SerializeField]
		private Transform drawRoot;
		[SerializeField]
		private Button drawButton;
		[SerializeField]
		private TextMeshProUGUI drawText;
		[SerializeField]
		private CardZoneUpperCountWidget drawUpperRoot;
		[SerializeField]
		private TextMeshProUGUI drawUpperText;
		[SerializeField]
		private Transform discardRoot;
		[SerializeField]
		private Button discardButton;
		[SerializeField]
		private TextMeshProUGUI discardText;
		[SerializeField]
		private CardZoneUpperCountWidget discardUpperRoot;
		[SerializeField]
		private TextMeshProUGUI discardUpperText;
		[SerializeField]
		private Transform exileRoot;
		[SerializeField]
		private Button exileButton;
		[SerializeField]
		private TextMeshProUGUI exileText;
		[SerializeField]
		private RectTransform cardHandParent;
		[SerializeField]
		private RectTransform cardHandReorderCache;
		[SerializeField]
		private RectTransform cardHoveredParent;
		[SerializeField]
		private RectTransform cardDrawPoint;
		[SerializeField]
		private RectTransform cardPendingUsePoint;
		[SerializeField]
		private RectTransform cardPendingUseCache;
		[SerializeField]
		private RectTransform cardFollowPlayPoint;
		[SerializeField]
		private Transform cardEffectLayer;
		[SerializeField]
		private CardWidget cardPrefab;
		[SerializeField]
		private HandCard handCardPrefab;
		[SerializeField]
		private Transform cardFlyHelperPrefab;
		[SerializeField]
		private CardFlyBrief cardFlyBrief;
		[SerializeField]
		private GameObject cardFlyTrail;
		[SerializeField]
		private GameObject cardUpgrade;
		[SerializeField]
		private GameObject pileTransformEffect;
		[SerializeField]
		[Range(0f, 1f)]
		private float curvatureRatio = 0.8f;
		[SerializeField]
		[Range(0f, 45f)]
		private float deltaRotate = 4f;
		[SerializeField]
		private int deltaX = 250;
		[SerializeField]
		private int hoveredY = 280;
		[SerializeField]
		private Vector2 handOffset = new Vector2(0f, -900f);
		[SerializeField]
		private FastDeckViewer fastDrawViewer;
		[SerializeField]
		private FastDeckViewer fastDiscardViewer;
	}
}
