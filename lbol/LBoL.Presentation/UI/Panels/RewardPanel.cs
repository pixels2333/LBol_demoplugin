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
	public class RewardPanel : UiPanel<ShowRewardContent>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		public Vector3 AbandonPos
		{
			get
			{
				return this.abandon.transform.position;
			}
		}
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
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<RewardInteraction>(new InteractionViewer<RewardInteraction>(this.ViewReward));
		}
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<RewardInteraction>(new InteractionViewer<RewardInteraction>(this.ViewReward));
		}
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
		public override void OnLocaleChanged()
		{
			this._defaultHeaders = "Reward.Headers".LocalizeStrings(true);
			this._cardHeader = "Reward.CardHeader".Localize(true);
			this._abandonString = "Reward.Abandon".Localize(true);
			this._abandonMoneyString = "Reward.AbandonMoney".Localize(true);
			this._abandonMaxHpString = "Reward.AbandonMaxHp".Localize(true);
		}
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
		private void RefreshNextButtonText()
		{
			if (this._rewardWidgets.Count > 0)
			{
				UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(3), null);
				return;
			}
			UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(base.GameRun.CurrentStation.IsStageEnd ? 1 : 0), null);
		}
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
		public Vector3 OnGainExhibitReward(Exhibit exhibit)
		{
			RewardWidget rewardWidget = this._rewardWidgets.Find((RewardWidget widget) => widget.StationReward.Exhibit == exhibit);
			Vector3 position = rewardWidget.transform.position;
			this._rewardWidgets.Remove(rewardWidget);
			Object.Destroy(rewardWidget.gameObject);
			this.ResetAllPositions(true);
			return position;
		}
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
		public CardWidget CloneCardWidget(CardWidget c)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(c, base.transform);
			cardWidget.Card = c.Card;
			return cardWidget;
		}
		[SerializeField]
		private GameObject rewardRoot;
		[SerializeField]
		private RewardWidget rewardTemplate;
		[SerializeField]
		private Transform rewardLayout;
		[SerializeField]
		private GameObject selectCardRoot;
		[SerializeField]
		private Transform selectCardLayout;
		[SerializeField]
		private CardWidget cardWidget;
		[SerializeField]
		private Button returnButton;
		[SerializeField]
		private Button abandonButton;
		[SerializeField]
		private TextMeshProUGUI header;
		[SerializeField]
		private TextMeshProUGUI abandon;
		[SerializeField]
		private TextMeshProUGUI abandonMoney;
		[SerializeField]
		private GameObject bg;
		[SerializeField]
		private GameObject specialBg;
		[SerializeField]
		private Button nextButton;
		private IList<string> _defaultHeaders;
		private string _cardHeader;
		private string _abandonString;
		private string _abandonMoneyString;
		private string _abandonMaxHpString;
		private readonly List<RewardWidget> _rewardWidgets = new List<RewardWidget>();
		private bool _isGainingExhibit;
		private CardWidget _bufferedCardWidget;
		private const float XStart = -2300f;
		private readonly float[] _xDistance = new float[] { 800f, 800f, 800f, 750f, 700f };
		private const float StartDelay = 0.1f;
		private const float FlyInTime = 0.45f;
		private const float PauseTime = 0.1f;
		private const float FlipTime = 0.35f;
		private const float CreateInterval = 0.05f;
		private const float FlipInterval = 0.05f;
		private const float Scale = 1.2f;
		private enum HeaderStatus
		{
			Default,
			Card
		}
	}
}
