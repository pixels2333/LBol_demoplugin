using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000AD RID: 173
	public class RewardPanel : UiPanel<ShowRewardContent>
	{
		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000981 RID: 2433 RVA: 0x000306CD File Offset: 0x0002E8CD
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000982 RID: 2434 RVA: 0x000306D0 File Offset: 0x0002E8D0
		public Vector3 AbandonPos
		{
			get
			{
				return this.abandon.transform.position;
			}
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000983 RID: 2435 RVA: 0x000306E2 File Offset: 0x0002E8E2
		// (set) Token: 0x06000984 RID: 2436 RVA: 0x000306EF File Offset: 0x0002E8EF
		private bool RewardVisible
		{
			get
			{
				return this.rewardRoot.activeSelf;
			}
			set
			{
				this.rewardRoot.SetActive(value);
			}
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000985 RID: 2437 RVA: 0x000306FD File Offset: 0x0002E8FD
		// (set) Token: 0x06000986 RID: 2438 RVA: 0x0003070A File Offset: 0x0002E90A
		private bool CardVisible
		{
			get
			{
				return this.selectCardRoot.activeSelf;
			}
			set
			{
				this.selectCardRoot.SetActive(value);
			}
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x00030718 File Offset: 0x0002E918
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<RewardInteraction>(new InteractionViewer<RewardInteraction>(this.ViewReward));
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x00030736 File Offset: 0x0002E936
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<RewardInteraction>(new InteractionViewer<RewardInteraction>(this.ViewReward));
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x00030754 File Offset: 0x0002E954
		private IEnumerator ViewReward(RewardInteraction interaction)
		{
			ShowRewardContent showRewardContent = new ShowRewardContent
			{
				RewardType = RewardType.Other
			};
			List<StationReward> list = Enumerable.ToList<StationReward>(Enumerable.Select<Exhibit, StationReward>(interaction.PendingExhibits, new Func<Exhibit, StationReward>(StationReward.CreateExhibit)));
			showRewardContent.Rewards = list;
			base.Show(showRewardContent);
			UiManager.GetPanel<VnPanel>().SetNextButton(false, default(int?), null);
			this.nextButton.gameObject.SetActive(true);
			this.specialBg.gameObject.SetActive(true);
			yield return new WaitUntil(() => !base.gameObject.activeSelf || (this._rewardWidgets.Count == 0 && !this._isGainingExhibit));
			if (base.gameObject.activeSelf)
			{
				base.Hide();
			}
			yield break;
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x0003076C File Offset: 0x0002E96C
		public override void OnLocaleChanged()
		{
			this._defaultHeaders = "Reward.Headers".LocalizeStrings(true);
			this._cardHeader = "Reward.CardHeader".Localize(true);
			this._abandonString = "Reward.Abandon".Localize(true);
			this._abandonMoneyString = "Reward.AbandonMoney".Localize(true);
			this._abandonMaxHpString = "Reward.AbandonMaxHp".Localize(true);
		}

		// Token: 0x0600098B RID: 2443 RVA: 0x000307D0 File Offset: 0x0002E9D0
		protected override void OnShowing(ShowRewardContent showRewardContent)
		{
			this.specialBg.gameObject.SetActive(false);
			this.nextButton.gameObject.SetActive(false);
			this.SetHeader(RewardPanel.HeaderStatus.Default);
			if (showRewardContent.RewardType == RewardType.Station)
			{
				Station station = showRewardContent.Station;
				List<StationReward> list = station.Rewards;
				if (list == null)
				{
					Debug.LogError("Showing empty rewards");
					list = new List<StationReward>();
				}
				foreach (StationReward stationReward in list)
				{
					RewardWidget rewardWidget2 = Object.Instantiate<RewardWidget>(this.rewardTemplate, this.rewardLayout);
					rewardWidget2.StationReward = stationReward;
					rewardWidget2.Click += delegate
					{
						this.StartCoroutine(this.CoAcquireReward(station, rewardWidget2));
					};
					this._rewardWidgets.Add(rewardWidget2);
				}
				if (showRewardContent.ShowNextButton)
				{
					UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(0), new Action(base.Hide));
				}
				this.RefreshNextButtonText();
			}
			else
			{
				using (List<StationReward>.Enumerator enumerator = showRewardContent.Rewards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StationReward reward = enumerator.Current;
						RewardWidget rewardWidget = Object.Instantiate<RewardWidget>(this.rewardTemplate, this.rewardLayout);
						rewardWidget.StationReward = reward;
						rewardWidget.Click += delegate
						{
							if (reward.Type == StationRewardType.Money)
							{
								UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(rewardWidget.transform.position, reward.Money, this.transform, 1f);
								this.GameRun.GainMoney(reward.Money, true, new VisualSourceData
								{
									SourceType = VisualSourceType.Reward,
									Index = this._rewardWidgets.IndexOf(rewardWidget)
								});
							}
							else if (reward.Type == StationRewardType.Exhibit)
							{
								this.StartCoroutine(this.CoGainExhibit(reward.Exhibit, this._rewardWidgets.IndexOf(rewardWidget)));
							}
							this._rewardWidgets.Remove(rewardWidget);
							Object.Destroy(rewardWidget.gameObject);
							this.ResetAllPositions(true);
						};
						this._rewardWidgets.Add(rewardWidget);
					}
				}
			}
			this.ResetAllPositions(false);
			this.bg.transform.DOScaleY(1f, 0.2f).From(0.7f, true, false).SetUpdate(true);
			CanvasGroup component = base.GetComponent<CanvasGroup>();
			component.interactable = true;
			component.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true);
			this._isGainingExhibit = false;
			this.RewardVisible = true;
			this.CardVisible = false;
		}

		// Token: 0x0600098C RID: 2444 RVA: 0x00030A30 File Offset: 0x0002EC30
		private IEnumerator CoGainExhibit(Exhibit rewardExhibit, int indexOf)
		{
			this._isGainingExhibit = true;
			base.GetComponent<CanvasGroup>().interactable = false;
			yield return base.GameRun.GainExhibitRunner(rewardExhibit, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Reward,
				Index = indexOf
			});
			base.GetComponent<CanvasGroup>().interactable = true;
			this._isGainingExhibit = false;
			yield break;
		}

		// Token: 0x0600098D RID: 2445 RVA: 0x00030A50 File Offset: 0x0002EC50
		protected override void OnHided()
		{
			base.transform.DOKill(false);
			this.rewardLayout.DestroyChildren();
			this.selectCardLayout.DestroyChildren();
			this._rewardWidgets.Clear();
			if (this._bufferedCardWidget)
			{
				Object.Destroy(this._bufferedCardWidget.gameObject);
				this._bufferedCardWidget = null;
			}
		}

		// Token: 0x0600098E RID: 2446 RVA: 0x00030AB0 File Offset: 0x0002ECB0
		private void RefreshNextButtonText()
		{
			if (this._rewardWidgets.Count > 0)
			{
				UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(3), null);
				return;
			}
			UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(base.GameRun.CurrentStation.IsStageEnd ? 1 : 0), null);
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x00030B05 File Offset: 0x0002ED05
		private IEnumerator CoAcquireReward(Station station, RewardWidget rewardWidget)
		{
			StationReward stationReward = rewardWidget.StationReward;
			switch (stationReward.Type)
			{
			case StationRewardType.Money:
				AudioManager.PlayUi("MoneyAcquire", false);
				UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(rewardWidget.transform.position, stationReward.Money, UiManager.GetPanel<GameRunVisualPanel>().transform, 1f);
				this._rewardWidgets.Remove(rewardWidget);
				yield return station.AcquireRewardRunner(stationReward);
				Object.Destroy(rewardWidget.gameObject);
				this.ResetAllPositions(true);
				break;
			case StationRewardType.Card:
				this.ShowCardSelection(station, rewardWidget);
				break;
			case StationRewardType.Exhibit:
				yield return station.AcquireRewardRunner(stationReward);
				break;
			case StationRewardType.Tool:
				this._bufferedCardWidget = rewardWidget.ToolCardWidget;
				yield return station.AcquireRewardRunner(stationReward);
				this._rewardWidgets.Remove(rewardWidget);
				Object.Destroy(rewardWidget.gameObject);
				break;
			case StationRewardType.RemoveCard:
			{
				List<Card> list = Enumerable.ToList<Card>(base.GameRun.BaseDeckInBossRemoveReward);
				if (list.Count > 0)
				{
					ShowCardsPanel panel = UiManager.GetPanel<ShowCardsPanel>();
					yield return panel.ShowAsync(new ShowCardsPayload
					{
						Name = "Game.Deck".Localize(true),
						Description = "Cards.RemoveTips".Localize(true),
						Cards = list,
						CanCancel = true,
						InteractionType = InteractionType.Remove
					});
					if (!panel.IsCanceled)
					{
						Card selectedCard = panel.SelectedCard;
						base.GameRun.RemoveDeckCard(selectedCard, true);
						this._rewardWidgets.Remove(rewardWidget);
						Object.Destroy(rewardWidget.gameObject);
					}
					panel = null;
				}
				else
				{
					Debug.LogWarning("No card is non-basic and non-misfortune, should not generate remove card reward.");
					this._rewardWidgets.Remove(rewardWidget);
					Object.Destroy(rewardWidget.gameObject);
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.RefreshNextButtonText();
			yield break;
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x00030B24 File Offset: 0x0002ED24
		public Vector3 OnGainExhibitReward(Exhibit exhibit)
		{
			RewardWidget rewardWidget = this._rewardWidgets.Find((RewardWidget widget) => widget.StationReward.Exhibit == exhibit);
			Vector3 position = rewardWidget.transform.position;
			this._rewardWidgets.Remove(rewardWidget);
			Object.Destroy(rewardWidget.gameObject);
			this.ResetAllPositions(true);
			return position;
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x00030B80 File Offset: 0x0002ED80
		private void ShowCardSelection(Station station, RewardWidget rewardWidget)
		{
			RewardPanel.<>c__DisplayClass52_0 CS$<>8__locals1 = new RewardPanel.<>c__DisplayClass52_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.rewardWidget = rewardWidget;
			CS$<>8__locals1.station = station;
			this.selectCardRoot.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false)
				.SetEase(Ease.OutCubic)
				.SetUpdate(true);
			this.abandonButton.interactable = true;
			List<Card> list = CS$<>8__locals1.rewardWidget.StationReward.Cards;
			CS$<>8__locals1.total = list.Count;
			if (CS$<>8__locals1.total > 5)
			{
				CS$<>8__locals1.total = 5;
				list = Enumerable.ToList<Card>(Enumerable.Take<Card>(list, CS$<>8__locals1.total));
				Debug.LogError("Too many cards in reward.");
			}
			CS$<>8__locals1.widgets = new List<CardWidget>();
			foreach (Card card in list)
			{
				CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardWidget, this.selectCardLayout);
				cardWidget.Card = card;
				cardWidget.name = "Card:" + card.Name;
				Transform transform = cardWidget.transform;
				transform.localPosition = new Vector3(-2300f, 0f, 0f);
				transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				transform.localScale = new Vector3(1.2f, 1.2f, 1f);
				CS$<>8__locals1.widgets.Add(cardWidget);
			}
			CS$<>8__locals1.slidePlayed = false;
			Sequence sequence = DOTween.Sequence();
			foreach (ValueTuple<int, CardWidget> valueTuple in CS$<>8__locals1.widgets.WithIndices<CardWidget>())
			{
				int i = valueTuple.Item1;
				Transform transform2 = valueTuple.Item2.transform;
				Sequence sequence2 = DOTween.Sequence().AppendInterval(0.1f + (float)i * 0.05f);
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
				Sequence sequence3 = sequence2.AppendCallback(tweenCallback).Append(transform2.DOLocalMoveX(this._xDistance[CS$<>8__locals1.total - 1] * ((float)i - (float)(CS$<>8__locals1.total - 1) / 2f), 0.45f, false).SetEase(Ease.OutCubic)).AppendInterval(0.1f + (float)i * 0.05f)
					.AppendCallback(delegate
					{
						AudioManager.PlayUi("CardFlip_" + (5 - CS$<>8__locals1.total + i + 1).ToString(), false);
					})
					.Append(transform2.DOLocalRotate(Vector3.zero, 0.35f, RotateMode.Fast).SetEase(Ease.OutQuad))
					.SetUpdate(true);
				sequence.Join(sequence3);
			}
			sequence.SetTarget(this.selectCardRoot).SetLink(this.selectCardRoot).SetAutoKill(true)
				.SetUpdate(true)
				.OnComplete(delegate
				{
					Func<UniTask> func;
					if ((func = CS$<>8__locals1.<>9__4) == null)
					{
						func = (CS$<>8__locals1.<>9__4 = delegate
						{
							RewardPanel.<>c__DisplayClass52_0.<<ShowCardSelection>b__4>d <<ShowCardSelection>b__4>d;
							<<ShowCardSelection>b__4>d.<>t__builder = AsyncUniTaskMethodBuilder.Create();
							<<ShowCardSelection>b__4>d.<>4__this = CS$<>8__locals1;
							<<ShowCardSelection>b__4>d.<>1__state = -1;
							<<ShowCardSelection>b__4>d.<>t__builder.Start<RewardPanel.<>c__DisplayClass52_0.<<ShowCardSelection>b__4>d>(ref <<ShowCardSelection>b__4>d);
							return <<ShowCardSelection>b__4>d.<>t__builder.Task;
						});
					}
					UniTask.Create(func);
				});
			this.abandonButton.onClick.RemoveAllListeners();
			this.abandonButton.onClick.AddListener(delegate
			{
				CS$<>8__locals1.<>4__this.CardSelectAbandon(CS$<>8__locals1.rewardWidget, CS$<>8__locals1.widgets);
			});
			this.abandonMoney.text = string.Format(this._abandonMoneyString, base.GameRun.RewardCardAbandonMoney);
			if (base.GameRun != null && base.GameRun.Player.HasExhibit<XiaorenWan>())
			{
				XiaorenWan exhibit = base.GameRun.Player.GetExhibit<XiaorenWan>();
				TextMeshProUGUI textMeshProUGUI = this.abandonMoney;
				textMeshProUGUI.text += "\n";
				TextMeshProUGUI textMeshProUGUI2 = this.abandonMoney;
				textMeshProUGUI2.text += string.Format(this._abandonMaxHpString, exhibit.Value1);
			}
			this.abandon.text = this._abandonString;
			this.returnButton.onClick.RemoveAllListeners();
			this.returnButton.onClick.AddListener(new UnityAction(this.CardSelectReturn));
			this.SetHeader(RewardPanel.HeaderStatus.Card);
			this.RewardVisible = false;
			this.CardVisible = true;
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00030FF4 File Offset: 0x0002F1F4
		private void SetHeader(RewardPanel.HeaderStatus status)
		{
			TextMeshProUGUI textMeshProUGUI = this.header;
			string text;
			if (status != RewardPanel.HeaderStatus.Default)
			{
				if (status != RewardPanel.HeaderStatus.Card)
				{
					throw new ArgumentOutOfRangeException("status", status, null);
				}
				text = this._cardHeader;
			}
			else
			{
				text = this._defaultHeaders[Random.Range(0, this._defaultHeaders.Count)];
			}
			textMeshProUGUI.text = text;
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00031054 File Offset: 0x0002F254
		private void CardSelectAbandon(RewardWidget rewardWidget, List<CardWidget> cardWidgets)
		{
			this._rewardWidgets.Remove(rewardWidget);
			this.abandonButton.interactable = false;
			foreach (CardWidget cardWidget in cardWidgets)
			{
				Button component = cardWidget.GetComponent<Button>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
			Singleton<GameMaster>.Instance.CurrentGameRun.AbandonReward(rewardWidget.StationReward);
			Object.Destroy(rewardWidget.gameObject);
			this.CardSelectReturn();
			this.ResetAllPositions(true);
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x000310F8 File Offset: 0x0002F2F8
		private void CardSelectReturn()
		{
			this.RewardVisible = true;
			DOTween.Kill(this.selectCardRoot, false);
			TweenerCore<float, float, FloatOptions> tweenerCore = this.selectCardRoot.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).SetUpdate(true)
				.SetAutoKill(true)
				.From(1f, true, false);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, delegate
			{
				this.selectCardLayout.DestroyChildren();
				this.SetHeader(RewardPanel.HeaderStatus.Default);
				this.CardVisible = false;
			});
			this.RefreshNextButtonText();
			this.ResetAllPositions(true);
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x0003117C File Offset: 0x0002F37C
		private void ResetAllPositions(bool approaching = true)
		{
			if (this._rewardWidgets.Count > 0)
			{
				float num = (float)(-(float)(this._rewardWidgets.Count - 1)) * 750f / 2f;
				foreach (ValueTuple<int, RewardWidget> valueTuple in this._rewardWidgets.WithIndices<RewardWidget>())
				{
					int item = valueTuple.Item1;
					RewardWidget item2 = valueTuple.Item2;
					if (approaching)
					{
						item2.ApproachingPosition = new Vector3?(new Vector3(num + (float)item * 750f, 0f));
					}
					else
					{
						item2.transform.localPosition = new Vector3(num + (float)item * 750f, 0f);
					}
				}
			}
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x00031244 File Offset: 0x0002F444
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

		// Token: 0x06000997 RID: 2455 RVA: 0x00031272 File Offset: 0x0002F472
		public CardWidget CloneCardWidget(CardWidget c)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(c, base.transform);
			cardWidget.Card = c.Card;
			return cardWidget;
		}

		// Token: 0x04000701 RID: 1793
		[SerializeField]
		private GameObject rewardRoot;

		// Token: 0x04000702 RID: 1794
		[SerializeField]
		private RewardWidget rewardTemplate;

		// Token: 0x04000703 RID: 1795
		[SerializeField]
		private Transform rewardLayout;

		// Token: 0x04000704 RID: 1796
		[SerializeField]
		private GameObject selectCardRoot;

		// Token: 0x04000705 RID: 1797
		[SerializeField]
		private Transform selectCardLayout;

		// Token: 0x04000706 RID: 1798
		[SerializeField]
		private CardWidget cardWidget;

		// Token: 0x04000707 RID: 1799
		[SerializeField]
		private Button returnButton;

		// Token: 0x04000708 RID: 1800
		[SerializeField]
		private Button abandonButton;

		// Token: 0x04000709 RID: 1801
		[SerializeField]
		private TextMeshProUGUI header;

		// Token: 0x0400070A RID: 1802
		[SerializeField]
		private TextMeshProUGUI abandon;

		// Token: 0x0400070B RID: 1803
		[SerializeField]
		private TextMeshProUGUI abandonMoney;

		// Token: 0x0400070C RID: 1804
		[SerializeField]
		private GameObject bg;

		// Token: 0x0400070D RID: 1805
		[SerializeField]
		private GameObject specialBg;

		// Token: 0x0400070E RID: 1806
		[SerializeField]
		private Button nextButton;

		// Token: 0x0400070F RID: 1807
		private IList<string> _defaultHeaders;

		// Token: 0x04000710 RID: 1808
		private string _cardHeader;

		// Token: 0x04000711 RID: 1809
		private string _abandonString;

		// Token: 0x04000712 RID: 1810
		private string _abandonMoneyString;

		// Token: 0x04000713 RID: 1811
		private string _abandonMaxHpString;

		// Token: 0x04000714 RID: 1812
		private readonly List<RewardWidget> _rewardWidgets = new List<RewardWidget>();

		// Token: 0x04000715 RID: 1813
		private bool _isGainingExhibit;

		// Token: 0x04000716 RID: 1814
		private CardWidget _bufferedCardWidget;

		// Token: 0x04000717 RID: 1815
		private const float XStart = -2300f;

		// Token: 0x04000718 RID: 1816
		private readonly float[] _xDistance = new float[] { 800f, 800f, 800f, 750f, 700f };

		// Token: 0x04000719 RID: 1817
		private const float StartDelay = 0.1f;

		// Token: 0x0400071A RID: 1818
		private const float FlyInTime = 0.45f;

		// Token: 0x0400071B RID: 1819
		private const float PauseTime = 0.1f;

		// Token: 0x0400071C RID: 1820
		private const float FlipTime = 0.35f;

		// Token: 0x0400071D RID: 1821
		private const float CreateInterval = 0.05f;

		// Token: 0x0400071E RID: 1822
		private const float FlipInterval = 0.05f;

		// Token: 0x0400071F RID: 1823
		private const float Scale = 1.2f;

		// Token: 0x02000297 RID: 663
		private enum HeaderStatus
		{
			// Token: 0x0400119A RID: 4506
			Default,
			// Token: 0x0400119B RID: 4507
			Card
		}
	}
}
