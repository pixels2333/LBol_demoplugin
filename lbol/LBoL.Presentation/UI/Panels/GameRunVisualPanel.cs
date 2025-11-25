using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Adventure;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.Panels
{
	public class GameRunVisualPanel : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		private static Button DeckButton
		{
			get
			{
				return UiManager.GetPanel<SystemBoard>().deckButton;
			}
		}
		protected override void OnEnterBattle()
		{
			base.Battle.ActionViewer.Register<GainMoneyAction>(new BattleActionViewer<GainMoneyAction>(this.ViewGainMoney), null);
		}
		protected override void OnLeaveBattle()
		{
			base.Battle.ActionViewer.Unregister<GainMoneyAction>(new BattleActionViewer<GainMoneyAction>(this.ViewGainMoney));
		}
		private IEnumerator ViewGainMoney(GainMoneyAction action)
		{
			Vector3 vector;
			switch (action.SpecialSource)
			{
			case SpecialSourceType.None:
				vector = UiManager.GetPanel<PlayBoard>().FindActionSourceWorldPosition(action.Source) ?? Vector3.zero;
				break;
			case SpecialSourceType.DrawZone:
				vector = UiManager.GetPanel<PlayBoard>().CardUi.DrawPosition;
				break;
			case SpecialSourceType.DisCardZone:
				vector = UiManager.GetPanel<PlayBoard>().CardUi.DiscardPosition;
				break;
			default:
				vector = Vector3.zero;
				break;
			}
			UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(vector, action.Money, base.transform, 1f);
			yield return new WaitForSeconds(1f);
			if (action.Source is GainTreasure)
			{
				base.GameRun.Stats.TotalGainTreasure += action.Money;
				if (base.GameRun.Stats.TotalGainTreasure >= 500 && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					GameMaster.UnlockAchievement(AchievementKey.MikeAdventure);
				}
			}
			yield break;
		}
		public void PlayAddToDeckEffect(IEnumerable<Card> cards, IEnumerable<CardWidget> sourceWidgets = null, float waitingTime = 0f)
		{
			List<Card> list = Enumerable.ToList<Card>(cards);
			using (IEnumerator<CardWidget> enumerator = (sourceWidgets ?? Enumerable.Empty<CardWidget>()).GetEnumerator())
			{
				int count = list.Count;
				int num = 0;
				foreach (ValueTuple<int, Card> valueTuple in list.WithIndices<Card>())
				{
					int item = valueTuple.Item1;
					Card item2 = valueTuple.Item2;
					Transform parent = Utils.CreateGameObject(this.cardFlyLayer, "CardFlyParent").transform;
					CardWidget widget = (enumerator.MoveNext() ? enumerator.Current : null);
					if (widget != null)
					{
						if (widget.Card != item2)
						{
							Debug.LogError(string.Concat(new string[] { "$[GameRunVisualPanel] Source widget card ", item2.DebugName, " is not the card adding (", item2.DebugName, ")" }));
							widget.Card = item2;
						}
						parent.position = widget.transform.position;
						widget.transform.SetParent(parent);
					}
					else
					{
						parent.localPosition = new Vector3((float)((count - 1) * -200 + num * 400), 0f, 0f);
						num++;
						widget = Object.Instantiate<CardWidget>(this.cardPrefab, parent);
						widget.Card = item2;
					}
					ShowingCard showingCard;
					if (widget.TryGetComponent<ShowingCard>(out showingCard))
					{
						showingCard.enabled = false;
					}
					Transform transform = widget.transform;
					GameObject gameObject = Object.Instantiate<GameObject>(this.cardFlyTrail, parent);
					AudioManager.PlayUi("CardFly", false);
					int num2 = Random.Range(100, 200);
					DOTween.Sequence().Append(gameObject.GetOrAddComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false)).AppendInterval(waitingTime + 0.05f * (float)item)
						.Append(parent.DOMove(GameRunVisualPanel.DeckButton.transform.position, 0.4f, false).SetEase(Ease.InSine).OnComplete(delegate
						{
							Object.Destroy(widget.gameObject);
						}))
						.Join(transform.DOLocalRotate(new Vector3(0f, 0f, -500f), 0.2f, RotateMode.FastBeyond360))
						.Join(transform.DOScale(new Vector3(0.2f, 0.2f, 0f), 0.2f))
						.Join(transform.DOLocalMoveY((float)num2, 0.2f, false).SetEase(Ease.OutSine).SetRelative(true)
							.SetLoops(2, LoopType.Yoyo))
						.Join(gameObject.transform.DOLocalMoveY((float)num2, 0.2f, false).SetEase(Ease.OutSine).SetRelative(true)
							.SetLoops(2, LoopType.Yoyo))
						.AppendCallback(delegate
						{
							GameRunVisualPanel.ImageBlink(GameRunVisualPanel.DeckButton.image);
							UiManager.GetPanel<SystemBoard>().OnDeckChanged();
						})
						.AppendInterval(2f)
						.AppendCallback(delegate
						{
							Object.Destroy(parent.gameObject);
						})
						.SetUpdate(true)
						.SetLink(base.gameObject);
				}
			}
		}
		public void ViewRemoveDeckCards(IEnumerable<Card> cards)
		{
			List<Card> list = Enumerable.ToList<Card>(cards);
			int count = list.Count;
			foreach (ValueTuple<int, Card> valueTuple in list.WithIndices<Card>())
			{
				int item = valueTuple.Item1;
				Card item2 = valueTuple.Item2;
				CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardPrefab, this.cardFlyLayer);
				cardWidget.Card = item2;
				cardWidget.RectTransform.localPosition = new Vector3(((float)item + (float)(1 - count) / 2f) * 400f, 0f, 0f);
				cardWidget.Remove();
			}
		}
		public void DebugCardFly(int count)
		{
			Card[] array = Enumerable.ToArray<Card>(Enumerable.Select<ValueTuple<Type, CardConfig>, Card>(Library.EnumerateCardTypes().SampleMany(count, new Func<int, int, int>(Random.Range)), ([TupleElementNames(new string[] { "cardType", "config" })] ValueTuple<Type, CardConfig> t) => Library.CreateCard(t.Item1)));
			this.PlayAddToDeckEffect(array, null, 0f);
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
		public void PlayUpgradeDeckCardsEffect(Card[] cards, float waitingTime = 0f)
		{
			int num = cards.Length;
			Sequence sequence = DOTween.Sequence();
			foreach (ValueTuple<int, Card> valueTuple in cards.WithIndices<Card>())
			{
				int item = valueTuple.Item1;
				Card item2 = valueTuple.Item2;
				CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardPrefab, this.cardFlyLayer);
				Object.Instantiate<GameObject>(this.cardUpgrade, cardWidget.transform);
				cardWidget.Card = item2;
				Transform transform = cardWidget.transform;
				transform.localPosition = new Vector3((float)((num - 1) * -200 + item * 400), 0f, 0f);
				Transform transform2 = Utils.CreateGameObject(this.cardFlyLayer, "CardParent").transform;
				transform2.localPosition = transform.localPosition;
				transform.SetParent(transform2);
				transform.DOScale(1f, 0.4f).From(0f, true, false).SetAutoKill(true)
					.SetTarget(this)
					.SetEase(Ease.OutCubic);
				AudioManager.PlayUi("UpgradeCardTemp", false);
				sequence.Join(transform.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).From(1f, true, false));
				Object.Destroy(cardWidget.gameObject, 2f);
				Object.Destroy(transform2.gameObject, 2f);
			}
			sequence.SetUpdate(true).SetAutoKill(true).SetTarget(this)
				.SetDelay(waitingTime);
		}
		[SerializeField]
		private CardWidget cardPrefab;
		[SerializeField]
		private CardFlyBrief cardFlyBrief;
		[SerializeField]
		private GameObject cardFlyTrail;
		[SerializeField]
		private GameObject cardUpgrade;
		[SerializeField]
		private Transform cardFlyLayer;
	}
}
