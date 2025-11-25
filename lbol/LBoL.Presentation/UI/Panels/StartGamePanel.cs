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
	public class StartGamePanel : UiPanel<StartGameData>, IInputActionHandler
	{
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
		protected override void OnHiding()
		{
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Idle);
			UiManager.PopActionHandler(this);
		}
		public override void OnLocaleChanged()
		{
			base.OnLocaleChanged();
			for (int i = 0; i < 4; i++)
			{
				this.difficultyText[i].text = ("Tooltip.Difficulty" + StartGamePanel.Difficulties[i].ToString() + ".Description").Localize(true);
			}
			this.JadeBoxLocalize();
		}
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
		public void BeginSkipDialog()
		{
			UiManager.GetPanel<VnPanel>().UserSkipDialog(true);
		}
		public void EndSkipDialog()
		{
			UiManager.GetPanel<VnPanel>().UserSkipDialog(false);
		}
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
		public void UI_ReplayOpening()
		{
			VnPanel panel = UiManager.GetPanel<VnPanel>();
			panel.ResetComic();
			panel.RunDialog("Opening", new DialogStorage(), new global::Yarn.Library(), null, null, null, null);
		}
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
		private StartGamePanel.PuzzleToggleState ToggleStatus { get; set; }
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
		private void InitialForSeed()
		{
			this._setSeed = false;
		}
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
		private void OnSeedCancelButtonClicked()
		{
			this.CloseSeedPanel(default(bool?));
		}
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
		private int ActiveJadeBoxCount
		{
			get
			{
				return Enumerable.Count<JadeBoxToggle>(this._jadeBoxToggles.Values, (JadeBoxToggle toggle) => toggle.IsOn);
			}
		}
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
		private void JadeBoxLocalize()
		{
			foreach (JadeBoxToggle jadeBoxToggle in this._jadeBoxToggles.Values)
			{
				jadeBoxToggle.Refresh();
			}
		}
		private void SetJadeBoxStatus()
		{
			bool flag = Singleton<GameMaster>.Instance.CurrentProfileLevel >= 10;
			this.jadeBoxButton.interactable = flag;
			this._jadeBoxTooltip.enabled = !flag;
			this.jadeBoxLockImage.gameObject.SetActive(!flag);
			this.jadeBoxText.color = (flag ? Color.white : new Color(1f, 1f, 1f, 0.2f));
		}
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
		private void SetNoClearHint()
		{
			this.noClearHint.gameObject.SetActive(this._setSeed || this.ActiveJadeBoxCount > 0);
		}
		[Header("全局")]
		[SerializeField]
		private CanvasGroup backgroundManaPanel;
		[SerializeField]
		private Transform backgroundRotationRoot;
		[SerializeField]
		private CanvasGroup basicGroup;
		[SerializeField]
		private Transform backgroundOuterRing;
		[SerializeField]
		private Transform backgroundInnerRing;
		[SerializeField]
		private Transform backgroundManaGroup;
		[SerializeField]
		private AssociationList<ManaColor, Transform> backgroundManaList;
		[SerializeField]
		private CanvasGroup nonBasicGroup;
		[SerializeField]
		private AssociationList<ManaColor, Sprite> nonBasicManaSprites;
		[SerializeField]
		private Image nonBasicLeftColor;
		[SerializeField]
		private Image nonBasicRightColor;
		[SerializeField]
		private Button returnButton;
		[Header("角色面板")]
		[SerializeField]
		private CanvasGroup characterPanelRoot;
		[SerializeField]
		private CanvasGroup characterPanelTopRoot;
		[SerializeField]
		private CanvasGroup characterPanelBottomRoot;
		[SerializeField]
		private CanvasGroup characterSetupRoot;
		[SerializeField]
		private List<CanvasGroup> characterStandPicList;
		[SerializeField]
		private Image mainStandPicShadow;
		[SerializeField]
		private Image mainStandPic;
		[SerializeField]
		private StartStatusWidget characterStatusWidget;
		[SerializeField]
		private List<StartSetupWidget> characterSetupList;
		[SerializeField]
		private TextMeshProUGUI characterHint;
		[SerializeField]
		private GameObject characterHintRoot;
		[SerializeField]
		private DeckHolder deckHolder;
		[SerializeField]
		private Button deckReturnButton;
		[SerializeField]
		private Button characterLeftButton;
		[SerializeField]
		private Button characterRightButton;
		[SerializeField]
		private SwitchWidget gameModeSwitch;
		[SerializeField]
		private SwitchWidget randomResultSwitch;
		[SerializeField]
		private Button characterConfirmButton;
		[SerializeField]
		private CommonButtonWidget replayOpeningButton;
		[Header("难度")]
		[SerializeField]
		private CanvasGroup difficultyPanelRoot;
		[SerializeField]
		private Transform difficultyPanelBg;
		[SerializeField]
		private Transform difficultyPanelRotationBg;
		[SerializeField]
		private Button difficultyLeftButton;
		[SerializeField]
		private Button difficultyRightButton;
		[SerializeField]
		private Button difficultyConfirmButton;
		[SerializeField]
		private DifficultyGroup[] difficultyGroups;
		[SerializeField]
		private List<TextMeshProUGUI> difficultyText;
		[SerializeField]
		private GameObject difficultyHint;
		[Header("资源")]
		[SerializeField]
		private AssociationList<string, Sprite> standPicList;
		[SerializeField]
		private Sprite lockedHeadPic;
		[SerializeField]
		private AssociationList<string, Sprite> headPicList;
		private readonly float[] _standPicScale = new float[] { 0.75f, 0.85f, 1f, 0.85f, 0.75f };
		private readonly float[] _standPicAlpha = new float[] { 1f, 1f, 1f, 1f, 1f };
		private readonly float[] _standPicX = new float[] { -1550f, -800f, 0f, 800f, 1550f };
		private readonly float[] _standPicY = new float[] { 200f, 200f, 120f, 200f, 200f };
		private static readonly GameDifficulty[] Difficulties = EnumHelper<GameDifficulty>.GetValues();
		private bool _isDifficultyLock;
		private int _playerIndex;
		private PlayerUnit _player;
		private List<PlayerUnit> _players;
		private int _selectedType;
		private StartGamePanel.TypeCandidate _typeCandidate;
		private StartGamePanel.TypeCandidate[] _typeCandidates;
		private int _difficultyIndex = 1;
		private GameDifficulty _selectedDifficulty;
		private string _seedString;
		private Stage[] _stages;
		private Type _debutAdventure;
		private readonly Dictionary<PlayerUnitConfig, CharacterButtonWidget> _playerButtons = new Dictionary<PlayerUnitConfig, CharacterButtonWidget>();
		private readonly Dictionary<PuzzleFlag, PuzzleToggleWidget> _puzzleToggles = new Dictionary<PuzzleFlag, PuzzleToggleWidget>();
		private readonly Dictionary<JadeBox, JadeBoxToggle> _jadeBoxToggles = new Dictionary<JadeBox, JadeBoxToggle>();
		private SimpleTooltipSource _modeTooltip;
		private SimpleTooltipSource _randomTooltip;
		private SimpleTooltipSource _confirmTooltip;
		private SimpleTooltipSource _puzzleTooltip;
		private SimpleTooltipSource _jadeBoxTooltip;
		private int _currentPanelPhase = 1;
		private bool _showingBasicRing = true;
		private Sequence _nonBasicTween;
		private const float TweenTime = 0.3f;
		private const float TweenMoveX = 1000f;
		[Header("难题")]
		[SerializeField]
		private Transform puzzleContent;
		[SerializeField]
		private PuzzleToggleWidget puzzleToggleTemplate;
		[SerializeField]
		private GameObject puzzleHint;
		[SerializeField]
		private GameObject puzzleSelectAllGroup;
		[SerializeField]
		private CommonToggleWidget puzzleSelectAllToggle;
		[Header("游戏种子")]
		[SerializeField]
		private Button seedButton;
		[SerializeField]
		private CanvasGroup seedCanvasGroup;
		[SerializeField]
		private Image seedSetImage;
		[SerializeField]
		private GameObject seedPanelRoot;
		[SerializeField]
		private Button seedConfirmButton;
		[SerializeField]
		private Button seedCancelButton;
		[SerializeField]
		private TMP_InputField seedInputText;
		[SerializeField]
		private TextMeshProUGUI seedTipText;
		[Header("玉匣")]
		[SerializeField]
		private Button jadeBoxButton;
		[SerializeField]
		private Image jadeBoxLockImage;
		[SerializeField]
		private TextMeshProUGUI jadeBoxText;
		[SerializeField]
		private CanvasGroup jadeBoxCanvasGroup;
		[SerializeField]
		private Image jadeBoxSetImage;
		[SerializeField]
		private GameObject jadeBoxPanelRoot;
		[SerializeField]
		private Button jadeBoxReturnButton;
		[SerializeField]
		private RectTransform jadeBoxContent;
		[SerializeField]
		private JadeBoxToggle jadeBoxTemplate;
		[SerializeField]
		private TextMeshProUGUI jadeBoxCount;
		private bool _setSeed;
		[SerializeField]
		private TextMeshProUGUI noClearHint;
		private const float FadeTime = 0.2f;
		private const float CircleTweenTime = 0.3f;
		private const int JadeBoxUnlockLevel = 10;
		private const int MaxJadeBox = 3;
		private class TypeCandidate
		{
			public string Name { get; set; }
			public UltimateSkill Us { get; set; }
			public Exhibit Exhibit { get; set; }
			public Card[] Deck { get; set; }
		}
		public enum PuzzleToggleState
		{
			All,
			Part,
			None
		}
	}
}
