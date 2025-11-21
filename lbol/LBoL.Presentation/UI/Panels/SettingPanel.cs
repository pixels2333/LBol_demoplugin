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
	// Token: 0x020000B4 RID: 180
	public class SettingPanel : UiPanel<SettingsPanelType>, IInputActionHandler
	{
		// Token: 0x17000198 RID: 408
		// (get) Token: 0x060009F8 RID: 2552 RVA: 0x00032569 File Offset: 0x00030769
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x0003256C File Offset: 0x0003076C
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

		// Token: 0x060009FA RID: 2554 RVA: 0x000326F4 File Offset: 0x000308F4
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

		// Token: 0x060009FB RID: 2555 RVA: 0x00032808 File Offset: 0x00030A08
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

		// Token: 0x060009FC RID: 2556 RVA: 0x00032AD8 File Offset: 0x00030CD8
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

		// Token: 0x060009FD RID: 2557 RVA: 0x00032C2C File Offset: 0x00030E2C
		private void ResetLocaleDropdown()
		{
			this.localeDropdown.dropdown.ClearOptions();
			this.localeDropdown.dropdown.AddOptions(Enumerable.ToList<string>(Enumerable.Select<LocaleSettingItem, string>(LocaleSettingItem.All, (LocaleSettingItem l) => l.Name)));
			this.localeDropdown.dropdown.SetValueWithoutNotify(LocaleSettingItem.All.FindIndexOf((LocaleSettingItem l) => l.Locale == Localization.CurrentLocale));
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x00032CC0 File Offset: 0x00030EC0
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

		// Token: 0x060009FF RID: 2559 RVA: 0x00032DB1 File Offset: 0x00030FB1
		protected override void OnShown()
		{
			base.OnShown();
			this.backgroundHideButton.interactable = true;
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x00032DC5 File Offset: 0x00030FC5
		protected override void OnHiding()
		{
			this.backgroundHideButton.interactable = false;
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x00032DE5 File Offset: 0x00030FE5
		void IInputActionHandler.OnCancel()
		{
			if (!this.rebindRoot.activeSelf)
			{
				base.Hide();
			}
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x00032DFC File Offset: 0x00030FFC
		public void UI_SetResolution(int index)
		{
			int num;
			int num2;
			this._resolutions[index].Deconstruct(out num, out num2);
			int num3 = num;
			int num4 = num2;
			Screen.SetResolution(num3, num4, Screen.fullScreenMode);
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x00032E2C File Offset: 0x0003102C
		public void UI_SetFrameRate(int index)
		{
			FrameSetting frameSetting = this._frameSettings[index];
			QualitySettings.vSyncCount = frameSetting.VsyncCount;
			Application.targetFrameRate = frameSetting.FrameRateForSetting;
			PlayerPrefs.SetInt("VsyncCount", frameSetting.VsyncCount);
			PlayerPrefs.SetInt("TargetFrameRate", frameSetting.FrameRateForSetting);
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x00032E7E File Offset: 0x0003107E
		public void UI_SetFullScreenMode()
		{
			Screen.fullScreen = this.fullScreenSwitch.IsOn;
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x00032E90 File Offset: 0x00031090
		public void UI_SetLocale(int index)
		{
			Locale locale = LocaleSettingItem.All[index].Locale;
			if (locale != Localization.CurrentLocale)
			{
				base.StartCoroutine(this.CoChangeLocale(locale));
			}
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x00032EC4 File Offset: 0x000310C4
		private IEnumerator CoChangeLocale(Locale locale)
		{
			yield return UiManager.ShowLoading(0.1f).ToCoroutine(null);
			yield return UniTask.ToCoroutine(() => L10nManager.SetLocaleAsync(locale));
			yield return UiManager.HideLoading(0.1f).ToCoroutine(null);
			yield break;
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x00032ED3 File Offset: 0x000310D3
		public void UI_SetMasterVolume(float value)
		{
			this.InternalSetMasterVolume(value);
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x00032EDC File Offset: 0x000310DC
		private void InternalSetMasterVolume(float value)
		{
			AudioManager.MasterVolume = value;
			float masterVolume = AudioManager.MasterVolume;
			this.masterVolumeText.text = (masterVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("MasterVolume", masterVolume);
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x00032F29 File Offset: 0x00031129
		public void UI_SetBgmVolume(float value)
		{
			this.InternalSetBgmVolume(value);
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x00032F34 File Offset: 0x00031134
		private void InternalSetBgmVolume(float value)
		{
			AudioManager.BgmVolume = value;
			float bgmVolume = AudioManager.BgmVolume;
			this.bgmVolumeText.text = (bgmVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("BgmVolume", bgmVolume);
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x00032F81 File Offset: 0x00031181
		public void UI_SetUiVolume(float value)
		{
			this.InternalSetUiVolume(value);
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x00032F8C File Offset: 0x0003118C
		private void InternalSetUiVolume(float value)
		{
			AudioManager.UiVolume = value;
			float uiVolume = AudioManager.UiVolume;
			this.uiVolumeText.text = (uiVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("UiVolume", uiVolume);
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x00032FD9 File Offset: 0x000311D9
		public void UI_OnPointerDownPlaySampleUi()
		{
			if (!this.uiPlaying)
			{
				this.uiPlaying = true;
				this.uiTimer = 0.5f;
			}
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x00032FF5 File Offset: 0x000311F5
		public void UI_OnPointerUpPlaySampleUi()
		{
			if (this.uiPlaying)
			{
				this.uiPlaying = false;
			}
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x00033006 File Offset: 0x00031206
		public void UI_OnPointerDownPlaySampleSfx()
		{
			if (!this.sfxPlaying)
			{
				this.sfxPlaying = true;
				this.sfxTimer = 0.5f;
			}
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x00033022 File Offset: 0x00031222
		public void UI_OnPointerUpPlaySampleSfx()
		{
			if (this.sfxPlaying)
			{
				this.sfxPlaying = false;
			}
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x00033033 File Offset: 0x00031233
		public void UI_SetSfxVolume(float value)
		{
			this.InternalSetSfxVolume(value);
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x0003303C File Offset: 0x0003123C
		private void InternalSetSfxVolume(float value)
		{
			AudioManager.SfxVolume = value;
			float sfxVolume = AudioManager.SfxVolume;
			this.sfxVolumeText.text = (sfxVolume * 100f).RoundToInt().ToString() + "%";
			PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x0003308C File Offset: 0x0003128C
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

		// Token: 0x06000A14 RID: 2580 RVA: 0x00033105 File Offset: 0x00031305
		public void UI_SetBackgroundMute()
		{
			AudioManager.IsBackgroundMute = this.backgroundMuteToggle.IsOn;
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x00033117 File Offset: 0x00031317
		public void UI_IsTurboMode(bool isOn)
		{
			GameMaster.IsTurboMode = isOn;
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x0003311F File Offset: 0x0003131F
		public void UI_ShowVerboseKeywords(bool isOn)
		{
			GameMaster.ShowVerboseKeywords = isOn;
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x00033127 File Offset: 0x00031327
		public void UI_ShowIllustrator(bool isOn)
		{
			GameMaster.ShowIllustrator = isOn;
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x00033130 File Offset: 0x00031330
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

		// Token: 0x06000A19 RID: 2585 RVA: 0x00033186 File Offset: 0x00031386
		public void UI_PreferWideTooltips(bool isOn)
		{
			GameMaster.PreferWideTooltips = isOn;
			UiManager.GetPanel<TooltipsLayer>().Refresh();
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x00033198 File Offset: 0x00031398
		public void UI_RightClickCancel(bool isOn)
		{
			GameMaster.RightClickCancel = isOn;
		}

		// Token: 0x06000A1B RID: 2587 RVA: 0x000331A0 File Offset: 0x000313A0
		public void UI_IsLoopOrder(bool isOn)
		{
			GameMaster.IsLoopOrder = isOn;
			CardWidget[] array = Object.FindObjectsOfType<CardWidget>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshBgTextureForLoopOrder();
			}
		}

		// Token: 0x06000A1C RID: 2588 RVA: 0x000331CF File Offset: 0x000313CF
		public void UI_IsAnimatingEnvironmentEnabled(bool isOn)
		{
			GameMaster.IsAnimatingEnvironmentEnabled = isOn;
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x000331D7 File Offset: 0x000313D7
		public void UI_SingleEnemyAutoSelect(bool isOn)
		{
			GameMaster.SingleEnemyAutoSelect = isOn;
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x000331DF File Offset: 0x000313DF
		public void UI_ShowXCostEmptyUseWarning(bool isOn)
		{
			GameMaster.ShowXCostEmptyUseWarning = isOn;
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x000331E8 File Offset: 0x000313E8
		public void UI_ShowShortcut(bool isOn)
		{
			GameMaster.ShowShortcut = isOn;
			HandCard[] array = Object.FindObjectsOfType<HandCard>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ShowShortcut = isOn;
			}
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x00033218 File Offset: 0x00031418
		public void UI_ShowCardOrder(bool isOn)
		{
			GameMaster.ShowCardOrder = isOn;
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x00033220 File Offset: 0x00031420
		public void UI_ShowReload(bool isOn)
		{
			GameMaster.ShowReload = isOn;
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x00033228 File Offset: 0x00031428
		public void UI_Shake(bool isOn)
		{
			GameMaster.Shake = isOn;
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x00033230 File Offset: 0x00031430
		public void UI_CostMoreLeft(bool isOn)
		{
			GameMaster.CostMoreLeft = isOn;
			foreach (HandCard handCard in Object.FindObjectsOfType<HandCard>())
			{
				handCard.CostMoreLeft = handCard.ShouldShowLeftCost;
			}
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00033265 File Offset: 0x00031465
		public void UI_Cursor(bool isOn)
		{
			GameMaster.SetCursor(isOn);
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x0003326D File Offset: 0x0003146D
		public void UI_ChangeHintLevel(int index)
		{
			GameMaster.HintLevel = (HintLevel)index;
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x00033275 File Offset: 0x00031475
		public void UI_ChangeQuickPlayLevel(int index)
		{
			GameMaster.QuickPlayLevel = (QuickPlayLevel)index;
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x00033280 File Offset: 0x00031480
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

		// Token: 0x06000A28 RID: 2600 RVA: 0x000332D3 File Offset: 0x000314D3
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

		// Token: 0x06000A29 RID: 2601 RVA: 0x0003330F File Offset: 0x0003150F
		public void UI_Hide()
		{
			base.Hide();
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x00033318 File Offset: 0x00031518
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

		// Token: 0x06000A2B RID: 2603 RVA: 0x00033374 File Offset: 0x00031574
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

		// Token: 0x06000A2C RID: 2604 RVA: 0x000333B4 File Offset: 0x000315B4
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

		// Token: 0x06000A2D RID: 2605 RVA: 0x000333F4 File Offset: 0x000315F4
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

		// Token: 0x06000A2E RID: 2606 RVA: 0x00033480 File Offset: 0x00031680
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

		// Token: 0x06000A2F RID: 2607 RVA: 0x000336F4 File Offset: 0x000318F4
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

		// Token: 0x06000A30 RID: 2608 RVA: 0x000338F8 File Offset: 0x00031AF8
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

		// Token: 0x06000A31 RID: 2609 RVA: 0x0003394C File Offset: 0x00031B4C
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

		// Token: 0x06000A32 RID: 2610 RVA: 0x00033A92 File Offset: 0x00031C92
		public void SetReenterStationInteractable(bool interactable)
		{
			this.reenterStationButton.interactable = interactable;
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x00033AA0 File Offset: 0x00031CA0
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

		// Token: 0x0400076A RID: 1898
		[Header("综合")]
		[SerializeField]
		private List<CanvasGroup> tabs;

		// Token: 0x0400076B RID: 1899
		[SerializeField]
		private ToggleGroup tabGroup;

		// Token: 0x0400076C RID: 1900
		[SerializeField]
		private GameObject mainMenuGroup;

		// Token: 0x0400076D RID: 1901
		[SerializeField]
		private GameObject inGameGroup;

		// Token: 0x0400076E RID: 1902
		[SerializeField]
		private Button reenterStationButton;

		// Token: 0x0400076F RID: 1903
		[SerializeField]
		private Button backgroundHideButton;

		// Token: 0x04000770 RID: 1904
		[Header("主设置")]
		[SerializeField]
		private DropdownWidget resolutionDropdown;

		// Token: 0x04000771 RID: 1905
		[SerializeField]
		private DropdownWidget frameRateDropdown;

		// Token: 0x04000772 RID: 1906
		[SerializeField]
		private SwitchWidget fullScreenSwitch;

		// Token: 0x04000773 RID: 1907
		[SerializeField]
		private CanvasGroup animatingEnvironmentEnabledParent;

		// Token: 0x04000774 RID: 1908
		[SerializeField]
		private SwitchWidget animatingEnvironmentEnabledSwitch;

		// Token: 0x04000775 RID: 1909
		[SerializeField]
		private GameObject localeGroup;

		// Token: 0x04000776 RID: 1910
		[SerializeField]
		private DropdownWidget localeDropdown;

		// Token: 0x04000777 RID: 1911
		[Header("音频")]
		[SerializeField]
		private Slider masterVolumeSlider;

		// Token: 0x04000778 RID: 1912
		[SerializeField]
		private TextMeshProUGUI masterVolumeText;

		// Token: 0x04000779 RID: 1913
		[SerializeField]
		private Slider bgmVolumeSlider;

		// Token: 0x0400077A RID: 1914
		[SerializeField]
		private TextMeshProUGUI bgmVolumeText;

		// Token: 0x0400077B RID: 1915
		[SerializeField]
		private Slider uiVolumeSlider;

		// Token: 0x0400077C RID: 1916
		[SerializeField]
		private TextMeshProUGUI uiVolumeText;

		// Token: 0x0400077D RID: 1917
		[SerializeField]
		private Slider sfxVolumeSlider;

		// Token: 0x0400077E RID: 1918
		[SerializeField]
		private TextMeshProUGUI sfxVolumeText;

		// Token: 0x0400077F RID: 1919
		[SerializeField]
		private SwitchWidget backgroundMuteToggle;

		// Token: 0x04000780 RID: 1920
		[Header("偏好")]
		[SerializeField]
		private GameObject turboModeParent;

		// Token: 0x04000781 RID: 1921
		[SerializeField]
		private SwitchWidget turboModeSwitch;

		// Token: 0x04000782 RID: 1922
		[SerializeField]
		private GameObject keywordParent;

		// Token: 0x04000783 RID: 1923
		[SerializeField]
		private SwitchWidget keywordSwitch;

		// Token: 0x04000784 RID: 1924
		[SerializeField]
		private GameObject showIllustratorParent;

		// Token: 0x04000785 RID: 1925
		[SerializeField]
		private SwitchWidget showIllustratorSwitch;

		// Token: 0x04000786 RID: 1926
		[SerializeField]
		private GameObject tooltipSizeParent;

		// Token: 0x04000787 RID: 1927
		[SerializeField]
		private SwitchWidget tooltipSizeSwitch;

		// Token: 0x04000788 RID: 1928
		[SerializeField]
		private GameObject tooltipWideParent;

		// Token: 0x04000789 RID: 1929
		[SerializeField]
		private SwitchWidget tooltipWideSwitch;

		// Token: 0x0400078A RID: 1930
		[SerializeField]
		private GameObject rightClickCancelParent;

		// Token: 0x0400078B RID: 1931
		[SerializeField]
		private SwitchWidget rightClickCancelSwitch;

		// Token: 0x0400078C RID: 1932
		[SerializeField]
		private GameObject loopOrderSizeParent;

		// Token: 0x0400078D RID: 1933
		[SerializeField]
		private SwitchWidget loopOrderSizeSwitch;

		// Token: 0x0400078E RID: 1934
		[SerializeField]
		private GameObject singleEnemyAutoSelectParent;

		// Token: 0x0400078F RID: 1935
		[SerializeField]
		private SwitchWidget singleEnemyAutoSelectSwitch;

		// Token: 0x04000790 RID: 1936
		[SerializeField]
		private GameObject quickPlayLevelParent;

		// Token: 0x04000791 RID: 1937
		[SerializeField]
		private TMP_Dropdown quickPlayLevelDropdown;

		// Token: 0x04000792 RID: 1938
		[SerializeField]
		private GameObject showXCostWarningParent;

		// Token: 0x04000793 RID: 1939
		[SerializeField]
		private SwitchWidget showXCostWarningSwitch;

		// Token: 0x04000794 RID: 1940
		[SerializeField]
		private GameObject showShortcutParent;

		// Token: 0x04000795 RID: 1941
		[SerializeField]
		private SwitchWidget showShortcutSwitch;

		// Token: 0x04000796 RID: 1942
		[SerializeField]
		private GameObject showCardOrderParent;

		// Token: 0x04000797 RID: 1943
		[SerializeField]
		private SwitchWidget showCardOrderSwitch;

		// Token: 0x04000798 RID: 1944
		[SerializeField]
		private GameObject showReloadParent;

		// Token: 0x04000799 RID: 1945
		[SerializeField]
		private SwitchWidget showReloadSwitch;

		// Token: 0x0400079A RID: 1946
		[SerializeField]
		private GameObject shakeParent;

		// Token: 0x0400079B RID: 1947
		[SerializeField]
		private SwitchWidget shakeSwitch;

		// Token: 0x0400079C RID: 1948
		[SerializeField]
		private GameObject costMoreLeftParent;

		// Token: 0x0400079D RID: 1949
		[SerializeField]
		private SwitchWidget costMoreLeftSwitch;

		// Token: 0x0400079E RID: 1950
		[SerializeField]
		private GameObject cursorParent;

		// Token: 0x0400079F RID: 1951
		[SerializeField]
		private SwitchWidget cursorSwitch;

		// Token: 0x040007A0 RID: 1952
		[SerializeField]
		private GameObject hintLevelParent;

		// Token: 0x040007A1 RID: 1953
		[SerializeField]
		private TMP_Dropdown hintLevelDropdown;

		// Token: 0x040007A2 RID: 1954
		[SerializeField]
		private GameObject resetHintParent;

		// Token: 0x040007A3 RID: 1955
		[Header("按键")]
		[SerializeField]
		private RectTransform keyMappingParent;

		// Token: 0x040007A4 RID: 1956
		[SerializeField]
		private ActionMappingRow actionMappingRowTemplate;

		// Token: 0x040007A5 RID: 1957
		[SerializeField]
		private GameObject rebindRoot;

		// Token: 0x040007A6 RID: 1958
		[SerializeField]
		private TextMeshProUGUI rebindMessage;

		// Token: 0x040007A7 RID: 1959
		[SerializeField]
		private GameObject enableKeyboardParent;

		// Token: 0x040007A8 RID: 1960
		[SerializeField]
		private SwitchWidget enableKeyboardSwitch;

		// Token: 0x040007A9 RID: 1961
		[SerializeField]
		private GameObject enableGamepadParent;

		// Token: 0x040007AA RID: 1962
		[SerializeField]
		private SwitchWidget enableGamepadSwitch;

		// Token: 0x040007AB RID: 1963
		[SerializeField]
		private CanvasGroup keyBindingsGroup;

		// Token: 0x040007AC RID: 1964
		private CanvasGroup _canvasGroup;

		// Token: 0x040007AD RID: 1965
		private IReadOnlyList<FrameSetting> _frameSettings;

		// Token: 0x040007AE RID: 1966
		private IReadOnlyList<Vector2Int> _resolutions;

		// Token: 0x040007AF RID: 1967
		private SimpleTooltipSource _animatingEnvironmentTooltipSource;

		// Token: 0x040007B0 RID: 1968
		private const float RecommendMaster = 50f;

		// Token: 0x040007B1 RID: 1969
		private const float RecommendMusic = 50f;

		// Token: 0x040007B2 RID: 1970
		private const float RecommendUi = 100f;

		// Token: 0x040007B3 RID: 1971
		private const float RecommendSfx = 100f;

		// Token: 0x040007B4 RID: 1972
		private bool sfxPlaying;

		// Token: 0x040007B5 RID: 1973
		private float sfxTimer;

		// Token: 0x040007B6 RID: 1974
		private bool uiPlaying;

		// Token: 0x040007B7 RID: 1975
		private float uiTimer;

		// Token: 0x040007B8 RID: 1976
		private int _tabIndex;

		// Token: 0x040007B9 RID: 1977
		private const float TweenTime = 0.3f;

		// Token: 0x040007BA RID: 1978
		private const float TweenMoveX = 1600f;
	}
}
