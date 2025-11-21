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
	// Token: 0x020000A1 RID: 161
	public class MainMenuPanel : UiPanel
	{
		// Token: 0x17000162 RID: 354
		// (get) Token: 0x0600084B RID: 2123 RVA: 0x00027F3E File Offset: 0x0002613E
		public bool IsEarlyAccessShowing
		{
			get
			{
				return this.earlyAccessHint.activeSelf;
			}
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x0600084C RID: 2124 RVA: 0x00027F4B File Offset: 0x0002614B
		public bool IsMainMenuGourp
		{
			get
			{
				return this.mainMenuButtonGroup.gameObject.activeSelf;
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x0600084D RID: 2125 RVA: 0x00027F5D File Offset: 0x0002615D
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x0600084E RID: 2126 RVA: 0x00027F60 File Offset: 0x00026160
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

		// Token: 0x0600084F RID: 2127 RVA: 0x00027F9C File Offset: 0x0002619C
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

		// Token: 0x06000850 RID: 2128 RVA: 0x00028059 File Offset: 0x00026259
		protected override void OnShowing()
		{
			this.RefreshProfile();
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x00028061 File Offset: 0x00026261
		public override void OnLocaleChanged()
		{
			this.RefreshProfileName();
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x00028069 File Offset: 0x00026269
		public void UI_RestoreGameClicked()
		{
			GameMaster.RestoreGameRun(Singleton<GameMaster>.Instance.GameRunSaveData);
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x0002807C File Offset: 0x0002627C
		public void UI_NewGameClicked()
		{
			if (Singleton<GameMaster>.Instance.CurrentSaveIndex == null)
			{
				UiManager.Show<ProfilePanel>();
			}
			this.NewGame();
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x000280A8 File Offset: 0x000262A8
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

		// Token: 0x06000855 RID: 2133 RVA: 0x000280E4 File Offset: 0x000262E4
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

		// Token: 0x06000856 RID: 2134 RVA: 0x0002810B File Offset: 0x0002630B
		public void ChangeToLogo()
		{
			if (this._isLogoState)
			{
				return;
			}
			base.GetComponent<Animator>().SetTrigger("EnterLogo");
			this._isLogoState = true;
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x0002812D File Offset: 0x0002632D
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

		// Token: 0x06000858 RID: 2136 RVA: 0x00028157 File Offset: 0x00026357
		public void SkipLogo()
		{
			base.GetComponent<Animator>().SetTrigger("SkipLogo");
			this._isLogoState = false;
		}

		// Token: 0x06000859 RID: 2137 RVA: 0x00028170 File Offset: 0x00026370
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

		// Token: 0x0600085A RID: 2138 RVA: 0x000282DF File Offset: 0x000264DF
		protected override void OnHiding()
		{
			base.OnHiding();
			this.shadingAnimate = false;
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x000282EE File Offset: 0x000264EE
		protected override void OnHided()
		{
			this.KillLoopTweens();
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x000282F6 File Offset: 0x000264F6
		protected void OnDestroy()
		{
			this.KillLoopTweens();
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x00028300 File Offset: 0x00026500
		private void KillLoopTweens()
		{
			this.shading1.DOKill(false);
			this.shading1.transform.DOKill(false);
			this.shading2.DOKill(false);
			this.shading2.transform.DOKill(false);
			this.smoke.DOKill(false);
			this.smoke.transform.DOKill(false);
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x0002836C File Offset: 0x0002656C
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

		// Token: 0x0600085F RID: 2143 RVA: 0x000284A3 File Offset: 0x000266A3
		private void NewGame()
		{
			UiManager.GetPanel<StartGamePanel>().Show(MainMenuPanel.DefaultMode);
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x000284B4 File Offset: 0x000266B4
		private void AbandonGame()
		{
			GameMaster.RequestAbandonGameRun(false);
			this.RefreshProfile();
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x000284C2 File Offset: 0x000266C2
		public void UI_ShowMuseum()
		{
			UiManager.Show<MuseumPanel>();
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x000284C9 File Offset: 0x000266C9
		public void UI_Settings()
		{
			UiManager.GetPanel<SettingPanel>().Show(SettingsPanelType.MainMenu);
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x000284D6 File Offset: 0x000266D6
		public void UI_ShowSubMenu()
		{
			this.StartMenuButtonAnim(0f, 1);
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x000284E4 File Offset: 0x000266E4
		public void UI_ShowMainMenu()
		{
			this.StartMenuButtonAnim(0f, 0);
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x000284F2 File Offset: 0x000266F2
		public void UI_ShowLicenses()
		{
			UiManager.Show<LicensesPanel>();
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x000284F9 File Offset: 0x000266F9
		public void UI_ShowMusicRoom()
		{
			UiManager.Show<MusicRoomPanel>();
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x00028500 File Offset: 0x00026700
		public void UI_ShowComplexRules()
		{
			UiManager.Show<ComplexRulesPanel>();
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x00028507 File Offset: 0x00026707
		public void UI_ShowCredits()
		{
			UiManager.Show<CreditsPanel>();
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0002850E File Offset: 0x0002670E
		public void UI_ShowHistory()
		{
			UiManager.Show<HistoryPanel>();
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x00028515 File Offset: 0x00026715
		public void UI_QuitGame()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "QuitGame",
				Buttons = DialogButtons.ConfirmCancel,
				OnConfirm = new Action(GameMaster.QuitGame)
			});
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x0002854A File Offset: 0x0002674A
		public void UI_SwitchEarlyAccessHint(bool isOn)
		{
			this.earlyAccessHint.SetActive(isOn);
			GamepadNavigationManager.RefreshSelection();
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x0002855D File Offset: 0x0002675D
		public void UI_ShowProfilePanel()
		{
			AudioManager.Button(0);
			UiManager.Show<ProfilePanel>();
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x0002856C File Offset: 0x0002676C
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

		// Token: 0x0600086E RID: 2158 RVA: 0x00028634 File Offset: 0x00026834
		private void RefreshProfileName()
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			this.profileNameText.text = ((currentProfile != null) ? currentProfile.Name : null) ?? "";
			this.profileLevelText.text = "Game.UnlockLevel".Localize(true) + string.Format("   {0}", ExpHelper.GetLevelForTotalExp((currentProfile != null) ? currentProfile.Exp : 0));
		}

		// Token: 0x040005EA RID: 1514
		[SerializeField]
		private TextMeshProUGUI versionText;

		// Token: 0x040005EB RID: 1515
		[SerializeField]
		private TextMeshProUGUI profileNameText;

		// Token: 0x040005EC RID: 1516
		[SerializeField]
		private TextMeshProUGUI profileLevelText;

		// Token: 0x040005ED RID: 1517
		[SerializeField]
		private Image profileHead;

		// Token: 0x040005EE RID: 1518
		[SerializeField]
		private Button newGameButton;

		// Token: 0x040005EF RID: 1519
		[SerializeField]
		private Button restoreGameButton;

		// Token: 0x040005F0 RID: 1520
		[SerializeField]
		private Button abandonGameButton;

		// Token: 0x040005F1 RID: 1521
		[Header("动画组件引用")]
		[SerializeField]
		private Transform mainMenuButtonGroup;

		// Token: 0x040005F2 RID: 1522
		[SerializeField]
		private Transform subMenuButtonGroup;

		// Token: 0x040005F3 RID: 1523
		[SerializeField]
		private Image shading1;

		// Token: 0x040005F4 RID: 1524
		[SerializeField]
		private Image shading2;

		// Token: 0x040005F5 RID: 1525
		[SerializeField]
		private Image smoke;

		// Token: 0x040005F6 RID: 1526
		[Header("EarlyAccess")]
		[SerializeField]
		private Button earlyAccessButton;

		// Token: 0x040005F7 RID: 1527
		[SerializeField]
		private Button earlyAccessReturn;

		// Token: 0x040005F8 RID: 1528
		[SerializeField]
		private GameObject earlyAccessHint;

		// Token: 0x040005F9 RID: 1529
		[SerializeField]
		private Button earlyAccessBackButton;

		// Token: 0x040005FA RID: 1530
		[SerializeField]
		private Button discordButton;

		// Token: 0x040005FB RID: 1531
		private bool shadingAnimate;

		// Token: 0x040005FC RID: 1532
		private bool _isLogoState = true;

		// Token: 0x040005FD RID: 1533
		private CanvasGroup canvasGroup;

		// Token: 0x040005FE RID: 1534
		private static bool _needSkipLogo;
	}
}
