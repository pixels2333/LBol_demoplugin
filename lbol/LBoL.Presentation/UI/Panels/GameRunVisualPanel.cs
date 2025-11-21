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
	// Token: 0x02000099 RID: 153
	public class GameRunVisualPanel : UiPanel
	{
		// Token: 0x17000154 RID: 340
		// (get) Token: 0x060007F3 RID: 2035 RVA: 0x00025705 File Offset: 0x00023905
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x060007F4 RID: 2036 RVA: 0x00025708 File Offset: 0x00023908
		private static Button DeckButton
		{
			get
			{
				return UiManager.GetPanel<SystemBoard>().deckButton;
			}
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x00025714 File Offset: 0x00023914
		protected override void OnEnterBattle()
		{
			base.Battle.ActionViewer.Register<GainMoneyAction>(new BattleActionViewer<GainMoneyAction>(this.ViewGainMoney), null);
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x00025733 File Offset: 0x00023933
		protected override void OnLeaveBattle()
		{
			base.Battle.ActionViewer.Unregister<GainMoneyAction>(new BattleActionViewer<GainMoneyAction>(this.ViewGainMoney));
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x00025751 File Offset: 0x00023951
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

		// Token: 0x060007F8 RID: 2040 RVA: 0x00025768 File Offset: 0x00023968
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

		// Token: 0x060007F9 RID: 2041 RVA: 0x00025B0C File Offset: 0x00023D0C
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

		// Token: 0x060007FA RID: 2042 RVA: 0x00025BB8 File Offset: 0x00023DB8
		public void DebugCardFly(int count)
		{
			Card[] array = Enumerable.ToArray<Card>(Enumerable.Select<ValueTuple<Type, CardConfig>, Card>(Library.EnumerateCardTypes().SampleMany(count, new Func<int, int, int>(Random.Range)), ([TupleElementNames(new string[] { "cardType", "config" })] ValueTuple<Type, CardConfig> t) => Library.CreateCard(t.Item1)));
			this.PlayAddToDeckEffect(array, null, 0f);
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x00025C14 File Offset: 0x00023E14
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

		// Token: 0x060007FC RID: 2044 RVA: 0x00025C90 File Offset: 0x00023E90
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

		// Token: 0x0400058B RID: 1419
		[SerializeField]
		private CardWidget cardPrefab;

		// Token: 0x0400058C RID: 1420
		[SerializeField]
		private CardFlyBrief cardFlyBrief;

		// Token: 0x0400058D RID: 1421
		[SerializeField]
		private GameObject cardFlyTrail;

		// Token: 0x0400058E RID: 1422
		[SerializeField]
		private GameObject cardUpgrade;

		// Token: 0x0400058F RID: 1423
		[SerializeField]
		private Transform cardFlyLayer;
	}
}
