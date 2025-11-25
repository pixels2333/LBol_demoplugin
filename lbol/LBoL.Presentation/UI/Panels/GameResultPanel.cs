using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class GameResultPanel : UiPanel<GameResultData>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
		public bool ResultVisible
		{
			get
			{
				return this._resultVisible;
			}
			set
			{
				this._resultVisible = value;
				this.resultTitle.gameObject.SetActive(value);
			}
		}
		public bool DesVisible
		{
			get
			{
				return this._desVisible;
			}
			set
			{
				this._desVisible = value;
				this.resultDescription.gameObject.SetActive(value);
			}
		}
		public bool ReturnVisible
		{
			get
			{
				return this._returnVisible;
			}
			set
			{
				this._returnVisible = value;
				this.returnButton.gameObject.SetActive(value);
				this.tabToggleRoot.gameObject.SetActive(value);
			}
		}
		public override async UniTask CustomLocalizationAsync()
		{
			this._winString = "GameResult.Win".Localize(true);
			this._loseString = "GameResult.Lose".Localize(true);
			this._normalString = "GameResult.NormalEnd".Localize(true);
			this._trueEndFailString = "GameResult.TrueEndFail".Localize(true);
			this._trueEndString = "GameResult.TrueEnd".Localize(true);
			this._badString = "GameResult.BadEnd".Localize(true);
			Dictionary<string, GameResultPanel.StringTableEntry> dictionary = await Localization.LoadFileAsync<GameResultPanel.StringTableEntry>("BluePoint");
			this._stringTable = dictionary;
		}
		public void Awake()
		{
			this.ResultVisible = false;
			this.DesVisible = false;
			this.ReturnVisible = false;
			this.returnButton.onClick.AddListener(new UnityAction(base.Hide));
			this.skipButton.onClick.AddListener(delegate
			{
				Sequence curRunner = this._curRunner;
				if (curRunner == null)
				{
					return;
				}
				curRunner.Complete(true);
			});
			this.levelUpBgButton.onClick.AddListener(delegate
			{
				this.levelUpRoot.gameObject.SetActive(false);
			});
			this.tabDetail.onClick.AddListener(delegate
			{
				DOTween.Sequence().AppendCallback(delegate
				{
					this.detailRoot.transform.localScale = Vector3.one;
					this.detailRoot.gameObject.SetActive(true);
					this.tabDetail.interactable = false;
					this.tabFold.interactable = false;
					this.tabFold.GetComponent<CanvasGroup>().alpha = 0f;
					this.tabFold.gameObject.SetActive(true);
				}).Join(this.tabToggleRoot.GetComponent<RectTransform>().DOAnchorPosY(-800f, 0.6f, false))
					.Join(this.expRoot.DOFade(0f, 0.4f))
					.Join(this.expRoot.transform.DOScaleY(0f, 0.6f))
					.Join(this.detailRoot.DOFade(1f, 0.4f))
					.Join(this.tabDetail.GetComponent<CanvasGroup>().DOFade(0f, 0.4f))
					.Join(this.tabFold.GetComponent<CanvasGroup>().DOFade(1f, 0.4f))
					.OnComplete(delegate
					{
						this.tabDetail.gameObject.SetActive(false);
						this.tabFold.interactable = true;
						this.expRoot.gameObject.SetActive(false);
					})
					.SetLink(base.gameObject)
					.SetUpdate(true);
			});
			this.tabFold.onClick.AddListener(delegate
			{
				DOTween.Sequence().AppendCallback(delegate
				{
					this.expRoot.transform.localScale = Vector3.one;
					this.expRoot.gameObject.SetActive(true);
					this.tabDetail.interactable = false;
					this.tabFold.interactable = false;
					this.tabDetail.GetComponent<CanvasGroup>().alpha = 0f;
					this.tabDetail.gameObject.SetActive(true);
				}).Join(this.tabToggleRoot.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.6f, false))
					.Join(this.expRoot.DOFade(1f, 0.4f))
					.Join(this.detailRoot.transform.DOScaleY(0f, 0.6f))
					.Join(this.detailRoot.DOFade(0f, 0.4f))
					.Join(this.tabDetail.GetComponent<CanvasGroup>().DOFade(1f, 0.4f))
					.Join(this.tabFold.GetComponent<CanvasGroup>().DOFade(0f, 0.4f))
					.OnComplete(delegate
					{
						this.tabFold.gameObject.SetActive(false);
						this.tabDetail.interactable = true;
						this.detailRoot.gameObject.SetActive(false);
					})
					.SetLink(base.gameObject)
					.SetUpdate(true);
			});
			this.levelUpRoot.gameObject.SetActive(false);
			foreach (CardWidget cardWidget in this.levelUpCards)
			{
				cardWidget.GetComponent<ShowingCard>().SetScale(1.2f, 1.4f);
			}
		}
		private void RefreshExp()
		{
			this._curMaxExp = ExpHelper.GetExpForLevel(this._curLevel);
			this.expText.text = string.Format("{0}/{1}", this._curExp, this._curMaxExp);
			this.expSlider.fillAmount = (float)this._curExp / (float)this._curMaxExp;
		}
		protected override void OnShowing(GameResultData payload)
		{
			this.scoreGroup.gameObject.SetActive(false);
			this.expRoot.gameObject.SetActive(false);
			this.deltaScoreText.gameObject.SetActive(false);
			this.tabToggleRoot.gameObject.SetActive(false);
			this.tabFold.gameObject.SetActive(false);
			switch (payload.Type)
			{
			case GameResultType.Failure:
				this.Lose();
				break;
			case GameResultType.NormalEnd:
				this.Win(GameResultType.NormalEnd);
				break;
			case GameResultType.TrueEndFail:
				this.Win(GameResultType.TrueEndFail);
				break;
			case GameResultType.TrueEnd:
				this.Win(GameResultType.TrueEnd);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (payload.Type == GameResultType.Failure)
			{
				this.portrait.sprite = this.characterDefeatPortraits[payload.PlayerId];
				this.shadow.sprite = this.characterDefeatPortraits[payload.PlayerId];
			}
			else
			{
				this.portrait.sprite = this.characterPortraits[payload.PlayerId];
				this.shadow.sprite = this.characterPortraits[payload.PlayerId];
			}
			this.scoreDetailContent.DestroyChildren();
			this._scoreWidgets.Clear();
			foreach (ScoreData scoreData in payload.ScoreDatas)
			{
				GameResultPanel.StringTableEntry stringTableEntry;
				if (!this._stringTable.TryGetValue(scoreData.Id, ref stringTableEntry))
				{
					stringTableEntry = new GameResultPanel.StringTableEntry
					{
						Name = "<" + scoreData.Id + ".Name>",
						Description = "<" + scoreData.Id + ".Description>"
					};
				}
				object[] nameArguments = scoreData.NameArguments;
				string text = ((nameArguments != null) ? string.Format(stringTableEntry.Name, nameArguments) : stringTableEntry.Name);
				object[] descriptionArguments = scoreData.DescriptionArguments;
				string text2 = ((descriptionArguments != null) ? string.Format(stringTableEntry.Description, descriptionArguments) : stringTableEntry.Description);
				ScoreWidget scoreWidget = Object.Instantiate<ScoreWidget>(this.scoreTemplate, this.scoreDetailContent);
				scoreWidget.SetContent(text, scoreData.Num, scoreData.TotalBluePoint);
				SimpleTooltipSource.CreateDirect(scoreWidget.gameObject, text, text2);
				this._scoreWidgets.Add(scoreWidget);
			}
			if (payload.DebugExp > 0)
			{
				ScoreWidget scoreWidget2 = Object.Instantiate<ScoreWidget>(this.scoreTemplate, this.scoreDetailContent);
				scoreWidget2.SetContent("Debug", 0, payload.DebugExp);
				this._scoreWidgets.Add(scoreWidget2);
			}
			if (payload.DifficultyMultipler != 0f)
			{
				this._deltaExp = (int)((float)payload.BluePoint / payload.DifficultyMultipler);
			}
			else
			{
				this._deltaExp = 0;
			}
			this.totalScoreText.text = "0";
			this.deltaScoreText.text = string.Format("+{0}", this._deltaExp);
			this._expMultiplier = payload.DifficultyMultipler;
			this._curLevel = ExpHelper.GetLevelForTotalExp(payload.PreviousTotalExp);
			if (this._curLevel > 0)
			{
				this._curExp = payload.PreviousTotalExp - ExpHelper.GetMaxExpForLevel(this._curLevel - 1);
			}
			else
			{
				this._curExp = payload.PreviousTotalExp;
			}
			this.RefreshExp();
			for (int j = 0; j < ExpHelper.MaxLevel; j++)
			{
				CardPackWidget pack2 = Object.Instantiate<CardPackWidget>(this.cardPackTemplate, this.cardPackRoot);
				RectTransform component = pack2.GetComponent<RectTransform>();
				int num = j / 2;
				int num2 = j % 2;
				float num3 = this.cardPackStartPos.x + (float)num * this.cardPackSpacing.x;
				float num4 = this.cardPackStartPos.y - (float)num2 * this.cardPackSpacing.y;
				if (num2 == 1)
				{
					num3 += 190f;
				}
				component.anchoredPosition = new Vector2(num3, num4);
				this._cardPackWidgets.Add(pack2);
				if (j < this._curLevel)
				{
					pack2.Unlock(true);
				}
				int ci = j + 1;
				pack2.Button.onClick.AddListener(delegate
				{
					if (pack2.IsLock)
					{
						return;
					}
					this.ShowLevelUpPanel(ci, true);
				});
			}
			foreach (CardPackWidget cardPackWidget in Enumerable.ToList<CardPackWidget>(Enumerable.Where<CardPackWidget>(this._cardPackWidgets, (CardPackWidget pack, int i) => i % 2 == 1)))
			{
				cardPackWidget.transform.SetAsLastSibling();
			}
			foreach (ParticleSystem particleSystem in this.levelUpParticles)
			{
				particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		protected override void OnShown()
		{
			base.StartCoroutine(this.Runner());
		}
		protected override void OnHiding()
		{
			if (GameMaster.Status == GameMaster.GameMasterStatus.MainMenu)
			{
				AudioManager.EnterLayer0();
			}
		}
		private void Win(GameResultType resultType)
		{
			AudioManager.Victory(resultType == GameResultType.TrueEnd);
			this.resultTitle.text = this._winString;
			TextMeshProUGUI textMeshProUGUI = this.resultDescription;
			string text;
			switch (resultType)
			{
			case GameResultType.NormalEnd:
				text = this._normalString;
				break;
			case GameResultType.TrueEndFail:
				text = this._trueEndFailString;
				break;
			case GameResultType.TrueEnd:
				text = this._trueEndString;
				break;
			default:
				throw new ArgumentOutOfRangeException("resultType", resultType, null);
			}
			textMeshProUGUI.text = text;
		}
		private void Lose()
		{
			AudioManager.Fail(GameMaster.Status == GameMaster.GameMasterStatus.MainMenu);
			this.resultTitle.text = this._loseString;
			this.resultDescription.text = this._badString;
			Image[] componentsInChildren = this.grayGroup.GetComponentsInChildren<Image>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = this.grayMaterial;
			}
			RawImage[] componentsInChildren2 = this.grayGroup.GetComponentsInChildren<RawImage>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].material = this.grayMaterial;
			}
		}
		private IEnumerator Runner()
		{
			GameResultPanel.<>c__DisplayClass77_0 CS$<>8__locals1 = new GameResultPanel.<>c__DisplayClass77_0();
			CS$<>8__locals1.<>4__this = this;
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.ResultVisible = true;
				CS$<>8__locals1.<>4__this.skipButton.gameObject.SetActive(true);
			});
			sequence.Append(this.resultTitle.DOFade(1f, 0.5f).From(0f, true, false));
			sequence.AppendInterval(0.5f);
			sequence.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.DesVisible = true;
			});
			sequence.Append(this.resultDescription.DOFade(1f, 0.5f).From(0f, true, false));
			sequence.AppendInterval(1f);
			sequence.Append(this.resultDescription.DOFade(0f, 0.5f).From(1f, true, false));
			sequence.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.skipButton.gameObject.SetActive(false);
			});
			sequence.SetUpdate(true);
			this._curRunner = sequence;
			yield return sequence.WaitForCompletion();
			Sequence sequence2 = DOTween.Sequence();
			sequence2.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.scoreGroup.gameObject.SetActive(true);
				CS$<>8__locals1.<>4__this.skipButton.gameObject.SetActive(true);
			});
			sequence2.Append(this.scoreGroup.DOFade(1f, 0.5f).From(0f, true, false));
			foreach (ScoreWidget scoreWidget in this._scoreWidgets)
			{
				sequence2.Append(scoreWidget.transform.DOScaleX(1f, 0.4f).From(2f, true, false));
				sequence2.Join(scoreWidget.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false));
				sequence2.Join(this.detailRoot.GetComponent<ScrollRect>().DONormalizedPos(Vector2.zero, 0.2f, false));
			}
			sequence2.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.deltaScoreText.gameObject.SetActive(true);
			});
			sequence2.Append(this.deltaScoreText.DOFade(1f, 0.5f).From(0f, true, false));
			CS$<>8__locals1.lerp1 = this._deltaExp;
			sequence2.Append(DOTween.To(() => CS$<>8__locals1.lerp1, delegate(int value)
			{
				CS$<>8__locals1.lerp1 = value;
				CS$<>8__locals1.<>4__this.deltaScoreText.text = string.Format("+{0}", CS$<>8__locals1.lerp1);
			}, 0, 1f));
			CS$<>8__locals1.lerp2 = 0;
			sequence2.Join(DOTween.To(() => CS$<>8__locals1.lerp2, delegate(int value)
			{
				CS$<>8__locals1.lerp2 = value;
				CS$<>8__locals1.<>4__this.totalScoreText.text = CS$<>8__locals1.lerp2.ToString();
			}, this._deltaExp, 1f));
			sequence2.Append(this.deltaScoreText.DOFade(0f, 0.5f).From(1f, true, false));
			CS$<>8__locals1.lerp4 = this._deltaExp;
			sequence2.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.deltaScoreText.text = string.Format("X{0:0.00} ", CS$<>8__locals1.<>4__this._expMultiplier) + "GameResult.DifficultyFactor".Localize(true);
			});
			sequence2.Append(this.deltaScoreText.DOFade(1f, 0.5f).From(0f, true, false));
			this._deltaExp = (int)((float)this._deltaExp * this._expMultiplier);
			sequence2.Append(DOTween.To(() => CS$<>8__locals1.lerp4, delegate(int value)
			{
				CS$<>8__locals1.lerp4 = value;
				CS$<>8__locals1.<>4__this.totalScoreText.text = CS$<>8__locals1.lerp4.ToString();
			}, this._deltaExp, 1f));
			sequence2.AppendInterval(1f);
			sequence2.Append(this.detailRoot.DOFade(0f, 0.5f).From(1f, true, false));
			sequence2.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.expRoot.gameObject.SetActive(true);
				CS$<>8__locals1.<>4__this.detailRoot.gameObject.SetActive(false);
			});
			sequence2.Append(this.expRoot.DOFade(1f, 0.5f).From(0f, true, false));
			sequence2.AppendCallback(delegate
			{
				CS$<>8__locals1.<>4__this.skipButton.gameObject.SetActive(false);
			});
			sequence2.SetUpdate(true);
			this._curRunner = sequence2;
			yield return sequence2.WaitForCompletion();
			for (;;)
			{
				GameResultPanel.<>c__DisplayClass77_1 CS$<>8__locals2 = new GameResultPanel.<>c__DisplayClass77_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				if (this._deltaExp == 0)
				{
					goto IL_07F0;
				}
				if (this._curLevel >= ExpHelper.MaxLevel)
				{
					break;
				}
				CS$<>8__locals2.isLevelUp = false;
				Sequence sequence3 = DOTween.Sequence();
				Sequence sequence4 = sequence3;
				TweenCallback tweenCallback;
				if ((tweenCallback = CS$<>8__locals2.CS$<>8__locals1.<>9__14) == null)
				{
					tweenCallback = (CS$<>8__locals2.CS$<>8__locals1.<>9__14 = delegate
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.skipButton.gameObject.SetActive(true);
					});
				}
				sequence4.AppendCallback(tweenCallback);
				if (this._curExp + this._deltaExp >= this._curMaxExp)
				{
					this._deltaExp -= this._curMaxExp - this._curExp;
					sequence3.Append(this.expSlider.DOFillAmount(1f, 1f));
					int lerp4 = this._curExp;
					sequence3.Join(DOTween.To(() => lerp4, delegate(int value)
					{
						lerp4 = value;
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.expText.text = string.Format("{0}/{1}", lerp4, CS$<>8__locals2.CS$<>8__locals1.<>4__this._curMaxExp);
					}, this._curMaxExp, 1f));
					Sequence sequence5 = sequence3;
					TweenCallback tweenCallback2;
					if ((tweenCallback2 = CS$<>8__locals2.CS$<>8__locals1.<>9__19) == null)
					{
						tweenCallback2 = (CS$<>8__locals2.CS$<>8__locals1.<>9__19 = delegate
						{
							CS$<>8__locals2.CS$<>8__locals1.<>4__this.expParticle.Play();
							CS$<>8__locals2.CS$<>8__locals1.<>4__this._cardPackWidgets[CS$<>8__locals2.CS$<>8__locals1.<>4__this._curLevel].Unlock(false);
						});
					}
					sequence5.AppendCallback(tweenCallback2);
					sequence3.AppendInterval(1f);
					sequence3.AppendCallback(delegate
					{
						CS$<>8__locals2.isLevelUp = true;
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.ShowLevelUpPanel(CS$<>8__locals2.CS$<>8__locals1.<>4__this._curLevel + 1, false);
					});
				}
				else
				{
					sequence3.Append(this.expSlider.DOFillAmount((float)(this._curExp + this._deltaExp) / (float)this._curMaxExp, 1f));
					int lerp3 = this._curExp;
					sequence3.Join(DOTween.To(() => lerp3, delegate(int value)
					{
						lerp3 = value;
						CS$<>8__locals2.CS$<>8__locals1.<>4__this._curExp = value;
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.expText.text = string.Format("{0}/{1}", lerp3, CS$<>8__locals2.CS$<>8__locals1.<>4__this._curMaxExp);
					}, this._curExp + this._deltaExp, 1f));
					this._deltaExp = 0;
				}
				Sequence sequence6 = sequence3;
				TweenCallback tweenCallback3;
				if ((tweenCallback3 = CS$<>8__locals2.CS$<>8__locals1.<>9__15) == null)
				{
					tweenCallback3 = (CS$<>8__locals2.CS$<>8__locals1.<>9__15 = delegate
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.skipButton.gameObject.SetActive(false);
					});
				}
				sequence6.AppendCallback(tweenCallback3);
				sequence3.SetUpdate(true);
				this._curRunner = sequence3;
				yield return sequence3.WaitForCompletion();
				if (CS$<>8__locals2.isLevelUp)
				{
					Func<bool> func;
					if ((func = CS$<>8__locals2.CS$<>8__locals1.<>9__16) == null)
					{
						func = (CS$<>8__locals2.CS$<>8__locals1.<>9__16 = () => !CS$<>8__locals2.CS$<>8__locals1.<>4__this.levelUpRoot.gameObject.activeSelf);
					}
					yield return new WaitUntil(func);
					this._curLevel++;
					this._curExp = 0;
					this.RefreshExp();
				}
				CS$<>8__locals2 = null;
				if (this._deltaExp <= 0)
				{
					goto IL_07F0;
				}
			}
			this.expText.text = "MAX/MAX";
			this.expSlider.fillAmount = 1f;
			IL_07F0:
			this.ReturnVisible = true;
			this.returnButton.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).From(0f, true, false);
			this.tabToggleRoot.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).From(0f, true, false);
			yield break;
		}
		private void ShowLevelUpPanel(int level, bool instant = false)
		{
			List<Card> list = new List<Card>();
			ExpConfig expConfig = Enumerable.First<ExpConfig>(ExpConfig.AllConfig(), (ExpConfig config) => config.Level == level);
			foreach (string text in expConfig.UnlockCards)
			{
				Card card = Library.CreateCard(text);
				list.Add(card);
			}
			Exhibit exhibit = null;
			if (expConfig.UnlockExhibits.Count > 0)
			{
				exhibit = Library.CreateExhibit(expConfig.UnlockExhibits[0]);
			}
			for (int i = 0; i < 3; i++)
			{
				if (list.Count > i)
				{
					this.levelUpCards[i].Card = list[i];
					GameMaster.RevealCard(list[i].Id);
				}
				else
				{
					this.levelUpCards[i].Card = Library.CreateCard(this.fakeCardId);
				}
			}
			foreach (Image image in this.levelUpCardMasks)
			{
				image.gameObject.SetActive(false);
			}
			this.levelUpExhibit.gameObject.SetActive(exhibit != null);
			this.levelUpExhibitMask.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 360f);
			if (instant)
			{
				this.levelUpExhibit.Exhibit = exhibit;
				this.levelUpExhibitMask.gameObject.SetActive(false);
				this.exhibitImage.color = Color.white;
				this.sideTip.SetActive(true);
				this.levelUpExhibit.OpenTooltip();
			}
			else
			{
				this.levelUpBgButton.interactable = false;
				this.sideTip.SetActive(false);
				this.levelUpExhibit.IsLock = true;
				this.levelUpExhibit.Exhibit = exhibit;
				for (int j = 0; j < 3; j++)
				{
					this.levelUpCards[j].gameObject.SetActive(false);
					this.levelUpCardMasks[j].gameObject.SetActive(true);
				}
				Sequence sequence = DOTween.Sequence();
				sequence.Join(this.levelUpRoot.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false));
				sequence.AppendCallback(delegate
				{
					for (int k = 0; k < 3; k++)
					{
						this.levelUpCards[k].gameObject.SetActive(true);
					}
				});
				sequence.Append(this.levelUpExhibitMask.DOFade(0f, 0.8f).From(1f, true, false));
				sequence.Join(this.levelUpExhibitMask.GetComponent<RectTransform>().DOAnchorPosY(200f, 0.8f, false).SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
				sequence.Join(this.exhibitImage.DOColor(Color.white, 0.2f).From(Color.black, true, false));
				foreach (ValueTuple<int, Image> valueTuple in this.levelUpCardMasks.WithIndices<Image>())
				{
					int item = valueTuple.Item1;
					Image item2 = valueTuple.Item2;
					int index = item;
					sequence.Join(item2.DOFade(0f, 0.2f).From(1f, true, false).SetDelay(0.2f * (float)index)
						.OnPlay(delegate
						{
							this.levelUpParticles[index].Play();
							AudioManager.PlayUi("UpgradeCardTemp", false);
						}));
				}
				sequence.SetLink(base.gameObject).OnComplete(delegate
				{
					foreach (Image image2 in this.levelUpCardMasks)
					{
						image2.gameObject.SetActive(false);
					}
					this.levelUpExhibitMask.gameObject.SetActive(false);
					this.levelUpBgButton.interactable = true;
					this.levelUpExhibit.OpenTooltip();
				});
				sequence.SetDelay(1f).SetUpdate(true);
			}
			GameMaster.RevealExhibit((exhibit != null) ? exhibit.Id : null);
			this.levelUpRoot.gameObject.SetActive(true);
			foreach (ParticleSystem particleSystem in this.levelUpParticles)
			{
				particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		private bool _resultVisible;
		private bool _desVisible;
		private bool _returnVisible;
		[Header("总体")]
		[FormerlySerializedAs("resultText")]
		[SerializeField]
		private TextMeshProUGUI resultTitle;
		[FormerlySerializedAs("descriptionText")]
		[SerializeField]
		private TextMeshProUGUI resultDescription;
		[SerializeField]
		private Button returnButton;
		[SerializeField]
		private Image portrait;
		[SerializeField]
		private Image shadow;
		[Header("计分详情组件")]
		[SerializeField]
		private CanvasGroup scoreGroup;
		[SerializeField]
		private CanvasGroup detailRoot;
		[FormerlySerializedAs("bgButton")]
		[SerializeField]
		private Button skipButton;
		[FormerlySerializedAs("scoreContent")]
		[SerializeField]
		private Transform scoreDetailContent;
		[SerializeField]
		private ScoreWidget scoreTemplate;
		[FormerlySerializedAs("totalScore")]
		[SerializeField]
		private TextMeshProUGUI totalScoreText;
		[SerializeField]
		private TextMeshProUGUI deltaScoreText;
		[Header("经验面板组件")]
		[SerializeField]
		private CanvasGroup expRoot;
		[SerializeField]
		private Image expSlider;
		[SerializeField]
		private ParticleSystem expParticle;
		[SerializeField]
		private TextMeshProUGUI expText;
		[SerializeField]
		private CardPackWidget cardPackTemplate;
		[SerializeField]
		private Transform cardPackRoot;
		[SerializeField]
		private Vector2 cardPackStartPos;
		[SerializeField]
		private Vector2 cardPackSpacing;
		[Header("升级面板组件")]
		[SerializeField]
		private Transform levelUpRoot;
		[SerializeField]
		private Button levelUpBgButton;
		[SerializeField]
		private List<CardWidget> levelUpCards;
		[SerializeField]
		private List<ParticleSystem> levelUpParticles;
		[SerializeField]
		private MuseumExhibitWidget levelUpExhibit;
		[SerializeField]
		private List<Image> levelUpCardMasks;
		[SerializeField]
		private Image levelUpExhibitMask;
		[SerializeField]
		private Image exhibitImage;
		[SerializeField]
		private GameObject sideTip;
		[SerializeField]
		private string fakeCardId;
		[Header("切换面板组件")]
		[SerializeField]
		private Transform tabToggleRoot;
		[SerializeField]
		private Button tabDetail;
		[SerializeField]
		private Button tabFold;
		[Header("杂项")]
		[SerializeField]
		private AssociationList<string, Sprite> characterPortraits;
		[SerializeField]
		private AssociationList<string, Sprite> characterDefeatPortraits;
		[SerializeField]
		private Material grayMaterial;
		[SerializeField]
		private Transform grayGroup;
		private const float Delay = 1f;
		private string _winString;
		private string _loseString;
		private string _normalString;
		private string _trueEndFailString;
		private string _trueEndString;
		private string _badString;
		private string _returnString;
		private List<CardPackWidget> _cardPackWidgets = new List<CardPackWidget>();
		private List<ScoreWidget> _scoreWidgets = new List<ScoreWidget>();
		private Dictionary<string, GameResultPanel.StringTableEntry> _stringTable;
		private Sequence _curRunner;
		private int _deltaExp;
		private int _curExp;
		private int _curMaxExp;
		private int _curLevel;
		private float _expMultiplier;
		private struct StringTableEntry
		{
			public string Name;
			public string Description;
		}
	}
}
