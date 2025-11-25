using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation.I10N;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class SettingPanel : UiPanel<SettingsPanelType>, IInputActionHandler
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		private void ResetMainSettings()
		{
			this._resolutions = ResolutionHelper.GetAvailableResolutions();
			this.resolutionDropdown.dropdown.ClearOptions();
			this.resolutionDropdown.dropdown.AddOptions(Enumerable.ToList<string>(Enumerable.Select<Vector2Int, string>(this._resolutions, (Vector2Int res) => string.Format("{0} x {1}", res.x, res.y))));
			int num = this._resolutions.IndexOf(new Vector2Int(Screen.width, Screen.height));
			if (num < 0)
			{
				Debug.LogError(string.Format("Current resolution ({0}x{1}) not in resolutions", Screen.width, Screen.height));
				num = 0;
			}
			this.resolutionDropdown.dropdown.SetValueWithoutNotify(num);
			this._frameSettings = ResolutionHelper.GetAvailableFrameSettings();
			this.frameRateDropdown.dropdown.ClearOptions();
			this.frameRateDropdown.dropdown.AddOptions(Enumerable.ToList<string>(Enumerable.Select<FrameSetting, string>(this._frameSettings, new Func<FrameSetting, string>(ResolutionHelper.LocalizeTextForFrameSetting))));
			FrameSetting frameSetting = Enumerable.FirstOrDefault<FrameSetting>(this._frameSettings, (FrameSetting set) => set.Equals(new FrameSetting(QualitySettings.vSyncCount, Application.targetFrameRate)));
			int num2 = this._frameSettings.IndexOf(frameSetting);
			this.frameRateDropdown.dropdown.SetValueWithoutNotify(num2);
			this.fullScreenSwitch.SetValueWithoutNotifier(Screen.fullScreen, true);
			this.cursorSwitch.SetValueWithoutNotifier(GameMaster.UseLbolCursor, true);
			this.cursorSwitch.AddListener(new UnityAction<bool>(this.UI_Cursor));
		}
		private void ResetAudioSettings()
		{
			this.masterVolumeSlider.SetValueWithoutNotify(AudioManager.MasterVolume);
			this.masterVolumeText.text = (AudioManager.MasterVolume * 100f).RoundToInt().ToString() + "%";
			this.bgmVolumeSlider.SetValueWithoutNotify(AudioManager.BgmVolume);
			this.bgmVolumeText.text = (AudioManager.BgmVolume * 100f).RoundToInt().ToString() + "%";
			this.uiVolumeSlider.SetValueWithoutNotify(AudioManager.UiVolume);
			this.uiVolumeText.text = (AudioManager.UiVolume * 100f).RoundToInt().ToString() + "%";
			this.sfxVolumeSlider.SetValueWithoutNotify(AudioManager.SfxVolume);
			this.sfxVolumeText.text = (AudioManager.SfxVolume * 100f).RoundToInt().ToString() + "%";
			this.backgroundMuteToggle.SetValueWithoutNotifier(AudioManager.IsBackgroundMute, true);
		}
		private void ResetPreferenceSettings()
		{
			this.turboModeSwitch.SetValueWithoutNotifier(GameMaster.IsTurboMode, true);
			this.turboModeSwitch.AddListener(new UnityAction<bool>(this.UI_IsTurboMode));
			this.keywordSwitch.SetValueWithoutNotifier(GameMaster.ShowVerboseKeywords, true);
			this.keywordSwitch.AddListener(new UnityAction<bool>(this.UI_ShowVerboseKeywords));
			this.showIllustratorSwitch.SetValueWithoutNotifier(GameMaster.ShowIllustrator, true);
			this.showIllustratorSwitch.AddListener(new UnityAction<bool>(this.UI_ShowIllustrator));
			this.tooltipSizeSwitch.SetValueWithoutNotifier(GameMaster.IsLargeTooltips, true);
			this.tooltipSizeSwitch.AddListener(new UnityAction<bool>(this.UI_IsLargeTooltips));
			this.tooltipWideSwitch.SetValueWithoutNotifier(GameMaster.PreferWideTooltips, true);
			this.tooltipWideSwitch.AddListener(new UnityAction<bool>(this.UI_PreferWideTooltips));
			this.rightClickCancelSwitch.SetValueWithoutNotifier(GameMaster.RightClickCancel, true);
			this.rightClickCancelSwitch.AddListener(new UnityAction<bool>(this.UI_RightClickCancel));
			this.loopOrderSizeSwitch.SetValueWithoutNotifier(GameMaster.IsLoopOrder, true);
			this.loopOrderSizeSwitch.AddListener(new UnityAction<bool>(this.UI_IsLoopOrder));
			this.animatingEnvironmentEnabledSwitch.SetValueWithoutNotifier(GameMaster.IsAnimatingEnvironmentEnabled, true);
			this.animatingEnvironmentEnabledSwitch.AddListener(new UnityAction<bool>(this.UI_IsAnimatingEnvironmentEnabled));
			this.singleEnemyAutoSelectSwitch.SetValueWithoutNotifier(GameMaster.SingleEnemyAutoSelect, true);
			this.singleEnemyAutoSelectSwitch.AddListener(new UnityAction<bool>(this.UI_SingleEnemyAutoSelect));
			this.showXCostWarningSwitch.SetValueWithoutNotifier(GameMaster.ShowXCostEmptyUseWarning, true);
			this.showXCostWarningSwitch.AddListener(new UnityAction<bool>(this.UI_ShowXCostEmptyUseWarning));
			this.showShortcutSwitch.SetValueWithoutNotifier(GameMaster.ShowShortcut, true);
			this.showShortcutSwitch.AddListener(new UnityAction<bool>(this.UI_ShowShortcut));
			this.showCardOrderSwitch.SetValueWithoutNotifier(GameMaster.ShowCardOrder, true);
			this.showCardOrderSwitch.AddListener(new UnityAction<bool>(this.UI_ShowCardOrder));
			this.showReloadSwitch.SetValueWithoutNotifier(GameMaster.ShowReload, true);
			this.showReloadSwitch.AddListener(new UnityAction<bool>(this.UI_ShowReload));
			this.shakeSwitch.SetValueWithoutNotifier(GameMaster.Shake, true);
			this.shakeSwitch.AddListener(new UnityAction<bool>(this.UI_Shake));
			this.costMoreLeftSwitch.SetValueWithoutNotifier(GameMaster.CostMoreLeft, true);
			this.costMoreLeftSwitch.AddListener(new UnityAction<bool>(this.UI_CostMoreLeft));
			int num = (int)GameMaster.HintLevel;
			if (num >= 0 && num < 3)
			{
				this.hintLevelDropdown.SetValueWithoutNotify(num);
			}
			else
			{
				Debug.Log(string.Format("Invalid hint level: {0}", GameMaster.HintLevel));
			}
			num = (int)GameMaster.QuickPlayLevel;
			if (num >= 0 && num < 3)
			{
				this.quickPlayLevelDropdown.SetValueWithoutNotify(num);
				return;
			}
			Debug.Log(string.Format("Invalid quick play level: {0}", GameMaster.QuickPlayLevel));
		}
		private void ResetKeyBindings()
		{
			this.keyMappingParent.DestroyChildren();
			float num = 0f;
			using (IEnumerator<ActionMapping> enumerator = UiManager.EnumerateRebindableActions().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ActionMapping actionMapping = enumerator.Current;
					InputAction action = actionMapping.InputAction;
					ActionMappingRow row = Object.Instantiate<ActionMappingRow>(this.actionMappingRowTemplate, this.keyMappingParent);
					row.gameObject.SetActive(true);
					row.SetMapping(actionMapping);
					Action<InputActionRebindingExtensions.RebindingOperation> <>9__1;
					row.ButtonClicked += delegate
					{
						int keyboardIndex = action.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
						if (keyboardIndex < 0)
						{
							Debug.LogError(string.Format("Cannot rebind keyboard for {0}", action));
							return;
						}
						this.rebindRoot.gameObject.SetActive(true);
						this.rebindMessage.text = "Setting.Actions.RebindMessage".LocalizeFormat(new object[] { ("Setting.Actions." + actionMapping.Id).Localize(true) });
						action.Disable();
						InputActionRebindingExtensions.RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(keyboardIndex).WithCancelingThrough("<Keyboard>/escape").WithControlsExcluding("Mouse");
						Action<InputActionRebindingExtensions.RebindingOperation> action2;
						if ((action2 = <>9__1) == null)
						{
							action2 = (<>9__1 = delegate(InputActionRebindingExtensions.RebindingOperation _)
							{
								this.rebindRoot.gameObject.SetActive(false);
								action.Enable();
							});
						}
						rebindingOperation.OnCancel(action2).OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation _)
						{
							this.rebindRoot.gameObject.SetActive(false);
							row.Refresh();
							action.Enable();
							InputBinding inputBinding = action.bindings[keyboardIndex];
							GameMaster.SaveKeyboardBindings(actionMapping.Id, inputBinding.overridePath);
						}).Start();
					};
					RectTransform rectTransform = (RectTransform)row.transform;
					rectTransform.anchoredPosition = new Vector2(0f, -num);
					num += rectTransform.sizeDelta.y;
				}
			}
			this.keyMappingParent.sizeDelta = new Vector2(this.keyMappingParent.sizeDelta.x, num);
			this.enableKeyboardSwitch.SetValueWithoutNotifier(GameMaster.EnableKeyboard, true);
			this.enableGamepadSwitch.SetValueWithoutNotifier(InputDeviceManager.GamepadEnabled, true);
			this.keyBindingsGroup.interactable = GameMaster.EnableKeyboard;
		}
		private void ResetLocaleDropdown()
		{
			this.localeDropdown.dropdown.ClearOptions();
			this.localeDropdown.dropdown.AddOptions(Enumerable.ToList<string>(Enumerable.Select<LocaleSettingItem, string>(LocaleSettingItem.All, (LocaleSettingItem l) => l.Name)));
			this.localeDropdown.dropdown.SetValueWithoutNotify(LocaleSettingItem.All.FindIndexOf((LocaleSettingItem l) => l.Locale == Localization.CurrentLocale));
		}
		protected override void OnShowing(SettingsPanelType payload)
		{
			this.ResetMainSettings();
			this.ResetAudioSettings();
			this.ResetPreferenceSettings();
			this.ResetKeyBindings();
			this.ResetLocaleDropdown();
			if (payload != SettingsPanelType.MainMenu)
			{
				if (payload != SettingsPanelType.InGame)
				{
					throw new ArgumentOutOfRangeException("payload", payload, null);
				}
				this.mainMenuGroup.SetActive(false);
				this.inGameGroup.SetActive(true);
				this.animatingEnvironmentEnabledSwitch.IsLocked = true;
				this.animatingEnvironmentEnabledParent.alpha = 0.3f;
				this._animatingEnvironmentTooltipSource.SetWithGeneralKey("Setting.AnimatingEnvironment", "Setting.AnimatingEnvironmentDescriptionLocked");
			}
			else
			{
				this.mainMenuGroup.SetActive(true);
				this.inGameGroup.SetActive(false);
				this.animatingEnvironmentEnabledSwitch.IsLocked = false;
				this.animatingEnvironmentEnabledParent.alpha = 1f;
				this._animatingEnvironmentTooltipSource.SetWithGeneralKey("Setting.AnimatingEnvironment", "Setting.AnimatingEnvironmentDescription");
			}
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}
		protected override void OnShown()
		{
			base.OnShown();
			this.backgroundHideButton.interactable = true;
		}
		protected override void OnHiding()
		{
			this.backgroundHideButton.interactable = false;
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		void IInputActionHandler.OnCancel()
		{
			if (!this.rebindRoot.activeSelf)
			{
				base.Hide();
			}
		}
		public void UI_SetResolution(int index)
		{
			int num;
			int num2;
			this._resolutions[index].Deconstruct(out num, out num2);
			int num3 = num;
			int num4 = num2;
			Screen.SetResolution(num3, num4, Screen.fullScreenMode);
		}
		public void UI_SetFrameRate(int index)
		{
			FrameSetting frameSetting = this._frameSettings[index];
			QualitySettings.vSyncCount = frameSetting.VsyncCount;
			Application.targetFrameRate = frameSetting.FrameRateForSetting;
			PlayerPrefs.SetInt("VsyncCount", frameSetting.VsyncCount);
			PlayerPrefs.SetInt("TargetFrameRate", frameSetting.FrameRateForSetting);
		}
		public void UI_SetFullScreenMode()
		{
			Screen.fullScreen = this.fullScreenSwitch.IsOn;
		}
		public void UI_SetLocale(int index)
		{
			Locale locale = LocaleSettingItem.All[index].Locale;
			if (locale != Localization.CurrentLocale)
			{
				base.StartCoroutine(this.CoChangeLocale(locale));
			}
		}
		private IEnumerator CoChangeLocale(Locale locale)
		{
			yield return UiManager.ShowLoading(0.1f).ToCoroutine(null);
			yield return UniTask.ToCoroutine(() => L10nManager.SetLocaleAsync(locale));
			yield return UiManager.HideLoading(0.1f).ToCoroutine(null);
			yield break;
		}
		public void UI_SetMasterVolume(float value)
		{
			this.InternalSetMasterVolume(value);
		}
		private void InternalSetMasterVolume(float value)
		{
			AudioManager.MasterVolume = value;
			float masterVolume = AudioManager.MasterVolume;
			this.masterVolumeText.text = (masterVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("MasterVolume", masterVolume);
		}
		public void UI_SetBgmVolume(float value)
		{
			this.InternalSetBgmVolume(value);
		}
		private void InternalSetBgmVolume(float value)
		{
			AudioManager.BgmVolume = value;
			float bgmVolume = AudioManager.BgmVolume;
			this.bgmVolumeText.text = (bgmVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("BgmVolume", bgmVolume);
		}
		public void UI_SetUiVolume(float value)
		{
			this.InternalSetUiVolume(value);
		}
		private void InternalSetUiVolume(float value)
		{
			AudioManager.UiVolume = value;
			float uiVolume = AudioManager.UiVolume;
			this.uiVolumeText.text = (uiVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("UiVolume", uiVolume);
		}
		public void UI_OnPointerDownPlaySampleUi()
		{
			if (!this.uiPlaying)
			{
				this.uiPlaying = true;
				this.uiTimer = 0.5f;
			}
		}
		public void UI_OnPointerUpPlaySampleUi()
		{
			if (this.uiPlaying)
			{
				this.uiPlaying = false;
			}
		}
		public void UI_OnPointerDownPlaySampleSfx()
		{
			if (!this.sfxPlaying)
			{
				this.sfxPlaying = true;
				this.sfxTimer = 0.5f;
			}
		}
		public void UI_OnPointerUpPlaySampleSfx()
		{
			if (this.sfxPlaying)
			{
				this.sfxPlaying = false;
			}
		}
		public void UI_SetSfxVolume(float value)
		{
			this.InternalSetSfxVolume(value);
		}
		private void InternalSetSfxVolume(float value)
		{
			AudioManager.SfxVolume = value;
			float sfxVolume = AudioManager.SfxVolume;
			this.sfxVolumeText.text = (sfxVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
		}
		public void UI_RecommendVolume()
		{
			this.masterVolumeSlider.value = 0.5f;
			this.InternalSetMasterVolume(0.5f);
			this.bgmVolumeSlider.value = 0.5f;
			this.InternalSetBgmVolume(0.5f);
			this.uiVolumeSlider.value = 1f;
			this.InternalSetUiVolume(1f);
			this.sfxVolumeSlider.value = 1f;
			this.InternalSetSfxVolume(1f);
		}
		public void UI_SetBackgroundMute()
		{
			AudioManager.IsBackgroundMute = this.backgroundMuteToggle.IsOn;
		}
		public void UI_IsTurboMode(bool isOn)
		{
			GameMaster.IsTurboMode = isOn;
		}
		public void UI_ShowVerboseKeywords(bool isOn)
		{
			GameMaster.ShowVerboseKeywords = isOn;
		}
		public void UI_ShowIllustrator(bool isOn)
		{
			GameMaster.ShowIllustrator = isOn;
		}
		public void UI_IsLargeTooltips(bool isOn)
		{
			GameMaster.IsLargeTooltips = isOn;
			UiManager.GetPanel<TooltipsLayer>().Refresh();
			IntentionWidget[] array = Object.FindObjectsOfType<IntentionWidget>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].IsLargeTextSize = isOn;
			}
			UnitInfoWidget[] array2 = Object.FindObjectsOfType<UnitInfoWidget>();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].UpdateIntentions();
			}
		}
		public void UI_PreferWideTooltips(bool isOn)
		{
			GameMaster.PreferWideTooltips = isOn;
			UiManager.GetPanel<TooltipsLayer>().Refresh();
		}
		public void UI_RightClickCancel(bool isOn)
		{
			GameMaster.RightClickCancel = isOn;
		}
		public void UI_IsLoopOrder(bool isOn)
		{
			GameMaster.IsLoopOrder = isOn;
			CardWidget[] array = Object.FindObjectsOfType<CardWidget>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshBgTextureForLoopOrder();
			}
		}
		public void UI_IsAnimatingEnvironmentEnabled(bool isOn)
		{
			GameMaster.IsAnimatingEnvironmentEnabled = isOn;
		}
		public void UI_SingleEnemyAutoSelect(bool isOn)
		{
			GameMaster.SingleEnemyAutoSelect = isOn;
		}
		public void UI_ShowXCostEmptyUseWarning(bool isOn)
		{
			GameMaster.ShowXCostEmptyUseWarning = isOn;
		}
		public void UI_ShowShortcut(bool isOn)
		{
			GameMaster.ShowShortcut = isOn;
			HandCard[] array = Object.FindObjectsOfType<HandCard>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ShowShortcut = isOn;
			}
		}
		public void UI_ShowCardOrder(bool isOn)
		{
			GameMaster.ShowCardOrder = isOn;
		}
		public void UI_ShowReload(bool isOn)
		{
			GameMaster.ShowReload = isOn;
		}
		public void UI_Shake(bool isOn)
		{
			GameMaster.Shake = isOn;
		}
		public void UI_CostMoreLeft(bool isOn)
		{
			GameMaster.CostMoreLeft = isOn;
			foreach (HandCard handCard in Object.FindObjectsOfType<HandCard>())
			{
				handCard.CostMoreLeft = handCard.ShouldShowLeftCost;
			}
		}
		public void UI_Cursor(bool isOn)
		{
			GameMaster.SetCursor(isOn);
		}
		public void UI_ChangeHintLevel(int index)
		{
			GameMaster.HintLevel = (HintLevel)index;
		}
		public void UI_ChangeQuickPlayLevel(int index)
		{
			GameMaster.QuickPlayLevel = (QuickPlayLevel)index;
		}
		public void UI_ResetHint()
		{
			UiDialog<MessageContent> dialog = UiManager.GetDialog<MessageDialog>();
			MessageContent messageContent = new MessageContent();
			messageContent.TextKey = "ResetHintWarning";
			messageContent.Buttons = DialogButtons.ConfirmCancel;
			messageContent.OnConfirm = delegate
			{
				GameMaster.ResetHints();
			};
			dialog.Show(messageContent);
		}
		public void UI_ResetDefaultKeyBindingds()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "ResetKeyBindingsWarning",
				Buttons = DialogButtons.ConfirmCancel,
				Icon = MessageIcon.Warning,
				OnConfirm = delegate
				{
					GameMaster.ClearKeyboardBindings();
					this.ResetKeyBindings();
				}
			});
		}
		public void UI_Hide()
		{
			base.Hide();
		}
		public void UI_RestartBattle()
		{
			try
			{
				GameMaster.RequestReenterStation();
				base.Hide();
			}
			catch (Exception ex)
			{
				UiManager.GetDialog<MessageDialog>().Show(new MessageContent
				{
					TextKey = "CannotReenterStation",
					Icon = MessageIcon.Error,
					Buttons = DialogButtons.Confirm
				});
				Debug.LogError(ex);
			}
		}
		public void UI_LeaveGameRun()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "ReturnToMainMenu",
				SubTextKey = "UnsavedWarning",
				Buttons = DialogButtons.ConfirmCancel,
				OnConfirm = new Action(GameMaster.LeaveGameRun)
			});
		}
		public void UI_Quit()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "QuitGame",
				SubTextKey = "UnsavedWarning",
				Buttons = DialogButtons.ConfirmCancel,
				OnConfirm = new Action(GameMaster.QuitGame)
			});
		}
		private void Update()
		{
			if (this.sfxPlaying)
			{
				this.sfxTimer -= Time.unscaledDeltaTime;
				if (this.sfxTimer < 0f)
				{
					this.sfxTimer = 0.5f;
					AudioManager.PlaySfx("SakuyaKnifeLaunch", -1f);
				}
			}
			if (this.uiPlaying)
			{
				this.uiTimer -= Time.unscaledDeltaTime;
				if (this.uiTimer < 0f)
				{
					this.uiTimer = 0.5f;
					AudioManager.PlayUi("ButtonClick0", false);
				}
			}
		}
		private void Awake()
		{
			this.localeGroup.SetActive(true);
			this.enableKeyboardParent.SetActive(false);
			this.enableGamepadParent.SetActive(false);
			for (int i = 0; i < this.tabs.Count; i++)
			{
				CanvasGroup canvasGroup = this.tabs[i];
				canvasGroup.gameObject.SetActive(i == 0);
				canvasGroup.alpha = (float)((i == 0) ? 1 : 0);
			}
			this.actionMappingRowTemplate.gameObject.SetActive(false);
			SimpleTooltipSource.CreateWithGeneralKeyAndArgs(this.turboModeParent, "Setting.TurboMode", "Setting.TurboModeDescription", new object[] { GlobalConfig.TurboModeTimeScaleString }).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.keywordParent, "Setting.Keyword", "Setting.KeywordDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.showIllustratorParent, "Setting.Illustrator", "Setting.IllustratorDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.tooltipSizeParent, "Setting.TooltipSize", "Setting.TooltipSizeDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.tooltipWideParent, "Setting.TooltipWide", "Setting.TooltipWideDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.rightClickCancelParent, "Setting.RightClickCancel", "Setting.RightClickCancelDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.loopOrderSizeParent, "Setting.LoopOrder", "Setting.LoopOrderDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.costMoreLeftParent, "Setting.CostMoreLeft", "Setting.CostMoreLeftDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._animatingEnvironmentTooltipSource = SimpleTooltipSource.CreateWithGeneralKey(this.animatingEnvironmentEnabledParent.gameObject, "Setting.AnimatingEnvironment", "Setting.AnimatingEnvironmentDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.singleEnemyAutoSelectParent, "Setting.QuickPlay", "Setting.QuickPlayDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.quickPlayLevelParent, "Setting.SingleEnemyAutoSelect", "Setting.SingleEnemyAutoSelectDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.showXCostWarningParent, "Setting.XCostWarning", "Setting.XCostWarningDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.showShortcutParent, "Setting.ShowShortcut", "Setting.ShowShortcutDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.showCardOrderParent, "Setting.ShowCardOrder", "Setting.ShowCardOrderDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.showReloadParent, "Setting.ShowReload", "Setting.ShowReloadDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			SimpleTooltipSource.CreateWithGeneralKey(this.shakeParent, "Setting.Shake", "Setting.ShakeDescription").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
		public override void OnLocaleChanged()
		{
			TMP_Dropdown tmp_Dropdown = this.hintLevelDropdown;
			List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
			list.Add(new TMP_Dropdown.OptionData("Setting.Detailed".Localize(true)));
			list.Add(new TMP_Dropdown.OptionData("Setting.Brief".Localize(true)));
			list.Add(new TMP_Dropdown.OptionData("Setting.DontShow".Localize(true)));
			tmp_Dropdown.options = list;
			this.hintLevelDropdown.RefreshShownValue();
			TMP_Dropdown tmp_Dropdown2 = this.quickPlayLevelDropdown;
			List<TMP_Dropdown.OptionData> list2 = new List<TMP_Dropdown.OptionData>();
			list2.Add(new TMP_Dropdown.OptionData("Setting.Default".Localize(true)));
			list2.Add(new TMP_Dropdown.OptionData("Setting.DoubleClick".Localize(true)));
			list2.Add(new TMP_Dropdown.OptionData("Setting.SingleClick".Localize(true)));
			tmp_Dropdown2.options = list2;
			this.quickPlayLevelDropdown.RefreshShownValue();
			if (this._frameSettings != null && Enumerable.Any<FrameSetting>(this._frameSettings))
			{
				foreach (ValueTuple<int, string> valueTuple in Enumerable.ToList<string>(Enumerable.Select<FrameSetting, string>(this._frameSettings, new Func<FrameSetting, string>(ResolutionHelper.LocalizeTextForFrameSetting))).WithIndices<string>())
				{
					int item = valueTuple.Item1;
					string item2 = valueTuple.Item2;
					if (this.frameRateDropdown.dropdown.options.TryGetValue(item) != null)
					{
						this.frameRateDropdown.dropdown.options[item] = new TMP_Dropdown.OptionData(item2);
					}
				}
				this.frameRateDropdown.dropdown.RefreshShownValue();
			}
			if (Singleton<GameMaster>.Instance.CurrentProfile != null)
			{
				int num = (int)GameMaster.HintLevel;
				if (num >= 0 && num < 3)
				{
					this.hintLevelDropdown.SetValueWithoutNotify(num);
				}
				else
				{
					Debug.Log(string.Format("Invalid hint level: {0}", GameMaster.HintLevel));
				}
				num = (int)GameMaster.QuickPlayLevel;
				if (num >= 0 && num < 3)
				{
					this.quickPlayLevelDropdown.SetValueWithoutNotify(num);
				}
				else
				{
					Debug.Log(string.Format("Invalid quick play level: {0}", GameMaster.QuickPlayLevel));
				}
			}
			this.ResetLocaleDropdown();
		}
		public void UI_OnTabToggleChanged(Toggle item)
		{
			string name = item.name;
			int num;
			if (!(name == "Main"))
			{
				if (!(name == "Preference"))
				{
					if (!(name == "Key"))
					{
						num = -1;
					}
					else
					{
						num = 2;
					}
				}
				else
				{
					num = 1;
				}
			}
			else
			{
				num = 0;
			}
			int num2 = num;
			this.SwitchToTab(num2);
		}
		private void SwitchToTab(int index)
		{
			if (index == this._tabIndex)
			{
				return;
			}
			AudioManager.Button(0);
			AudioManager.PlayUi("Slide", false);
			float num = 1600f;
			if (index > this._tabIndex)
			{
				num = -num;
			}
			CanvasGroup old = this.tabs[this._tabIndex];
			CanvasGroup canvasGroup = this.tabs[index];
			this._tabIndex = index;
			old.DOKill(true);
			canvasGroup.DOKill(true);
			old.interactable = false;
			old.transform.DOLocalMoveX(num, 0.3f, false).From(0f, true, false).SetEase(Ease.OutCubic)
				.SetUpdate(true);
			old.DOFade(0f, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
			canvasGroup.gameObject.SetActive(true);
			canvasGroup.transform.DOLocalMoveX(0f, 0.3f, false).From(-num, true, false).SetEase(Ease.OutCubic)
				.SetUpdate(true);
			canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true)
				.OnComplete(delegate
				{
					old.gameObject.SetActive(false);
					old.interactable = true;
					GamepadNavigationManager.RefreshSelection();
				});
		}
		public void SetReenterStationInteractable(bool interactable)
		{
			this.reenterStationButton.interactable = interactable;
		}
		public void OnGamepadPairButtonActive(float valueStep)
		{
			int num = (int)((float)this._tabIndex + valueStep);
			if (num < 0)
			{
				return;
			}
			if (num >= this.tabs.Count)
			{
				return;
			}
			this.tabGroup.GetComponentsInChildren<Toggle>()[num].isOn = true;
		}
		[Header("综合")]
		[SerializeField]
		private List<CanvasGroup> tabs;
		[SerializeField]
		private ToggleGroup tabGroup;
		[SerializeField]
		private GameObject mainMenuGroup;
		[SerializeField]
		private GameObject inGameGroup;
		[SerializeField]
		private Button reenterStationButton;
		[SerializeField]
		private Button backgroundHideButton;
		[Header("主设置")]
		[SerializeField]
		private DropdownWidget resolutionDropdown;
		[SerializeField]
		private DropdownWidget frameRateDropdown;
		[SerializeField]
		private SwitchWidget fullScreenSwitch;
		[SerializeField]
		private CanvasGroup animatingEnvironmentEnabledParent;
		[SerializeField]
		private SwitchWidget animatingEnvironmentEnabledSwitch;
		[SerializeField]
		private GameObject localeGroup;
		[SerializeField]
		private DropdownWidget localeDropdown;
		[Header("音频")]
		[SerializeField]
		private Slider masterVolumeSlider;
		[SerializeField]
		private TextMeshProUGUI masterVolumeText;
		[SerializeField]
		private Slider bgmVolumeSlider;
		[SerializeField]
		private TextMeshProUGUI bgmVolumeText;
		[SerializeField]
		private Slider uiVolumeSlider;
		[SerializeField]
		private TextMeshProUGUI uiVolumeText;
		[SerializeField]
		private Slider sfxVolumeSlider;
		[SerializeField]
		private TextMeshProUGUI sfxVolumeText;
		[SerializeField]
		private SwitchWidget backgroundMuteToggle;
		[Header("偏好")]
		[SerializeField]
		private GameObject turboModeParent;
		[SerializeField]
		private SwitchWidget turboModeSwitch;
		[SerializeField]
		private GameObject keywordParent;
		[SerializeField]
		private SwitchWidget keywordSwitch;
		[SerializeField]
		private GameObject showIllustratorParent;
		[SerializeField]
		private SwitchWidget showIllustratorSwitch;
		[SerializeField]
		private GameObject tooltipSizeParent;
		[SerializeField]
		private SwitchWidget tooltipSizeSwitch;
		[SerializeField]
		private GameObject tooltipWideParent;
		[SerializeField]
		private SwitchWidget tooltipWideSwitch;
		[SerializeField]
		private GameObject rightClickCancelParent;
		[SerializeField]
		private SwitchWidget rightClickCancelSwitch;
		[SerializeField]
		private GameObject loopOrderSizeParent;
		[SerializeField]
		private SwitchWidget loopOrderSizeSwitch;
		[SerializeField]
		private GameObject singleEnemyAutoSelectParent;
		[SerializeField]
		private SwitchWidget singleEnemyAutoSelectSwitch;
		[SerializeField]
		private GameObject quickPlayLevelParent;
		[SerializeField]
		private TMP_Dropdown quickPlayLevelDropdown;
		[SerializeField]
		private GameObject showXCostWarningParent;
		[SerializeField]
		private SwitchWidget showXCostWarningSwitch;
		[SerializeField]
		private GameObject showShortcutParent;
		[SerializeField]
		private SwitchWidget showShortcutSwitch;
		[SerializeField]
		private GameObject showCardOrderParent;
		[SerializeField]
		private SwitchWidget showCardOrderSwitch;
		[SerializeField]
		private GameObject showReloadParent;
		[SerializeField]
		private SwitchWidget showReloadSwitch;
		[SerializeField]
		private GameObject shakeParent;
		[SerializeField]
		private SwitchWidget shakeSwitch;
		[SerializeField]
		private GameObject costMoreLeftParent;
		[SerializeField]
		private SwitchWidget costMoreLeftSwitch;
		[SerializeField]
		private GameObject cursorParent;
		[SerializeField]
		private SwitchWidget cursorSwitch;
		[SerializeField]
		private GameObject hintLevelParent;
		[SerializeField]
		private TMP_Dropdown hintLevelDropdown;
		[SerializeField]
		private GameObject resetHintParent;
		[Header("按键")]
		[SerializeField]
		private RectTransform keyMappingParent;
		[SerializeField]
		private ActionMappingRow actionMappingRowTemplate;
		[SerializeField]
		private GameObject rebindRoot;
		[SerializeField]
		private TextMeshProUGUI rebindMessage;
		[SerializeField]
		private GameObject enableKeyboardParent;
		[SerializeField]
		private SwitchWidget enableKeyboardSwitch;
		[SerializeField]
		private GameObject enableGamepadParent;
		[SerializeField]
		private SwitchWidget enableGamepadSwitch;
		[SerializeField]
		private CanvasGroup keyBindingsGroup;
		private CanvasGroup _canvasGroup;
		private IReadOnlyList<FrameSetting> _frameSettings;
		private IReadOnlyList<Vector2Int> _resolutions;
		private SimpleTooltipSource _animatingEnvironmentTooltipSource;
		private const float RecommendMaster = 50f;
		private const float RecommendMusic = 50f;
		private const float RecommendUi = 100f;
		private const float RecommendSfx = 100f;
		private bool sfxPlaying;
		private float sfxTimer;
		private bool uiPlaying;
		private float uiTimer;
		private int _tabIndex;
		private const float TweenTime = 0.3f;
		private const float TweenMoveX = 1600f;
	}
}
