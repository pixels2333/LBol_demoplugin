using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class MainMenuPanel : UiPanel
	{
		public bool IsEarlyAccessShowing
		{
			get
			{
				return this.earlyAccessHint.activeSelf;
			}
		}
		public bool IsMainMenuGourp
		{
			get
			{
				return this.mainMenuButtonGroup.gameObject.activeSelf;
			}
		}
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
		private static StartGameData DefaultMode
		{
			get
			{
				StartGameData startGameData = new StartGameData();
				startGameData.StagesCreateFunc = () => new Stage[]
				{
					Library.CreateStage<BambooForest>(),
					Library.CreateStage<XuanwuRavine>(),
					Library.CreateStage<WindGodLake>().AsNormalFinal(),
					Library.CreateStage<FinalStage>().AsTrueEndFinal()
				};
				startGameData.DebutAdventure = typeof(Debut);
				return startGameData;
			}
		}
		public void Awake()
		{
			this.canvasGroup = base.GetComponent<CanvasGroup>();
			this.versionText.text = VersionInfo.Current.Version;
			this.earlyAccessReturn.onClick.AddListener(delegate
			{
				this.UI_SwitchEarlyAccessHint(false);
			});
			this.earlyAccessBackButton.onClick.AddListener(delegate
			{
				this.UI_SwitchEarlyAccessHint(false);
			});
			this.earlyAccessButton.onClick.AddListener(delegate
			{
				this.UI_SwitchEarlyAccessHint(true);
			});
			this.earlyAccessHint.SetActive(false);
			this.discordButton.onClick.AddListener(delegate
			{
				Application.OpenURL("https://discord.gg/2KKWzJqWxS");
			});
		}
		protected override void OnShowing()
		{
			this.RefreshProfile();
		}
		public override void OnLocaleChanged()
		{
			this.RefreshProfileName();
		}
		public void UI_RestoreGameClicked()
		{
			GameMaster.RestoreGameRun(Singleton<GameMaster>.Instance.GameRunSaveData);
		}
		public void UI_NewGameClicked()
		{
			if (Singleton<GameMaster>.Instance.CurrentSaveIndex == null)
			{
				UiManager.Show<ProfilePanel>();
			}
			this.NewGame();
		}
		public void UI_AbandonGameClicked()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "GameAbandonWarning",
				Icon = MessageIcon.Warning,
				Buttons = DialogButtons.ConfirmCancel,
				OnConfirm = new Action(this.AbandonGame)
			});
		}
		protected override void OnShown()
		{
			if (MainMenuPanel._needSkipLogo)
			{
				this.SkipLogo();
				this.StartMenuButtonAnim(0f, 0);
			}
			MainMenuPanel._needSkipLogo = true;
			this.StartLoopTweens();
		}
		public void ChangeToLogo()
		{
			if (this._isLogoState)
			{
				return;
			}
			base.GetComponent<Animator>().SetTrigger("EnterLogo");
			this._isLogoState = true;
		}
		public void ChangeToMain(float delay = 0f)
		{
			if (!this._isLogoState)
			{
				return;
			}
			this.StartMenuButtonAnim(delay, 0);
			base.GetComponent<Animator>().SetTrigger("EnterMain");
			this._isLogoState = false;
		}
		public void SkipLogo()
		{
			base.GetComponent<Animator>().SetTrigger("SkipLogo");
			this._isLogoState = false;
		}
		public void StartMenuButtonAnim(float delay, int menuType = 0)
		{
			Transform currentGroup;
			Transform transform;
			if (menuType == 0)
			{
				transform = this.mainMenuButtonGroup;
				currentGroup = this.subMenuButtonGroup;
			}
			else
			{
				transform = this.subMenuButtonGroup;
				currentGroup = this.mainMenuButtonGroup;
			}
			if (currentGroup.gameObject.activeSelf)
			{
				for (int i = 0; i < currentGroup.childCount; i++)
				{
					currentGroup.GetChild(i).GetComponent<MainMenuButtonWidget>().enabled = false;
				}
				TweenerCore<float, float, FloatOptions> tweenerCore = currentGroup.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).SetUpdate(true)
					.From(1f, true, false)
					.SetAutoKill(true);
				tweenerCore.onKill = (TweenCallback)Delegate.Combine(tweenerCore.onKill, delegate
				{
					currentGroup.gameObject.SetActive(false);
					for (int k = 0; k < currentGroup.childCount; k++)
					{
						currentGroup.GetChild(k).GetComponent<MainMenuButtonWidget>().enabled = true;
					}
					GamepadNavigationManager.RefreshSelection();
				});
			}
			transform.GetComponent<CanvasGroup>().alpha = 1f;
			for (int j = 0; j < transform.childCount; j++)
			{
				transform.GetChild(j).DOLocalMoveX(0f, 0.6f, false).SetEase(Ease.OutCubic)
					.SetUpdate(true)
					.From(-1100f, true, false)
					.SetLink(base.gameObject)
					.SetDelay(delay + (float)j * 0.1f)
					.OnKill(delegate
					{
					});
			}
			transform.gameObject.SetActive(true);
		}
		protected override void OnHiding()
		{
			base.OnHiding();
			this.shadingAnimate = false;
		}
		protected override void OnHided()
		{
			this.KillLoopTweens();
		}
		protected void OnDestroy()
		{
			this.KillLoopTweens();
		}
		private void KillLoopTweens()
		{
			this.shading1.DOKill(false);
			this.shading1.transform.DOKill(false);
			this.shading2.DOKill(false);
			this.shading2.transform.DOKill(false);
			this.smoke.DOKill(false);
			this.smoke.transform.DOKill(false);
		}
		private void StartLoopTweens()
		{
			if (this.shading1 == null || this.shading2 == null || this.shadingAnimate)
			{
				return;
			}
			this.shading1.DOFade(0.6f, 1f).From(0f, true, false).SetUpdate(true);
			this.shading1.transform.DOLocalMoveX(1024f, 5f, false).From(0f, true, false).SetLoops(-1)
				.SetEase(Ease.Linear)
				.SetUpdate(true);
			this.shading2.DOFade(0.4f, 1f).From(0f, true, false).SetUpdate(true);
			this.shading2.transform.DOLocalMoveX(1024f, 4f, false).From(0f, true, false).SetLoops(-1)
				.SetEase(Ease.Linear)
				.SetUpdate(true);
			this.shadingAnimate = true;
			this.smoke.GetComponent<RectTransform>().DOAnchorPosX(-5120f, 100f, false).From(Vector2.zero, true, false)
				.SetLoops(-1)
				.SetEase(Ease.Linear)
				.SetUpdate(true);
		}
		private void NewGame()
		{
			UiManager.GetPanel<StartGamePanel>().Show(MainMenuPanel.DefaultMode);
		}
		private void AbandonGame()
		{
			GameMaster.RequestAbandonGameRun(false);
			this.RefreshProfile();
		}
		public void UI_ShowMuseum()
		{
			UiManager.Show<MuseumPanel>();
		}
		public void UI_Settings()
		{
			UiManager.GetPanel<SettingPanel>().Show(SettingsPanelType.MainMenu);
		}
		public void UI_ShowSubMenu()
		{
			this.StartMenuButtonAnim(0f, 1);
		}
		public void UI_ShowMainMenu()
		{
			this.StartMenuButtonAnim(0f, 0);
		}
		public void UI_ShowLicenses()
		{
			UiManager.Show<LicensesPanel>();
		}
		public void UI_ShowMusicRoom()
		{
			UiManager.Show<MusicRoomPanel>();
		}
		public void UI_ShowComplexRules()
		{
			UiManager.Show<ComplexRulesPanel>();
		}
		public void UI_ShowCredits()
		{
			UiManager.Show<CreditsPanel>();
		}
		public void UI_ShowHistory()
		{
			UiManager.Show<HistoryPanel>();
		}
		public void UI_QuitGame()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "QuitGame",
				Buttons = DialogButtons.ConfirmCancel,
				OnConfirm = new Action(GameMaster.QuitGame)
			});
		}
		public void UI_SwitchEarlyAccessHint(bool isOn)
		{
			this.earlyAccessHint.SetActive(isOn);
			GamepadNavigationManager.RefreshSelection();
		}
		public void UI_ShowProfilePanel()
		{
			AudioManager.Button(0);
			UiManager.Show<ProfilePanel>();
		}
		public void RefreshProfile()
		{
			if (Singleton<GameMaster>.Instance.GameRunSaveData != null)
			{
				this.restoreGameButton.gameObject.SetActive(true);
				this.abandonGameButton.gameObject.SetActive(true);
				this.newGameButton.gameObject.SetActive(false);
				this.profileHead.sprite = UiManager.GetPanel<ProfilePanel>().GetHeadSprite(Singleton<GameMaster>.Instance.GameRunSaveData.Player.Name);
			}
			else
			{
				this.restoreGameButton.gameObject.SetActive(false);
				this.abandonGameButton.gameObject.SetActive(false);
				this.newGameButton.gameObject.SetActive(true);
				this.profileHead.sprite = UiManager.GetPanel<ProfilePanel>().GetHeadSprite(null);
			}
			this.RefreshProfileName();
		}
		private void RefreshProfileName()
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			this.profileNameText.text = ((currentProfile != null) ? currentProfile.Name : null) ?? "";
			this.profileLevelText.text = "Game.UnlockLevel".Localize(true) + string.Format("   {0}", ExpHelper.GetLevelForTotalExp((currentProfile != null) ? currentProfile.Exp : 0));
		}
		[SerializeField]
		private TextMeshProUGUI versionText;
		[SerializeField]
		private TextMeshProUGUI profileNameText;
		[SerializeField]
		private TextMeshProUGUI profileLevelText;
		[SerializeField]
		private Image profileHead;
		[SerializeField]
		private Button newGameButton;
		[SerializeField]
		private Button restoreGameButton;
		[SerializeField]
		private Button abandonGameButton;
		[Header("动画组件引用")]
		[SerializeField]
		private Transform mainMenuButtonGroup;
		[SerializeField]
		private Transform subMenuButtonGroup;
		[SerializeField]
		private Image shading1;
		[SerializeField]
		private Image shading2;
		[SerializeField]
		private Image smoke;
		[Header("EarlyAccess")]
		[SerializeField]
		private Button earlyAccessButton;
		[SerializeField]
		private Button earlyAccessReturn;
		[SerializeField]
		private GameObject earlyAccessHint;
		[SerializeField]
		private Button earlyAccessBackButton;
		[SerializeField]
		private Button discordButton;
		private bool shadingAnimate;
		private bool _isLogoState = true;
		private CanvasGroup canvasGroup;
		private static bool _needSkipLogo;
	}
}
