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
	// Token: 0x02000098 RID: 152
	public class GameResultPanel : UiPanel<GameResultData>
	{
		// Token: 0x17000150 RID: 336
		// (get) Token: 0x060007D9 RID: 2009 RVA: 0x000246C6 File Offset: 0x000228C6
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x060007DA RID: 2010 RVA: 0x000246C9 File Offset: 0x000228C9
		// (set) Token: 0x060007DB RID: 2011 RVA: 0x000246D1 File Offset: 0x000228D1
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

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x060007DC RID: 2012 RVA: 0x000246EB File Offset: 0x000228EB
		// (set) Token: 0x060007DD RID: 2013 RVA: 0x000246F3 File Offset: 0x000228F3
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

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x060007DE RID: 2014 RVA: 0x0002470D File Offset: 0x0002290D
		// (set) Token: 0x060007DF RID: 2015 RVA: 0x00024715 File Offset: 0x00022915
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

		// Token: 0x060007E0 RID: 2016 RVA: 0x00024740 File Offset: 0x00022940
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

		// Token: 0x060007E1 RID: 2017 RVA: 0x00024784 File Offset: 0x00022984
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

		// Token: 0x060007E2 RID: 2018 RVA: 0x00024898 File Offset: 0x00022A98
		private void RefreshExp()
		{
			this._curMaxExp = ExpHelper.GetExpForLevel(this._curLevel);
			this.expText.text = string.Format("{0}/{1}", this._curExp, this._curMaxExp);
			this.expSlider.fillAmount = (float)this._curExp / (float)this._curMaxExp;
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x000248FC File Offset: 0x00022AFC
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

		// Token: 0x060007E4 RID: 2020 RVA: 0x00024E04 File Offset: 0x00023004
		protected override void OnShown()
		{
			base.StartCoroutine(this.Runner());
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x00024E13 File Offset: 0x00023013
		protected override void OnHiding()
		{
			if (GameMaster.Status == GameMaster.GameMasterStatus.MainMenu)
			{
				AudioManager.EnterLayer0();
			}
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x00024E24 File Offset: 0x00023024
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

		// Token: 0x060007E7 RID: 2023 RVA: 0x00024E9C File Offset: 0x0002309C
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

		// Token: 0x060007E8 RID: 2024 RVA: 0x00024F28 File Offset: 0x00023128
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

		// Token: 0x060007E9 RID: 2025 RVA: 0x00024F38 File Offset: 0x00023138
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

		// Token: 0x04000552 RID: 1362
		private bool _resultVisible;

		// Token: 0x04000553 RID: 1363
		private bool _desVisible;

		// Token: 0x04000554 RID: 1364
		private bool _returnVisible;

		// Token: 0x04000555 RID: 1365
		[Header("总体")]
		[FormerlySerializedAs("resultText")]
		[SerializeField]
		private TextMeshProUGUI resultTitle;

		// Token: 0x04000556 RID: 1366
		[FormerlySerializedAs("descriptionText")]
		[SerializeField]
		private TextMeshProUGUI resultDescription;

		// Token: 0x04000557 RID: 1367
		[SerializeField]
		private Button returnButton;

		// Token: 0x04000558 RID: 1368
		[SerializeField]
		private Image portrait;

		// Token: 0x04000559 RID: 1369
		[SerializeField]
		private Image shadow;

		// Token: 0x0400055A RID: 1370
		[Header("计分详情组件")]
		[SerializeField]
		private CanvasGroup scoreGroup;

		// Token: 0x0400055B RID: 1371
		[SerializeField]
		private CanvasGroup detailRoot;

		// Token: 0x0400055C RID: 1372
		[FormerlySerializedAs("bgButton")]
		[SerializeField]
		private Button skipButton;

		// Token: 0x0400055D RID: 1373
		[FormerlySerializedAs("scoreContent")]
		[SerializeField]
		private Transform scoreDetailContent;

		// Token: 0x0400055E RID: 1374
		[SerializeField]
		private ScoreWidget scoreTemplate;

		// Token: 0x0400055F RID: 1375
		[FormerlySerializedAs("totalScore")]
		[SerializeField]
		private TextMeshProUGUI totalScoreText;

		// Token: 0x04000560 RID: 1376
		[SerializeField]
		private TextMeshProUGUI deltaScoreText;

		// Token: 0x04000561 RID: 1377
		[Header("经验面板组件")]
		[SerializeField]
		private CanvasGroup expRoot;

		// Token: 0x04000562 RID: 1378
		[SerializeField]
		private Image expSlider;

		// Token: 0x04000563 RID: 1379
		[SerializeField]
		private ParticleSystem expParticle;

		// Token: 0x04000564 RID: 1380
		[SerializeField]
		private TextMeshProUGUI expText;

		// Token: 0x04000565 RID: 1381
		[SerializeField]
		private CardPackWidget cardPackTemplate;

		// Token: 0x04000566 RID: 1382
		[SerializeField]
		private Transform cardPackRoot;

		// Token: 0x04000567 RID: 1383
		[SerializeField]
		private Vector2 cardPackStartPos;

		// Token: 0x04000568 RID: 1384
		[SerializeField]
		private Vector2 cardPackSpacing;

		// Token: 0x04000569 RID: 1385
		[Header("升级面板组件")]
		[SerializeField]
		private Transform levelUpRoot;

		// Token: 0x0400056A RID: 1386
		[SerializeField]
		private Button levelUpBgButton;

		// Token: 0x0400056B RID: 1387
		[SerializeField]
		private List<CardWidget> levelUpCards;

		// Token: 0x0400056C RID: 1388
		[SerializeField]
		private List<ParticleSystem> levelUpParticles;

		// Token: 0x0400056D RID: 1389
		[SerializeField]
		private MuseumExhibitWidget levelUpExhibit;

		// Token: 0x0400056E RID: 1390
		[SerializeField]
		private List<Image> levelUpCardMasks;

		// Token: 0x0400056F RID: 1391
		[SerializeField]
		private Image levelUpExhibitMask;

		// Token: 0x04000570 RID: 1392
		[SerializeField]
		private Image exhibitImage;

		// Token: 0x04000571 RID: 1393
		[SerializeField]
		private GameObject sideTip;

		// Token: 0x04000572 RID: 1394
		[SerializeField]
		private string fakeCardId;

		// Token: 0x04000573 RID: 1395
		[Header("切换面板组件")]
		[SerializeField]
		private Transform tabToggleRoot;

		// Token: 0x04000574 RID: 1396
		[SerializeField]
		private Button tabDetail;

		// Token: 0x04000575 RID: 1397
		[SerializeField]
		private Button tabFold;

		// Token: 0x04000576 RID: 1398
		[Header("杂项")]
		[SerializeField]
		private AssociationList<string, Sprite> characterPortraits;

		// Token: 0x04000577 RID: 1399
		[SerializeField]
		private AssociationList<string, Sprite> characterDefeatPortraits;

		// Token: 0x04000578 RID: 1400
		[SerializeField]
		private Material grayMaterial;

		// Token: 0x04000579 RID: 1401
		[SerializeField]
		private Transform grayGroup;

		// Token: 0x0400057A RID: 1402
		private const float Delay = 1f;

		// Token: 0x0400057B RID: 1403
		private string _winString;

		// Token: 0x0400057C RID: 1404
		private string _loseString;

		// Token: 0x0400057D RID: 1405
		private string _normalString;

		// Token: 0x0400057E RID: 1406
		private string _trueEndFailString;

		// Token: 0x0400057F RID: 1407
		private string _trueEndString;

		// Token: 0x04000580 RID: 1408
		private string _badString;

		// Token: 0x04000581 RID: 1409
		private string _returnString;

		// Token: 0x04000582 RID: 1410
		private List<CardPackWidget> _cardPackWidgets = new List<CardPackWidget>();

		// Token: 0x04000583 RID: 1411
		private List<ScoreWidget> _scoreWidgets = new List<ScoreWidget>();

		// Token: 0x04000584 RID: 1412
		private Dictionary<string, GameResultPanel.StringTableEntry> _stringTable;

		// Token: 0x04000585 RID: 1413
		private Sequence _curRunner;

		// Token: 0x04000586 RID: 1414
		private int _deltaExp;

		// Token: 0x04000587 RID: 1415
		private int _curExp;

		// Token: 0x04000588 RID: 1416
		private int _curMaxExp;

		// Token: 0x04000589 RID: 1417
		private int _curLevel;

		// Token: 0x0400058A RID: 1418
		private float _expMultiplier;

		// Token: 0x0200024E RID: 590
		private struct StringTableEntry
		{
			// Token: 0x0400108A RID: 4234
			public string Name;

			// Token: 0x0400108B RID: 4235
			public string Description;
		}
	}
}
