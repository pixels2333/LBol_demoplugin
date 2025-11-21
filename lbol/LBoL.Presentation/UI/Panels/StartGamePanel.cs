using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.Helpers;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Yarn;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000BC RID: 188
	public class StartGamePanel : UiPanel<StartGameData>, IInputActionHandler
	{
		// Token: 0x06000AB3 RID: 2739 RVA: 0x00035780 File Offset: 0x00033980
		private void Awake()
		{
			this._modeTooltip = SimpleTooltipSource.CreateWithGeneralKey(this.gameModeSwitch.gameObject, "StartGame.FreeMode", "StartGame.FreeDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			this._randomTooltip = SimpleTooltipSource.CreateWithGeneralKey(this.randomResultSwitch.gameObject, "StartGame.NotShowRandomResultTitle", "StartGame.NotShowRandomResultDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			this._confirmTooltip = SimpleTooltipSource.CreateWithGeneralKey(this.difficultyConfirmButton.gameObject, "Explain.Title", "StartGame.NoStoryMode").WithPosition(TooltipDirection.Top, TooltipAlignment.Center);
			this._puzzleTooltip = SimpleTooltipSource.CreateWithGeneralKey(this.puzzleHint, "StartGame.Puzzle", "StartGame.PuzzleHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this._jadeBoxTooltip = SimpleTooltipSource.CreateWithGeneralKey(this.jadeBoxButton.gameObject, "Explain.Title", "StartGame.JadeBoxLock").WithPosition(TooltipDirection.Top, TooltipAlignment.Center);
			this._players = Enumerable.ToList<PlayerUnit>(Enumerable.OrderBy<PlayerUnit, int>(LBoL.Core.Library.GetSelectablePlayers(), (PlayerUnit player) => player.Config.ShowOrder));
			this.jadeBoxTemplate.gameObject.SetActive(false);
			for (int i = 0; i < 2; i++)
			{
				int index = i;
				StartSetupWidget startSetupWidget = this.characterSetupList[i];
				startSetupWidget.GetComponent<Button>().onClick.AddListener(delegate
				{
					this.SelectType(index);
				});
				startSetupWidget.DeckButton.onClick.AddListener(delegate
				{
					this.deckHolder.Clear();
					int index2 = index;
					if (index2 != 0)
					{
						if (index2 == 1)
						{
							foreach (string text in this._player.Config.DeckB)
							{
								Card card = LBoL.Core.Library.CreateCard(text);
								this.deckHolder.AddCardWidget(card, true);
							}
							string text2 = this._player.ShortName + "Game.Deck".Localize(true) + "B";
							this.deckHolder.SetTitle(text2, "Cards.Show".Localize(true));
						}
					}
					else
					{
						foreach (string text3 in this._player.Config.DeckA)
						{
							Card card2 = LBoL.Core.Library.CreateCard(text3);
							this.deckHolder.AddCardWidget(card2, true);
						}
						string text4 = this._player.ShortName + "Game.Deck".Localize(true) + "A";
						this.deckHolder.SetTitle(text4, "Cards.Show".Localize(true));
					}
					CanvasGroup component = this.deckHolder.GetComponent<CanvasGroup>();
					component.DOKill(false);
					component.DOFade(1f, 0.4f).From(0f, true, false).SetUpdate(true);
					this.deckHolder.gameObject.SetActive(true);
					this.deckReturnButton.interactable = true;
				});
			}
			this.InitialForSeed();
			this.InitialForPuzzle();
			this.InitialForJadeBox();
			this.characterLeftButton.onClick.AddListener(delegate
			{
				this.SelectPlayer(this._playerIndex - 1, false);
			});
			this.characterRightButton.onClick.AddListener(delegate
			{
				this.SelectPlayer(this._playerIndex + 1, false);
			});
			this.difficultyLeftButton.onClick.AddListener(delegate
			{
				this.SelectDifficulty(this._difficultyIndex - 1, false);
			});
			this.difficultyRightButton.onClick.AddListener(delegate
			{
				this.SelectDifficulty(this._difficultyIndex + 1, false);
			});
			this.deckReturnButton.onClick.AddListener(delegate
			{
				this.deckReturnButton.interactable = false;
				CanvasGroup component2 = this.deckHolder.GetComponent<CanvasGroup>();
				component2.DOKill(false);
				TweenerCore<float, float, FloatOptions> tweenerCore = component2.DOFade(0f, 0.4f).SetUpdate(true).From(1f, true, false);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, delegate
				{
					this.deckHolder.gameObject.SetActive(false);
				});
			});
			this.characterConfirmButton.onClick.AddListener(new UnityAction(this.ConfirmSelectCharacter));
			this.returnButton.onClick.AddListener(delegate
			{
				switch (this._currentPanelPhase)
				{
				case 1:
					base.Hide();
					return;
				case 2:
					base.Hide();
					return;
				case 3:
				{
					this.characterPanelRoot.DOKill(false);
					this.difficultyPanelRoot.DOKill(false);
					this.characterPanelRoot.gameObject.SetActive(true);
					Sequence sequence = DOTween.Sequence();
					sequence.Join(this.characterPanelRoot.DOFade(1f, 0.2f).From(0f, true, false));
					sequence.Join(this.difficultyPanelRoot.DOFade(0f, 0.2f).From(1f, true, false));
					sequence.OnComplete(delegate
					{
						this.difficultyPanelRoot.gameObject.SetActive(false);
					}).SetUpdate(true).SetTarget(this.characterPanelRoot);
					this._currentPanelPhase = 2;
					return;
				}
				default:
					return;
				}
			});
			this.difficultyConfirmButton.onClick.AddListener(delegate
			{
				List<PuzzleFlag> list = new List<PuzzleFlag>();
				foreach (KeyValuePair<PuzzleFlag, PuzzleToggleWidget> keyValuePair in this._puzzleToggles)
				{
					PuzzleFlag puzzleFlag;
					PuzzleToggleWidget puzzleToggleWidget;
					keyValuePair.Deconstruct(ref puzzleFlag, ref puzzleToggleWidget);
					PuzzleFlag puzzleFlag2 = puzzleFlag;
					if (puzzleToggleWidget.Toggle.isOn)
					{
						list.Add(puzzleFlag2);
					}
				}
				PuzzleFlag puzzleFlag3 = PuzzleFlags.FromComponents(list);
				PlayerUnit playerUnit = LBoL.Core.Library.CreatePlayerUnit(this._player.GetType());
				playerUnit.SetUs(this._typeCandidate.Us);
				int selectedType = this._selectedType;
				PlayerType playerType;
				if (selectedType != 0)
				{
					if (selectedType != 1)
					{
						throw new ArgumentOutOfRangeException();
					}
					playerType = PlayerType.TypeB;
				}
				else
				{
					playerType = PlayerType.TypeA;
				}
				PlayerType playerType2 = playerType;
				IEnumerable<JadeBox> enumerable = Enumerable.Select<KeyValuePair<JadeBox, JadeBoxToggle>, JadeBox>(Enumerable.Where<KeyValuePair<JadeBox, JadeBoxToggle>>(this._jadeBoxToggles, (KeyValuePair<JadeBox, JadeBoxToggle> pair) => pair.Value.IsOn), (KeyValuePair<JadeBox, JadeBoxToggle> pair) => pair.Key);
				if (this._seedString == null)
				{
					GameMaster.StartGame(this._selectedDifficulty, puzzleFlag3, playerUnit, playerType2, this._typeCandidate.Exhibit, default(int?), this._typeCandidate.Deck, this._stages, this._debutAdventure, enumerable, this.gameModeSwitch.IsOn ? GameMode.StoryMode : GameMode.FreeMode, this.randomResultSwitch.IsOn);
					return;
				}
				ulong num;
				if (RandomGen.TryParseSeed(this._seedString, out num))
				{
					GameMaster.StartGame(new ulong?(num), this._selectedDifficulty, puzzleFlag3, playerUnit, playerType2, this._typeCandidate.Exhibit, default(int?), this._typeCandidate.Deck, this._stages, this._debutAdventure, enumerable, this.gameModeSwitch.IsOn ? GameMode.StoryMode : GameMode.FreeMode, this.randomResultSwitch.IsOn);
				}
			});
			this.gameModeSwitch.AddListener(delegate(bool flag)
			{
				if (flag)
				{
					this._modeTooltip.SetWithGeneralKey("StartGame.StoryMode", "StartGame.StoryDescription");
				}
				else
				{
					this._modeTooltip.SetWithGeneralKey("StartGame.FreeMode", "StartGame.FreeDescription");
				}
				this.RefreshDifficultyConfirm();
			});
			this.randomResultSwitch.AddListener(delegate(bool flag)
			{
				GameMaster.DefaultShowRandomResult = flag;
				if (flag)
				{
					this._randomTooltip.SetWithGeneralKey("StartGame.ShowRandomResultTitle", "StartGame.ShowRandomResultDescription");
					return;
				}
				this._randomTooltip.SetWithGeneralKey("StartGame.NotShowRandomResultTitle", "StartGame.NotShowRandomResultDescription");
			});
			this.seedButton.onClick.AddListener(new UnityAction(this.OnSeedButtonClicked));
			this.seedConfirmButton.onClick.AddListener(new UnityAction(this.OnSeedConfirmButtonClicked));
			this.seedCancelButton.onClick.AddListener(new UnityAction(this.OnSeedCancelButtonClicked));
			this.seedSetImage.fillAmount = 0f;
			this.jadeBoxButton.onClick.AddListener(new UnityAction(this.OnJadeBoxButtonClicked));
			this.jadeBoxReturnButton.onClick.AddListener(new UnityAction(this.OnJadeBoxReTurnButtonClicked));
			this.jadeBoxSetImage.fillAmount = 0f;
			this.SelectPlayer(-2, false);
			this.SelectDifficulty(1, true);
		}

		// Token: 0x06000AB4 RID: 2740 RVA: 0x00035AD0 File Offset: 0x00033CD0
		protected override void OnShowing(StartGameData data)
		{
			this._stages = data.StagesCreateFunc.Invoke();
			this._seedString = null;
			this._debutAdventure = data.DebutAdventure;
			this._currentPanelPhase = 2;
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.StartGame);
			GameObject gameObject = this.replayOpeningButton.gameObject;
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			gameObject.SetActive(currentProfile != null && currentProfile.OpeningPlayed);
			UiManager.PushActionHandler(this);
			foreach (KeyValuePair<PlayerUnitConfig, CharacterButtonWidget> keyValuePair in this._playerButtons)
			{
				PlayerUnitConfig playerUnitConfig;
				CharacterButtonWidget characterButtonWidget;
				keyValuePair.Deconstruct(ref playerUnitConfig, ref characterButtonWidget);
				PlayerUnitConfig playerUnitConfig2 = playerUnitConfig;
				CharacterButtonWidget characterButtonWidget2 = characterButtonWidget;
				characterButtonWidget2.Interactable = playerUnitConfig2.UnlockLevel != null;
			}
			this.SetPuzzleStatus();
			this.SetJadeBoxStatus();
			this.backgroundOuterRing.DOLocalRotate(new Vector3(0f, 0f, -360f), 20f, RotateMode.FastBeyond360).SetUpdate(true).SetLoops(-1, LoopType.Incremental)
				.SetEase(Ease.Linear)
				.SetLink(base.gameObject);
			this.difficultyPanelRotationBg.DOLocalRotate(new Vector3(0f, 0f, 360f), 200f, RotateMode.FastBeyond360).SetUpdate(true).SetLoops(-1, LoopType.Incremental)
				.SetEase(Ease.Linear)
				.SetLink(base.gameObject);
			this.difficultyPanelRoot.gameObject.SetActive(false);
			this.characterPanelRoot.gameObject.SetActive(true);
			this.characterPanelTopRoot.gameObject.SetActive(true);
			this.characterPanelBottomRoot.gameObject.SetActive(true);
			this.characterPanelRoot.alpha = 1f;
			this.characterPanelTopRoot.alpha = 1f;
			this.characterPanelBottomRoot.alpha = 1f;
			this.backgroundManaPanel.gameObject.SetActive(true);
			this.backgroundOuterRing.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
			this.backgroundManaPanel.alpha = 1f;
			this.backgroundManaGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			this.backgroundRotationRoot.localEulerAngles = Vector3.zero;
			this.backgroundManaPanel.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			this.SelectPlayer(0, true);
			this.randomResultSwitch.IsOn = GameMaster.DefaultShowRandomResult;
			this.gameModeSwitch.IsOn = false;
			GameDifficulty? gameDifficulty = default(GameDifficulty?);
			if (Singleton<GameMaster>.Instance.CurrentProfile != null)
			{
				gameDifficulty = Singleton<GameMaster>.Instance.CurrentProfile.HighestSuccessDifficulty;
			}
			this.difficultyHint.SetActive(gameDifficulty == null);
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x00035D9C File Offset: 0x00033F9C
		protected override void OnHiding()
		{
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Idle);
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x00035DB0 File Offset: 0x00033FB0
		public override void OnLocaleChanged()
		{
			base.OnLocaleChanged();
			for (int i = 0; i < 4; i++)
			{
				this.difficultyText[i].text = ("Tooltip.Difficulty" + StartGamePanel.Difficulties[i].ToString() + ".Description").Localize(true);
			}
			this.JadeBoxLocalize();
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x00035E14 File Offset: 0x00034014
		void IInputActionHandler.OnConfirm()
		{
			if (this.seedPanelRoot.gameObject.activeSelf)
			{
				this.seedConfirmButton.onClick.Invoke();
				return;
			}
			if (this._currentPanelPhase == 2 && this.characterConfirmButton.interactable)
			{
				this.characterConfirmButton.onClick.Invoke();
				return;
			}
			if (this._currentPanelPhase == 3 && this.difficultyConfirmButton.interactable)
			{
				this.difficultyConfirmButton.onClick.Invoke();
			}
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x00035E94 File Offset: 0x00034094
		void IInputActionHandler.OnCancel()
		{
			VnPanel panel = UiManager.GetPanel<VnPanel>();
			if (panel.IsRunning)
			{
				if (panel.CanSkipAll)
				{
					UiManager.GetDialog<MessageDialog>().Show(new MessageContent
					{
						TextKey = "SkipDialog",
						Buttons = DialogButtons.ConfirmCancel,
						OnConfirm = new Action(panel.SkipAll)
					});
				}
				return;
			}
			if (this.seedPanelRoot.gameObject.activeSelf)
			{
				this.seedCancelButton.onClick.Invoke();
				return;
			}
			if (this.deckHolder.gameObject.activeSelf)
			{
				this.deckReturnButton.onClick.Invoke();
				return;
			}
			if (this.returnButton.gameObject.activeSelf)
			{
				this.returnButton.onClick.Invoke();
			}
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x00035F54 File Offset: 0x00034154
		void IInputActionHandler.OnNavigate(NavigateDirection dir)
		{
			if (this._currentPanelPhase == 2)
			{
				if (dir == NavigateDirection.Left && this.characterLeftButton.interactable)
				{
					this.characterLeftButton.onClick.Invoke();
				}
				if (dir == NavigateDirection.Right && this.characterRightButton.interactable)
				{
					this.characterRightButton.onClick.Invoke();
				}
				return;
			}
			if (this._currentPanelPhase == 3)
			{
				if (dir == NavigateDirection.Left && this.difficultyLeftButton.interactable)
				{
					this.difficultyLeftButton.onClick.Invoke();
				}
				if (dir == NavigateDirection.Right && this.difficultyRightButton.interactable)
				{
					this.difficultyRightButton.onClick.Invoke();
				}
			}
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x00035FF6 File Offset: 0x000341F6
		public void BeginSkipDialog()
		{
			UiManager.GetPanel<VnPanel>().UserSkipDialog(true);
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x00036003 File Offset: 0x00034203
		public void EndSkipDialog()
		{
			UiManager.GetPanel<VnPanel>().UserSkipDialog(false);
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x00036010 File Offset: 0x00034210
		private void SelectPlayer(int index, bool instant = false)
		{
			if (this.gameModeSwitch.IsOn)
			{
				this.gameModeSwitch.IsOn = false;
			}
			int num = ((index > this._playerIndex) ? 1 : (-1));
			if (index == -1)
			{
				index = this._players.Count - 1;
				num = -1;
			}
			if (index == this._players.Count)
			{
				index = 0;
				num = 1;
			}
			if (index == -2)
			{
				this._playerIndex = index;
				this._player = null;
				this._typeCandidate = null;
				this._typeCandidates = null;
				this.SelectType(-1);
			}
			else
			{
				for (int i = 0; i < 5; i++)
				{
					int num2 = i - 2 + index;
					if (num2 < 0)
					{
						num2 += this._players.Count;
					}
					if (num2 >= this._players.Count)
					{
						num2 -= this._players.Count;
					}
					this.characterStandPicList[i].GetComponentInChildren<Image>().sprite = this.standPicList[this._players[num2].Id];
					if (!instant)
					{
						if ((i == 0 && num == -1) || (i == 4 && num == 1))
						{
							this.characterStandPicList[i].DOFade(this._standPicAlpha[i], 0.2f).From(0f, true, false).SetUpdate(true);
						}
						else
						{
							this.characterStandPicList[i].transform.DOScale(this._standPicScale[i], 0.2f).From(this._standPicScale[i + num], true, false).SetUpdate(true);
							this.characterStandPicList[i].GetComponent<RectTransform>().DOLocalMoveX(this._standPicX[i], 0.2f, false).From(this._standPicX[i + num], true, false)
								.SetUpdate(true);
							this.characterStandPicList[i].GetComponent<RectTransform>().DOLocalMoveY(this._standPicY[i], 0.2f, false).From(this._standPicY[i + num], true, false)
								.SetUpdate(true);
							this.characterStandPicList[i].DOFade(this._standPicAlpha[i], 0.2f).From(this._standPicAlpha[i + num], true, false).SetUpdate(true);
						}
					}
				}
				this._playerIndex = index;
				this._player = this._players[index];
				PlayerUnitConfig config = this._player.Config;
				if (instant)
				{
					if (config.BasicRingOrder != null)
					{
						int value = config.BasicRingOrder.Value;
						foreach (Transform transform in this.backgroundManaList.Values)
						{
							transform.transform.localEulerAngles = new Vector3(0f, 0f, (float)((value - 1) * -72));
						}
						this.backgroundRotationRoot.localEulerAngles = new Vector3(0f, 0f, (float)((value - 1) * 72));
						this.basicGroup.alpha = 1f;
						this.nonBasicGroup.alpha = 0f;
						this._showingBasicRing = true;
					}
					else
					{
						Sprite sprite;
						if (this.nonBasicManaSprites.TryGetValue(config.LeftColor, out sprite))
						{
							this.nonBasicLeftColor.sprite = sprite;
						}
						if (this.nonBasicManaSprites.TryGetValue(config.RightColor, out sprite))
						{
							this.nonBasicRightColor.sprite = sprite;
						}
						this.basicGroup.alpha = 0f;
						this.nonBasicGroup.alpha = 1f;
						this._showingBasicRing = false;
					}
				}
				else
				{
					Sequence nonBasicTween = this._nonBasicTween;
					if (nonBasicTween != null)
					{
						nonBasicTween.Complete(true);
					}
					if (config.BasicRingOrder != null)
					{
						if (!this._showingBasicRing)
						{
							this.basicGroup.alpha = 1f;
							this.nonBasicGroup.alpha = 0f;
							this._showingBasicRing = true;
						}
						int value2 = config.BasicRingOrder.Value;
						foreach (Transform transform2 in this.backgroundManaList.Values)
						{
							transform2.transform.localEulerAngles = new Vector3(0f, 0f, (float)((value2 - 1 - num) * -72));
						}
						this.backgroundRotationRoot.localEulerAngles = new Vector3(0f, 0f, (float)((value2 - 1 - num) * 72));
						foreach (Transform transform3 in this.backgroundManaList.Values)
						{
							transform3.transform.DOComplete(false);
							transform3.transform.DOLocalRotate(new Vector3(0f, 0f, (float)(num * -72)), 0.2f, RotateMode.Fast).SetUpdate(true).SetRelative<TweenerCore<Quaternion, Vector3, QuaternionOptions>>();
						}
						this.backgroundRotationRoot.DOComplete(false);
						this.backgroundRotationRoot.DOLocalRotate(new Vector3(0f, 0f, (float)(num * 72)), 0.2f, RotateMode.Fast).SetUpdate(true).SetRelative<TweenerCore<Quaternion, Vector3, QuaternionOptions>>();
					}
					else if (this._showingBasicRing)
					{
						this._nonBasicTween = DOTween.Sequence().Append(this.basicGroup.DOFade(0f, 0.2f)).Append(this.nonBasicGroup.DOFade(1f, 0.2f))
							.SetUpdate(true);
						Sprite sprite2;
						if (this.nonBasicManaSprites.TryGetValue(config.LeftColor, out sprite2))
						{
							this.nonBasicLeftColor.sprite = sprite2;
						}
						if (this.nonBasicManaSprites.TryGetValue(config.RightColor, out sprite2))
						{
							this.nonBasicRightColor.sprite = sprite2;
						}
						this._showingBasicRing = false;
					}
					else
					{
						this._nonBasicTween = DOTween.Sequence().Join(this.nonBasicLeftColor.DOFade(0f, 0.2f)).Join(this.nonBasicRightColor.DOFade(0f, 0.2f))
							.AppendCallback(delegate
							{
								Sprite sprite3;
								if (this.nonBasicManaSprites.TryGetValue(config.LeftColor, out sprite3))
								{
									this.nonBasicLeftColor.sprite = sprite3;
								}
								if (this.nonBasicManaSprites.TryGetValue(config.RightColor, out sprite3))
								{
									this.nonBasicRightColor.sprite = sprite3;
								}
							})
							.Append(this.nonBasicLeftColor.DOFade(1f, 0.2f))
							.Join(this.nonBasicRightColor.DOFade(1f, 0.2f))
							.SetUpdate(true);
					}
					this.characterSetupRoot.DOKill(false);
					this.characterSetupRoot.alpha = 0f;
					this.characterSetupRoot.DOFade(1f, 0.2f).SetDelay(0.4f).SetUpdate(true);
				}
				this.mainStandPic.sprite = this.standPicList[this._player.Id];
				this.mainStandPicShadow.sprite = this.standPicList[this._player.Id];
				this._typeCandidate = null;
				this._typeCandidates = new StartGamePanel.TypeCandidate[]
				{
					new StartGamePanel.TypeCandidate
					{
						Name = "TypeA",
						Us = LBoL.Core.Library.CreateUs(config.UltimateSkillA),
						Exhibit = LBoL.Core.Library.CreateExhibit(config.ExhibitA),
						Deck = Enumerable.ToArray<Card>(Enumerable.Select<string, Card>(config.DeckA, new Func<string, Card>(LBoL.Core.Library.CreateCard)))
					},
					new StartGamePanel.TypeCandidate
					{
						Name = "TypeB",
						Us = LBoL.Core.Library.CreateUs(config.UltimateSkillB),
						Exhibit = LBoL.Core.Library.CreateExhibit(config.ExhibitB),
						Deck = Enumerable.ToArray<Card>(Enumerable.Select<string, Card>(config.DeckB, new Func<string, Card>(LBoL.Core.Library.CreateCard)))
					}
				};
				foreach (ValueTuple<int, StartGamePanel.TypeCandidate> valueTuple in this._typeCandidates.WithIndices<StartGamePanel.TypeCandidate>())
				{
					int item = valueTuple.Item1;
					StartGamePanel.TypeCandidate item2 = valueTuple.Item2;
					this.characterSetupList[item].Set(item2.Us, item2.Exhibit);
				}
				this.characterStatusWidget.SetCharacter(this._player);
				this.SelectType(0);
			}
			this.RefreshConfirm();
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x000368B4 File Offset: 0x00034AB4
		private void SelectType(int index)
		{
			this._selectedType = index;
			foreach (StartSetupWidget startSetupWidget in this.characterSetupList)
			{
				startSetupWidget.SelectThisSkill(false);
			}
			if (index == -1)
			{
				this._typeCandidate = null;
				return;
			}
			this._typeCandidate = this._typeCandidates[index];
			this.characterSetupList[index].SelectThisSkill(true);
			ManaGroup initialMana = this._player.Config.InitialMana;
			ManaColor? baseManaColor = this.characterSetupList[index].Exhibit.Config.BaseManaColor;
			ManaGroup manaGroup;
			if (baseManaColor != null)
			{
				ManaColor valueOrDefault = baseManaColor.GetValueOrDefault();
				manaGroup = ManaGroup.Single(valueOrDefault);
			}
			else
			{
				manaGroup = ManaGroup.Empty;
			}
			ManaGroup manaGroup2 = manaGroup;
			this.characterStatusWidget.SetSetup(initialMana + manaGroup2, (index == 0) ? this._player.Config.DifficultyA : this._player.Config.DifficultyB);
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x000369C0 File Offset: 0x00034BC0
		private void SelectDifficulty(int index, bool immediate = false)
		{
			if (this._player != null)
			{
				ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
				GameDifficulty? highestDifficulty = currentProfile.GetHighestDifficulty(this._player.Id);
				GameDifficulty? highestPerfectDifficulty = currentProfile.GetHighestPerfectDifficulty(this._player.Id);
				int num = 2;
				GameDifficulty? gameDifficulty = currentProfile.HighestSuccessDifficulty;
				GameDifficulty gameDifficulty2 = GameDifficulty.Lunatic;
				if ((gameDifficulty.GetValueOrDefault() == gameDifficulty2) & (gameDifficulty != null))
				{
					num = 3;
				}
				DifficultyGroup difficultyGroup = this.difficultyGroups[index];
				if (index >= num)
				{
					bool flag = false;
					if (highestPerfectDifficulty != null)
					{
						gameDifficulty = highestPerfectDifficulty;
						gameDifficulty2 = StartGamePanel.Difficulties[index - 1];
						if ((gameDifficulty.GetValueOrDefault() >= gameDifficulty2) & (gameDifficulty != null))
						{
							flag = true;
							goto IL_00D5;
						}
					}
					if (highestDifficulty != null)
					{
						gameDifficulty = highestDifficulty;
						gameDifficulty2 = StartGamePanel.Difficulties[index - 1];
						if ((gameDifficulty.GetValueOrDefault() >= gameDifficulty2) & (gameDifficulty != null))
						{
							flag = true;
						}
					}
					IL_00D5:
					if (!flag)
					{
						difficultyGroup.SetLocked(true);
						this._isDifficultyLock = true;
					}
					else
					{
						this._isDifficultyLock = false;
						difficultyGroup.SetLocked(false);
					}
				}
				else
				{
					this._isDifficultyLock = false;
					difficultyGroup.SetLocked(false);
				}
				if (highestPerfectDifficulty != null)
				{
					gameDifficulty = highestPerfectDifficulty;
					gameDifficulty2 = StartGamePanel.Difficulties[index];
					if ((gameDifficulty.GetValueOrDefault() >= gameDifficulty2) & (gameDifficulty != null))
					{
						difficultyGroup.SetClearState(ClearState.PerfectCleared);
						goto IL_017A;
					}
				}
				if (highestDifficulty != null)
				{
					gameDifficulty = highestDifficulty;
					gameDifficulty2 = StartGamePanel.Difficulties[index];
					if ((gameDifficulty.GetValueOrDefault() >= gameDifficulty2) & (gameDifficulty != null))
					{
						difficultyGroup.SetClearState(ClearState.Cleared);
						goto IL_017A;
					}
				}
				difficultyGroup.SetClearState(ClearState.NotCleared);
			}
			IL_017A:
			this.difficultyLeftButton.interactable = index > 0;
			this.difficultyRightButton.interactable = index < StartGamePanel.Difficulties.Length - 1;
			if (immediate)
			{
				using (IEnumerator<ValueTuple<int, DifficultyGroup>> enumerator = this.difficultyGroups.WithIndices<DifficultyGroup>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ValueTuple<int, DifficultyGroup> valueTuple = enumerator.Current;
						int item = valueTuple.Item1;
						valueTuple.Item2.SetDifficultyActive(item == index);
					}
					goto IL_02C1;
				}
			}
			AudioManager.PlayUi("Slide", false);
			foreach (ValueTuple<int, DifficultyGroup> valueTuple2 in this.difficultyGroups.WithIndices<DifficultyGroup>())
			{
				int item2 = valueTuple2.Item1;
				valueTuple2.Item2.SetDifficultyActive(item2 == index || item2 == this._difficultyIndex);
			}
			if (index > this._difficultyIndex)
			{
				this.difficultyGroups[index].FadeIn(1000f, 0.3f);
				this.difficultyGroups[this._difficultyIndex].FadeOut(-1000f, 0.3f);
			}
			else
			{
				this.difficultyGroups[index].FadeIn(-1000f, 0.3f);
				this.difficultyGroups[this._difficultyIndex].FadeOut(1000f, 0.3f);
			}
			IL_02C1:
			this._difficultyIndex = index;
			this._selectedDifficulty = StartGamePanel.Difficulties[index];
			this.RefreshDifficultyConfirm();
		}

		// Token: 0x06000ABF RID: 2751 RVA: 0x00036CC4 File Offset: 0x00034EC4
		private void RefreshConfirm()
		{
			if (this._playerIndex < 0)
			{
				return;
			}
			int? unlockLevel = this._players[this._playerIndex].Config.UnlockLevel;
			bool flag = true;
			if (unlockLevel == null)
			{
				this.characterHintRoot.gameObject.SetActive(true);
				this.characterHint.text = "StartGame.NotReadyHint".Localize(true);
				flag = false;
			}
			else
			{
				int? num = unlockLevel;
				int currentProfileLevel = Singleton<GameMaster>.Instance.CurrentProfileLevel;
				if ((num.GetValueOrDefault() > currentProfileLevel) & (num != null))
				{
					this.characterHintRoot.gameObject.SetActive(true);
					this.characterHint.text = string.Format("StartGame.IsLockHint".Localize(true), unlockLevel);
					flag = false;
				}
				else
				{
					this.characterHintRoot.gameObject.SetActive(false);
				}
			}
			this.characterConfirmButton.interactable = flag;
		}

		// Token: 0x06000AC0 RID: 2752 RVA: 0x00036DA4 File Offset: 0x00034FA4
		private void RefreshDifficultyConfirm()
		{
			bool flag = !this.gameModeSwitch.IsOn && !this._isDifficultyLock;
			this._confirmTooltip.enabled = !flag;
			this.difficultyConfirmButton.interactable = flag;
			if (this.gameModeSwitch.IsOn)
			{
				this._confirmTooltip.SetWithGeneralKey("Explain.Title", "StartGame.NoStoryMode");
			}
			if (this._isDifficultyLock)
			{
				this._confirmTooltip.SetWithGeneralKey("StartGame.NeedClear", null);
			}
		}

		// Token: 0x06000AC1 RID: 2753 RVA: 0x00036E21 File Offset: 0x00035021
		public void UI_ReplayOpening()
		{
			VnPanel panel = UiManager.GetPanel<VnPanel>();
			panel.ResetComic();
			panel.RunDialog("Opening", new DialogStorage(), new global::Yarn.Library(), null, null, null, null);
		}

		// Token: 0x06000AC2 RID: 2754 RVA: 0x00036E48 File Offset: 0x00035048
		private void ConfirmSelectCharacter()
		{
			GameRunRecordSaveData gameRunRecordSaveData = Enumerable.LastOrDefault<GameRunRecordSaveData>(GameMaster.GetGameRunHistory());
			PuzzleFlag puzzleFlag;
			PuzzleToggleWidget puzzleToggleWidget;
			if (gameRunRecordSaveData != null)
			{
				this.SelectDifficulty((int)gameRunRecordSaveData.Difficulty, true);
				using (Dictionary<PuzzleFlag, PuzzleToggleWidget>.Enumerator enumerator = this._puzzleToggles.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<PuzzleFlag, PuzzleToggleWidget> keyValuePair = enumerator.Current;
						keyValuePair.Deconstruct(ref puzzleFlag, ref puzzleToggleWidget);
						PuzzleFlag puzzleFlag2 = puzzleFlag;
						PuzzleToggleWidget puzzleToggleWidget2 = puzzleToggleWidget;
						puzzleToggleWidget2.toggle.isOn = !puzzleToggleWidget2.IsLock && gameRunRecordSaveData.Puzzles.HasFlag(puzzleFlag2);
					}
					goto IL_00D8;
				}
			}
			this.SelectDifficulty(1, true);
			foreach (KeyValuePair<PuzzleFlag, PuzzleToggleWidget> keyValuePair in this._puzzleToggles)
			{
				KeyValuePair<PuzzleFlag, PuzzleToggleWidget> keyValuePair;
				keyValuePair.Deconstruct(ref puzzleFlag, ref puzzleToggleWidget);
				puzzleToggleWidget.toggle.isOn = false;
			}
			IL_00D8:
			this._currentPanelPhase = 3;
			this.characterPanelRoot.DOKill(false);
			this.difficultyPanelRoot.DOKill(false);
			this.difficultyPanelBg.DOKill(false);
			this.difficultyPanelRoot.gameObject.SetActive(true);
			Sequence sequence = DOTween.Sequence();
			sequence.Join(this.characterPanelRoot.DOFade(0f, 0.2f).From(1f, true, false));
			sequence.Join(this.difficultyPanelRoot.DOFade(1f, 0.2f).From(0f, true, false));
			sequence.Join(this.difficultyPanelBg.DOScaleY(1f, 0.4f).From(0f, true, false));
			sequence.OnComplete(delegate
			{
				this.characterPanelRoot.gameObject.SetActive(false);
			}).SetUpdate(true).SetTarget(this.characterPanelRoot);
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000AC3 RID: 2755 RVA: 0x00037028 File Offset: 0x00035228
		// (set) Token: 0x06000AC4 RID: 2756 RVA: 0x00037030 File Offset: 0x00035230
		private StartGamePanel.PuzzleToggleState ToggleStatus { get; set; }

		// Token: 0x06000AC5 RID: 2757 RVA: 0x0003703C File Offset: 0x0003523C
		private void InitialForPuzzle()
		{
			this.ToggleStatus = StartGamePanel.PuzzleToggleState.None;
			this.puzzleContent.DestroyChildren();
			for (int i = 0; i < PuzzleFlags.AllPuzzleFlags.Count; i++)
			{
				PuzzleToggleWidget puzzleToggleWidget = Object.Instantiate<PuzzleToggleWidget>(this.puzzleToggleTemplate, this.puzzleContent);
				this._puzzleToggles.Add(PuzzleFlags.AllPuzzleFlags[i], puzzleToggleWidget);
				if (i == PuzzleFlags.AllPuzzleFlags.Count - 1)
				{
					puzzleToggleWidget.SetEnd();
				}
				puzzleToggleWidget.SetPuzzle(PuzzleFlags.AllPuzzleFlags[i]);
				puzzleToggleWidget.Toggle.isOn = false;
				puzzleToggleWidget.Toggle.onValueChanged.AddListener(delegate(bool on)
				{
					this.ToggleStatus = StartGamePanel.PuzzleToggleState.Part;
					if (on)
					{
						if (Enumerable.All<PuzzleToggleWidget>(this._puzzleToggles.Values, (PuzzleToggleWidget w) => w.IsLock || w.Toggle.isOn))
						{
							this.ToggleStatus = StartGamePanel.PuzzleToggleState.All;
							this.puzzleSelectAllToggle.toggle.isOn = true;
							return;
						}
					}
					else
					{
						this.puzzleSelectAllToggle.toggle.isOn = false;
						if (Enumerable.All<PuzzleToggleWidget>(this._puzzleToggles.Values, (PuzzleToggleWidget w) => w.IsLock || !w.Toggle.isOn))
						{
							this.ToggleStatus = StartGamePanel.PuzzleToggleState.None;
						}
					}
				});
			}
			this.puzzleSelectAllToggle.toggle.isOn = false;
			this.puzzleSelectAllToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				if (on)
				{
					this.ToggleStatus = StartGamePanel.PuzzleToggleState.All;
					using (Dictionary<PuzzleFlag, PuzzleToggleWidget>.ValueCollection.Enumerator enumerator = this._puzzleToggles.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PuzzleToggleWidget puzzleToggleWidget2 = enumerator.Current;
							if (!puzzleToggleWidget2.IsLock)
							{
								puzzleToggleWidget2.Toggle.isOn = true;
							}
						}
						return;
					}
				}
				if (this.ToggleStatus == StartGamePanel.PuzzleToggleState.All)
				{
					this.ToggleStatus = StartGamePanel.PuzzleToggleState.None;
					foreach (PuzzleToggleWidget puzzleToggleWidget3 in this._puzzleToggles.Values)
					{
						if (!puzzleToggleWidget3.IsLock)
						{
							puzzleToggleWidget3.Toggle.isOn = false;
						}
					}
				}
			});
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x0003711C File Offset: 0x0003531C
		private void SetPuzzleStatus()
		{
			int num = 0;
			for (int i = 0; i < PuzzleFlags.AllPuzzleFlags.Count; i++)
			{
				bool flag = Singleton<GameMaster>.Instance.CurrentProfileLevel >= PuzzleConfig.AllConfig()[i].UnlockLevel;
				Enumerable.ToList<PuzzleToggleWidget>(this._puzzleToggles.Values)[i].IsLock = !flag;
				if (flag)
				{
					num++;
				}
			}
			this.puzzleSelectAllGroup.SetActive(num > 1);
			foreach (PuzzleToggleWidget puzzleToggleWidget in this._puzzleToggles.Values)
			{
				puzzleToggleWidget.Toggle.isOn = false;
			}
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x000371E4 File Offset: 0x000353E4
		private void InitialForSeed()
		{
			this._setSeed = false;
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x000371F0 File Offset: 0x000353F0
		private void OnSeedButtonClicked()
		{
			this.seedPanelRoot.SetActive(true);
			this.seedCanvasGroup.DOFade(1f, 0.2f).From(0f, true, false).OnComplete(delegate
			{
				this.seedCanvasGroup.interactable = true;
			})
				.SetUpdate(true);
			this.seedInputText.text = this._seedString ?? "";
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x0003725C File Offset: 0x0003545C
		private void OnSeedConfirmButtonClicked()
		{
			if (this.seedInputText.text == "")
			{
				this._seedString = null;
				this.CloseSeedPanel(new bool?(false));
				return;
			}
			ulong num;
			if (!RandomGen.TryParseSeed(this.seedInputText.text, out num))
			{
				this.seedTipText.DOKill(false);
				DOTween.Sequence().Append(this.seedTipText.DOFade(1f, 0.2f).From(0f, true, false)).AppendInterval(1f)
					.Append(this.seedTipText.DOFade(0f, 0.2f).From(1f, true, false))
					.SetTarget(this.seedTipText)
					.SetUpdate(true);
				return;
			}
			this._seedString = this.seedInputText.text;
			this.CloseSeedPanel(new bool?(true));
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x00037340 File Offset: 0x00035540
		private void OnSeedCancelButtonClicked()
		{
			this.CloseSeedPanel(default(bool?));
		}

		// Token: 0x06000ACB RID: 2763 RVA: 0x0003735C File Offset: 0x0003555C
		private void CloseSeedPanel(bool? setSeed)
		{
			this.seedCanvasGroup.interactable = false;
			this.seedCanvasGroup.DOFade(0f, 0.2f).From(1f, true, false).OnComplete(delegate
			{
				this.seedPanelRoot.SetActive(false);
				if (setSeed != null)
				{
					this._setSeed = setSeed.Value;
					this.SetNoClearHint();
					if (this._setSeed)
					{
						if (this.seedSetImage.fillAmount < 1f)
						{
							this.seedSetImage.DOFillAmount(1f, 0.3f).SetUpdate(true);
							return;
						}
					}
					else if (this.seedSetImage.fillAmount > 0f)
					{
						this.seedSetImage.DOFillAmount(0f, 0.3f).SetUpdate(true);
					}
				}
			})
				.SetUpdate(true);
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000ACC RID: 2764 RVA: 0x000373C2 File Offset: 0x000355C2
		private int ActiveJadeBoxCount
		{
			get
			{
				return Enumerable.Count<JadeBoxToggle>(this._jadeBoxToggles.Values, (JadeBoxToggle toggle) => toggle.IsOn);
			}
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x000373F4 File Offset: 0x000355F4
		private void InitialForJadeBox()
		{
			this.jadeBoxContent.DestroyChildren();
			foreach (JadeBoxConfig jadeBoxConfig in Enumerable.OrderBy<JadeBoxConfig, int>(Enumerable.Select<ValueTuple<Type, JadeBoxConfig>, JadeBoxConfig>(LBoL.Core.Library.EnumerateJadeBoxTypes(), (ValueTuple<Type, JadeBoxConfig> pair) => pair.Item2), (JadeBoxConfig config) => config.Index))
			{
				JadeBoxToggle toggle = Object.Instantiate<JadeBoxToggle>(this.jadeBoxTemplate, this.jadeBoxContent);
				JadeBox jadeBox = LBoL.Core.Library.CreateJadeBox(jadeBoxConfig.Id);
				toggle.JadeBox = jadeBox;
				toggle.gameObject.SetActive(true);
				this._jadeBoxToggles.Add(jadeBox, toggle);
				List<string> labels = Enumerable.ToList<string>(jadeBoxConfig.Group);
				toggle.Toggle.onValueChanged.AddListener(delegate(bool isOn)
				{
					AudioManager.Button(isOn ? 0 : 1);
					if (isOn)
					{
						using (List<string>.Enumerator enumerator2 = labels.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								string group = enumerator2.Current;
								foreach (JadeBoxToggle jadeBoxToggle in Enumerable.Select<KeyValuePair<JadeBox, JadeBoxToggle>, JadeBoxToggle>(Enumerable.Where<KeyValuePair<JadeBox, JadeBoxToggle>>(this._jadeBoxToggles, (KeyValuePair<JadeBox, JadeBoxToggle> pair) => Enumerable.Contains<string>(pair.Key.Config.Group, group)), (KeyValuePair<JadeBox, JadeBoxToggle> pair) => pair.Value))
								{
									jadeBoxToggle.Toggle.SetIsOnWithoutNotify(false);
								}
							}
						}
						toggle.Toggle.SetIsOnWithoutNotify(true);
					}
					this.RefreshJadeBoxIcon();
				});
			}
			this.SetNoClearHint();
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x0003752C File Offset: 0x0003572C
		private void JadeBoxLocalize()
		{
			foreach (JadeBoxToggle jadeBoxToggle in this._jadeBoxToggles.Values)
			{
				jadeBoxToggle.Refresh();
			}
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x00037584 File Offset: 0x00035784
		private void SetJadeBoxStatus()
		{
			bool flag = Singleton<GameMaster>.Instance.CurrentProfileLevel >= 10;
			this.jadeBoxButton.interactable = flag;
			this._jadeBoxTooltip.enabled = !flag;
			this.jadeBoxLockImage.gameObject.SetActive(!flag);
			this.jadeBoxText.color = (flag ? Color.white : new Color(1f, 1f, 1f, 0.2f));
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x00037600 File Offset: 0x00035800
		private void OnJadeBoxButtonClicked()
		{
			this.jadeBoxPanelRoot.SetActive(true);
			this.jadeBoxCanvasGroup.DOFade(1f, 0.2f).From(0f, true, false).OnComplete(delegate
			{
				this.jadeBoxCanvasGroup.interactable = true;
			})
				.SetUpdate(true);
			this.RefreshJadeBoxIcon();
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x00037658 File Offset: 0x00035858
		private void RefreshJadeBoxIcon()
		{
			this.jadeBoxCount.text = "StartGame.JadeBoxSelectedCount".LocalizeFormat(new object[] { this.ActiveJadeBoxCount, 3 });
			this.jadeBoxCount.color = ((this.ActiveJadeBoxCount < 3) ? Color.white : Color.yellow);
			foreach (KeyValuePair<JadeBox, JadeBoxToggle> keyValuePair in this._jadeBoxToggles)
			{
				keyValuePair.Value.bg.color = (keyValuePair.Value.IsOn ? Color.white : new Color(1f, 1f, 1f, 0.2f));
			}
			if (this.ActiveJadeBoxCount >= 3)
			{
				using (Dictionary<JadeBox, JadeBoxToggle>.Enumerator enumerator = this._jadeBoxToggles.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<JadeBox, JadeBoxToggle> keyValuePair2 = enumerator.Current;
						if (!keyValuePair2.Value.IsOn)
						{
							keyValuePair2.Value.icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
							keyValuePair2.Value.Toggle.enabled = false;
						}
					}
					return;
				}
			}
			foreach (KeyValuePair<JadeBox, JadeBoxToggle> keyValuePair3 in this._jadeBoxToggles)
			{
				keyValuePair3.Value.icon.color = Color.white;
				keyValuePair3.Value.Toggle.enabled = true;
			}
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x00037828 File Offset: 0x00035A28
		private void OnJadeBoxReTurnButtonClicked()
		{
			this.jadeBoxCanvasGroup.interactable = false;
			this.jadeBoxCanvasGroup.DOFade(0f, 0.2f).From(1f, true, false).OnComplete(delegate
			{
				this.jadeBoxPanelRoot.SetActive(false);
				this.SetNoClearHint();
				if (this.ActiveJadeBoxCount > 0)
				{
					if (this.jadeBoxSetImage.fillAmount < 1f)
					{
						this.jadeBoxSetImage.DOFillAmount(1f, 0.3f).SetUpdate(true);
						return;
					}
				}
				else if (this.jadeBoxSetImage.fillAmount > 0f)
				{
					this.jadeBoxSetImage.DOFillAmount(0f, 0.3f).SetUpdate(true);
				}
			})
				.SetUpdate(true);
		}

		// Token: 0x06000AD3 RID: 2771 RVA: 0x0003787A File Offset: 0x00035A7A
		private void SetNoClearHint()
		{
			this.noClearHint.gameObject.SetActive(this._setSeed || this.ActiveJadeBoxCount > 0);
		}

		// Token: 0x0400081F RID: 2079
		[Header("全局")]
		[SerializeField]
		private CanvasGroup backgroundManaPanel;

		// Token: 0x04000820 RID: 2080
		[SerializeField]
		private Transform backgroundRotationRoot;

		// Token: 0x04000821 RID: 2081
		[SerializeField]
		private CanvasGroup basicGroup;

		// Token: 0x04000822 RID: 2082
		[SerializeField]
		private Transform backgroundOuterRing;

		// Token: 0x04000823 RID: 2083
		[SerializeField]
		private Transform backgroundInnerRing;

		// Token: 0x04000824 RID: 2084
		[SerializeField]
		private Transform backgroundManaGroup;

		// Token: 0x04000825 RID: 2085
		[SerializeField]
		private AssociationList<ManaColor, Transform> backgroundManaList;

		// Token: 0x04000826 RID: 2086
		[SerializeField]
		private CanvasGroup nonBasicGroup;

		// Token: 0x04000827 RID: 2087
		[SerializeField]
		private AssociationList<ManaColor, Sprite> nonBasicManaSprites;

		// Token: 0x04000828 RID: 2088
		[SerializeField]
		private Image nonBasicLeftColor;

		// Token: 0x04000829 RID: 2089
		[SerializeField]
		private Image nonBasicRightColor;

		// Token: 0x0400082A RID: 2090
		[SerializeField]
		private Button returnButton;

		// Token: 0x0400082B RID: 2091
		[Header("角色面板")]
		[SerializeField]
		private CanvasGroup characterPanelRoot;

		// Token: 0x0400082C RID: 2092
		[SerializeField]
		private CanvasGroup characterPanelTopRoot;

		// Token: 0x0400082D RID: 2093
		[SerializeField]
		private CanvasGroup characterPanelBottomRoot;

		// Token: 0x0400082E RID: 2094
		[SerializeField]
		private CanvasGroup characterSetupRoot;

		// Token: 0x0400082F RID: 2095
		[SerializeField]
		private List<CanvasGroup> characterStandPicList;

		// Token: 0x04000830 RID: 2096
		[SerializeField]
		private Image mainStandPicShadow;

		// Token: 0x04000831 RID: 2097
		[SerializeField]
		private Image mainStandPic;

		// Token: 0x04000832 RID: 2098
		[SerializeField]
		private StartStatusWidget characterStatusWidget;

		// Token: 0x04000833 RID: 2099
		[SerializeField]
		private List<StartSetupWidget> characterSetupList;

		// Token: 0x04000834 RID: 2100
		[SerializeField]
		private TextMeshProUGUI characterHint;

		// Token: 0x04000835 RID: 2101
		[SerializeField]
		private GameObject characterHintRoot;

		// Token: 0x04000836 RID: 2102
		[SerializeField]
		private DeckHolder deckHolder;

		// Token: 0x04000837 RID: 2103
		[SerializeField]
		private Button deckReturnButton;

		// Token: 0x04000838 RID: 2104
		[SerializeField]
		private Button characterLeftButton;

		// Token: 0x04000839 RID: 2105
		[SerializeField]
		private Button characterRightButton;

		// Token: 0x0400083A RID: 2106
		[SerializeField]
		private SwitchWidget gameModeSwitch;

		// Token: 0x0400083B RID: 2107
		[SerializeField]
		private SwitchWidget randomResultSwitch;

		// Token: 0x0400083C RID: 2108
		[SerializeField]
		private Button characterConfirmButton;

		// Token: 0x0400083D RID: 2109
		[SerializeField]
		private CommonButtonWidget replayOpeningButton;

		// Token: 0x0400083E RID: 2110
		[Header("难度")]
		[SerializeField]
		private CanvasGroup difficultyPanelRoot;

		// Token: 0x0400083F RID: 2111
		[SerializeField]
		private Transform difficultyPanelBg;

		// Token: 0x04000840 RID: 2112
		[SerializeField]
		private Transform difficultyPanelRotationBg;

		// Token: 0x04000841 RID: 2113
		[SerializeField]
		private Button difficultyLeftButton;

		// Token: 0x04000842 RID: 2114
		[SerializeField]
		private Button difficultyRightButton;

		// Token: 0x04000843 RID: 2115
		[SerializeField]
		private Button difficultyConfirmButton;

		// Token: 0x04000844 RID: 2116
		[SerializeField]
		private DifficultyGroup[] difficultyGroups;

		// Token: 0x04000845 RID: 2117
		[SerializeField]
		private List<TextMeshProUGUI> difficultyText;

		// Token: 0x04000846 RID: 2118
		[SerializeField]
		private GameObject difficultyHint;

		// Token: 0x04000847 RID: 2119
		[Header("资源")]
		[SerializeField]
		private AssociationList<string, Sprite> standPicList;

		// Token: 0x04000848 RID: 2120
		[SerializeField]
		private Sprite lockedHeadPic;

		// Token: 0x04000849 RID: 2121
		[SerializeField]
		private AssociationList<string, Sprite> headPicList;

		// Token: 0x0400084A RID: 2122
		private readonly float[] _standPicScale = new float[] { 0.75f, 0.85f, 1f, 0.85f, 0.75f };

		// Token: 0x0400084B RID: 2123
		private readonly float[] _standPicAlpha = new float[] { 1f, 1f, 1f, 1f, 1f };

		// Token: 0x0400084C RID: 2124
		private readonly float[] _standPicX = new float[] { -1550f, -800f, 0f, 800f, 1550f };

		// Token: 0x0400084D RID: 2125
		private readonly float[] _standPicY = new float[] { 200f, 200f, 120f, 200f, 200f };

		// Token: 0x0400084E RID: 2126
		private static readonly GameDifficulty[] Difficulties = EnumHelper<GameDifficulty>.GetValues();

		// Token: 0x0400084F RID: 2127
		private bool _isDifficultyLock;

		// Token: 0x04000850 RID: 2128
		private int _playerIndex;

		// Token: 0x04000851 RID: 2129
		private PlayerUnit _player;

		// Token: 0x04000852 RID: 2130
		private List<PlayerUnit> _players;

		// Token: 0x04000853 RID: 2131
		private int _selectedType;

		// Token: 0x04000854 RID: 2132
		private StartGamePanel.TypeCandidate _typeCandidate;

		// Token: 0x04000855 RID: 2133
		private StartGamePanel.TypeCandidate[] _typeCandidates;

		// Token: 0x04000856 RID: 2134
		private int _difficultyIndex = 1;

		// Token: 0x04000857 RID: 2135
		private GameDifficulty _selectedDifficulty;

		// Token: 0x04000858 RID: 2136
		private string _seedString;

		// Token: 0x04000859 RID: 2137
		private Stage[] _stages;

		// Token: 0x0400085A RID: 2138
		private Type _debutAdventure;

		// Token: 0x0400085B RID: 2139
		private readonly Dictionary<PlayerUnitConfig, CharacterButtonWidget> _playerButtons = new Dictionary<PlayerUnitConfig, CharacterButtonWidget>();

		// Token: 0x0400085C RID: 2140
		private readonly Dictionary<PuzzleFlag, PuzzleToggleWidget> _puzzleToggles = new Dictionary<PuzzleFlag, PuzzleToggleWidget>();

		// Token: 0x0400085D RID: 2141
		private readonly Dictionary<JadeBox, JadeBoxToggle> _jadeBoxToggles = new Dictionary<JadeBox, JadeBoxToggle>();

		// Token: 0x0400085E RID: 2142
		private SimpleTooltipSource _modeTooltip;

		// Token: 0x0400085F RID: 2143
		private SimpleTooltipSource _randomTooltip;

		// Token: 0x04000860 RID: 2144
		private SimpleTooltipSource _confirmTooltip;

		// Token: 0x04000861 RID: 2145
		private SimpleTooltipSource _puzzleTooltip;

		// Token: 0x04000862 RID: 2146
		private SimpleTooltipSource _jadeBoxTooltip;

		// Token: 0x04000863 RID: 2147
		private int _currentPanelPhase = 1;

		// Token: 0x04000864 RID: 2148
		private bool _showingBasicRing = true;

		// Token: 0x04000865 RID: 2149
		private Sequence _nonBasicTween;

		// Token: 0x04000866 RID: 2150
		private const float TweenTime = 0.3f;

		// Token: 0x04000867 RID: 2151
		private const float TweenMoveX = 1000f;

		// Token: 0x04000868 RID: 2152
		[Header("难题")]
		[SerializeField]
		private Transform puzzleContent;

		// Token: 0x04000869 RID: 2153
		[SerializeField]
		private PuzzleToggleWidget puzzleToggleTemplate;

		// Token: 0x0400086A RID: 2154
		[SerializeField]
		private GameObject puzzleHint;

		// Token: 0x0400086B RID: 2155
		[SerializeField]
		private GameObject puzzleSelectAllGroup;

		// Token: 0x0400086C RID: 2156
		[SerializeField]
		private CommonToggleWidget puzzleSelectAllToggle;

		// Token: 0x0400086D RID: 2157
		[Header("游戏种子")]
		[SerializeField]
		private Button seedButton;

		// Token: 0x0400086E RID: 2158
		[SerializeField]
		private CanvasGroup seedCanvasGroup;

		// Token: 0x0400086F RID: 2159
		[SerializeField]
		private Image seedSetImage;

		// Token: 0x04000870 RID: 2160
		[SerializeField]
		private GameObject seedPanelRoot;

		// Token: 0x04000871 RID: 2161
		[SerializeField]
		private Button seedConfirmButton;

		// Token: 0x04000872 RID: 2162
		[SerializeField]
		private Button seedCancelButton;

		// Token: 0x04000873 RID: 2163
		[SerializeField]
		private TMP_InputField seedInputText;

		// Token: 0x04000874 RID: 2164
		[SerializeField]
		private TextMeshProUGUI seedTipText;

		// Token: 0x04000875 RID: 2165
		[Header("玉匣")]
		[SerializeField]
		private Button jadeBoxButton;

		// Token: 0x04000876 RID: 2166
		[SerializeField]
		private Image jadeBoxLockImage;

		// Token: 0x04000877 RID: 2167
		[SerializeField]
		private TextMeshProUGUI jadeBoxText;

		// Token: 0x04000878 RID: 2168
		[SerializeField]
		private CanvasGroup jadeBoxCanvasGroup;

		// Token: 0x04000879 RID: 2169
		[SerializeField]
		private Image jadeBoxSetImage;

		// Token: 0x0400087A RID: 2170
		[SerializeField]
		private GameObject jadeBoxPanelRoot;

		// Token: 0x0400087B RID: 2171
		[SerializeField]
		private Button jadeBoxReturnButton;

		// Token: 0x0400087C RID: 2172
		[SerializeField]
		private RectTransform jadeBoxContent;

		// Token: 0x0400087D RID: 2173
		[SerializeField]
		private JadeBoxToggle jadeBoxTemplate;

		// Token: 0x0400087E RID: 2174
		[SerializeField]
		private TextMeshProUGUI jadeBoxCount;

		// Token: 0x0400087F RID: 2175
		private bool _setSeed;

		// Token: 0x04000880 RID: 2176
		[SerializeField]
		private TextMeshProUGUI noClearHint;

		// Token: 0x04000881 RID: 2177
		private const float FadeTime = 0.2f;

		// Token: 0x04000882 RID: 2178
		private const float CircleTweenTime = 0.3f;

		// Token: 0x04000884 RID: 2180
		private const int JadeBoxUnlockLevel = 10;

		// Token: 0x04000885 RID: 2181
		private const int MaxJadeBox = 3;

		// Token: 0x020002D1 RID: 721
		private class TypeCandidate
		{
			// Token: 0x1700048C RID: 1164
			// (get) Token: 0x06001735 RID: 5941 RVA: 0x00067DBA File Offset: 0x00065FBA
			// (set) Token: 0x06001736 RID: 5942 RVA: 0x00067DC2 File Offset: 0x00065FC2
			public string Name { get; set; }

			// Token: 0x1700048D RID: 1165
			// (get) Token: 0x06001737 RID: 5943 RVA: 0x00067DCB File Offset: 0x00065FCB
			// (set) Token: 0x06001738 RID: 5944 RVA: 0x00067DD3 File Offset: 0x00065FD3
			public UltimateSkill Us { get; set; }

			// Token: 0x1700048E RID: 1166
			// (get) Token: 0x06001739 RID: 5945 RVA: 0x00067DDC File Offset: 0x00065FDC
			// (set) Token: 0x0600173A RID: 5946 RVA: 0x00067DE4 File Offset: 0x00065FE4
			public Exhibit Exhibit { get; set; }

			// Token: 0x1700048F RID: 1167
			// (get) Token: 0x0600173B RID: 5947 RVA: 0x00067DED File Offset: 0x00065FED
			// (set) Token: 0x0600173C RID: 5948 RVA: 0x00067DF5 File Offset: 0x00065FF5
			public Card[] Deck { get; set; }
		}

		// Token: 0x020002D2 RID: 722
		public enum PuzzleToggleState
		{
			// Token: 0x04001272 RID: 4722
			All,
			// Token: 0x04001273 RID: 4723
			Part,
			// Token: 0x04001274 RID: 4724
			None
		}
	}
}
