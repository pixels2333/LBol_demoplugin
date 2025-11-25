using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures;
using LBoL.Presentation.Environments;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Yarn;
namespace LBoL.Presentation.UI.Panels
{
	public class VnPanel : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.VisualNovel;
			}
		}
		public bool IsTempLocked { get; set; }
		public override async UniTask CustomLocalizationAsync()
		{
			this._nextString = "Game.Next".Localize(true);
			this._nextStageString = "Game.NextStage".Localize(true);
			this._skipString = "Reward.Skip".Localize(true);
			this._skipShopString = "Shop.Skip".Localize(true);
			TextMeshProUGUI textMeshProUGUI = this.nextButtonText;
			int? nextButtonStringIndex = this.NextButtonStringIndex;
			string text;
			if (nextButtonStringIndex != null)
			{
				switch (nextButtonStringIndex.GetValueOrDefault())
				{
				case 0:
					text = this._nextString;
					goto IL_00B0;
				case 1:
					text = this._nextStageString;
					goto IL_00B0;
				case 2:
					text = this._skipShopString;
					goto IL_00B0;
				}
			}
			text = this._skipString;
			IL_00B0:
			textMeshProUGUI.text = text;
			if (this._dialogRunner != null)
			{
				await this._dialogRunner.ReloadLocalizationAsync();
				if (this._currentLineShowing != null)
				{
					this._currentRevealTime = this._totalRevealTime;
					this._activeText.text = this._currentLineShowing.GetLocalizedText(this._dialogRunner);
				}
				foreach (OptionWidget optionWidget in this.optionWidgets)
				{
					if (optionWidget.isActiveAndEnabled)
					{
						optionWidget.GetComponentInChildren<TextMeshProUGUI>().text = optionWidget.Option.GetLocalizedText(this._dialogRunner);
					}
				}
			}
			if (this._leftCharacterNameUnit != null)
			{
				this.leftCharacterNameText.text = (L10nManager.Info.PreferShortName ? this._leftCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Short) : this._leftCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default));
			}
			if (this._rightCharacterNameUnit != null)
			{
				this.rightCharacterNameText.text = (L10nManager.Info.PreferShortName ? this._rightCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Short) : this._rightCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default));
			}
			if (this._currentAdventure != null)
			{
				this.adventureTitleText.text = this._currentAdventure.Title;
			}
		}
		private string HandleLineArgument(string arg, string format)
		{
			if (!arg.StartsWith('@'))
			{
				return arg;
			}
			ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(arg);
			ReadOnlySpan<char> readOnlySpan2 = readOnlySpan.Slice(1, readOnlySpan.Length - 1);
			if (MemoryExtensions.Equals(readOnlySpan2, "PlayerName", 4))
			{
				GameRunController gameRun = base.GameRun;
				UnitName unitName = ((gameRun != null) ? gameRun.Player.GetName() : null) ?? UnitNameTable.GetDefaultPlayerName();
				ValueTuple<bool, NounCase, UnitNameStyle> valueTuple = UnitName.ParseFormat(format);
				NounCase item = valueTuple.Item2;
				UnitNameStyle item2 = valueTuple.Item3;
				return unitName.ToString(true, item, item2);
			}
			if (MemoryExtensions.Equals(readOnlySpan2, "PlayerShortName", 4))
			{
				GameRunController gameRun2 = base.GameRun;
				UnitName unitName2 = ((gameRun2 != null) ? gameRun2.Player.GetName() : null) ?? UnitNameTable.GetDefaultPlayerName();
				ValueTuple<bool, NounCase, UnitNameStyle> valueTuple2 = UnitName.ParseFormat(format);
				NounCase item3 = valueTuple2.Item2;
				UnitNameStyle item4 = valueTuple2.Item3;
				return unitName2.ToString(true, item3, (item4 == UnitNameStyle.Default) ? UnitNameStyle.Short : item4);
			}
			if (MemoryExtensions.StartsWith(readOnlySpan2, "Exhibit-", 4))
			{
				readOnlySpan = readOnlySpan2;
				int num = "Exhibit-".Length;
				return LBoL.Core.Library.CreateExhibit(new string(readOnlySpan.Slice(num, readOnlySpan.Length - num))).GetName().ToString(format);
			}
			if (MemoryExtensions.StartsWith(readOnlySpan2, "Card-", 4))
			{
				readOnlySpan = readOnlySpan2;
				int num = "Card-".Length;
				return LBoL.Core.Library.CreateCard(new string(readOnlySpan.Slice(num, readOnlySpan.Length - num))).GetName().ToString(format);
			}
			throw new ArgumentException("Invalid argument in dialog: " + arg);
		}
		public int? NextButtonStringIndex { get; private set; }
		public void SetNextButton(bool active, int? index = null, Action call = null)
		{
			if (call != null)
			{
				this._nextButtonHandler = call;
			}
			this.nextButton.gameObject.SetActive(active);
			this.nextButton.interactable = active;
			if (index != null)
			{
				this.NextButtonStringIndex = index;
				TextMeshProUGUI textMeshProUGUI = this.nextButtonText;
				string text;
				if (index != null)
				{
					switch (index.GetValueOrDefault())
					{
					case 0:
						text = this._nextString;
						goto IL_0085;
					case 1:
						text = this._nextStageString;
						goto IL_0085;
					case 2:
						text = this._skipShopString;
						goto IL_0085;
					}
				}
				text = this._skipString;
				IL_0085:
				textMeshProUGUI.text = text;
			}
		}
		public bool AllScreenMode
		{
			get
			{
				return this._allScreenMode;
			}
			set
			{
				this._allScreenMode = value;
				if (value)
				{
					this._activeTextRoot = this.comicTextRoot;
					this._activeText = this.comicText;
					this.textRoot.SetActive(false);
					this.mainText.gameObject.SetActive(false);
					this.NextButtonRect.sizeDelta = new Vector2(3840f, 2160f);
				}
				else
				{
					this._activeTextRoot = this.textRoot;
					this._activeText = this.mainText;
					this.comicTextRoot.SetActive(false);
					this.comicText.gameObject.SetActive(false);
					this.NextButtonRect.sizeDelta = this.NextButtonDefaultSize;
				}
				this._activeText.text = string.Empty;
				this._activeTextRoot.SetActive(true);
				this._activeText.gameObject.SetActive(true);
			}
		}
		private RectTransform NextButtonRect { get; set; }
		private Vector2 NextButtonDefaultSize { get; set; }
		private void SetUserSkipping(bool value)
		{
			this._userSkipping = value;
			this.skipIcon.gameObject.SetActive(value);
		}
		private bool Skipping
		{
			get
			{
				return this._userSkipping || this._allSkipping;
			}
		}
		public void Awake()
		{
			this._canvasGroup = this.root.GetComponent<CanvasGroup>();
			if (!this._canvasGroup)
			{
				this._canvasGroup = this.root.AddComponent<CanvasGroup>();
			}
			this._commandHandler = RuntimeCommandHandler.Create(this);
			foreach (object obj in this.characterSlotRoot)
			{
				Transform transform = (Transform)obj;
				Image component = transform.GetComponent<Image>();
				if (component)
				{
					VnPanel.ProtraitSlot protraitSlot = new VnPanel.ProtraitSlot(component, transform.name);
					this._characterSlots.Add(protraitSlot.Name, protraitSlot);
					protraitSlot.IsActive = false;
				}
			}
			foreach (object obj2 in this.simpleSlotRoot)
			{
				Transform transform2 = (Transform)obj2;
				Image component2 = transform2.GetComponent<Image>();
				if (component2)
				{
					VnPanel.SimpleSlot simpleSlot = new VnPanel.SimpleSlot(component2, transform2.name);
					this._simpleSlots.Add(simpleSlot.Name, simpleSlot);
					simpleSlot.IsActive = false;
				}
			}
			foreach (VnPanel.AdvSlotEntry advSlotEntry in this.advSlots)
			{
				VnPanel.AdvSlot advSlot = new VnPanel.AdvSlot(advSlotEntry.front, advSlotEntry.back);
				advSlot.Clear();
				this._advSlots.Add(advSlot);
			}
			foreach (ValueTuple<int, OptionWidget> valueTuple in this.optionWidgets.WithIndices<OptionWidget>())
			{
				int i = valueTuple.Item1;
				OptionWidget item = valueTuple.Item2;
				item.AddListener(delegate
				{
					this.OnClickOption(i);
				});
				item.gameObject.SetActive(false);
			}
			this.nextLineButton.onClick.AddListener(new UnityAction(this.OnClickNextLine));
			this.NextButtonRect = this.nextLineButton.GetComponent<RectTransform>();
			this.NextButtonDefaultSize = this.NextButtonRect.sizeDelta;
			this.HideContent();
			this.cardWidgetTemplate.gameObject.SetActive(false);
			this.exhibitWidgetTemplate.gameObject.SetActive(false);
			this.optionSourceLayout.SetActive(false);
			this.nextButton.onClick.AddListener(delegate
			{
				if (!this.IsTempLocked)
				{
					Action nextButtonHandler = this._nextButtonHandler;
					if (nextButtonHandler == null)
					{
						return;
					}
					nextButtonHandler.Invoke();
				}
			});
			this.nextButton.gameObject.SetActive(false);
			this.skipIcon.gameObject.SetActive(false);
			this.tempArtText.gameObject.SetActive(false);
		}
		protected override void OnShowing()
		{
			GameMaster.ShowPoseAnimation = false;
		}
		protected override void OnHided()
		{
			GameMaster.ShowPoseAnimation = true;
			this.skipIcon.gameObject.SetActive(false);
		}
		public void UserSkipDialog(bool skip)
		{
			if (skip)
			{
				if (this._totalRevealTime != 0f)
				{
					this._currentRevealTime = this._totalRevealTime;
				}
				this.SetUserSkipping(true);
				return;
			}
			this.SetUserSkipping(false);
		}
		private void OnClickOption(int i)
		{
			if (this.IsTempLocked)
			{
				return;
			}
			this.ClearOptionSource();
			DialogOptionsPhase dialogOptionsPhase = this._dialogRunner.CurrentPhase as DialogOptionsPhase;
			if (dialogOptionsPhase == null)
			{
				Debug.LogError("Not in option phase");
				return;
			}
			if (i < 0 || i >= dialogOptionsPhase.Options.Length)
			{
				Debug.LogError(string.Format("Button index {0} out of options range (0 - {1})", i, dialogOptionsPhase.Options.Length - 1));
				return;
			}
			DialogOption dialogOption = dialogOptionsPhase.Options[i];
			OptionWidget[] array = this.optionWidgets;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].gameObject.SetActive(false);
			}
			this._selectedOptionId = new int?(dialogOption.Id);
		}
		public bool HandleNavigateAction(NavigateDirection dir)
		{
			return false;
		}
		public bool HandleSelectionFromKey(int i)
		{
			if (!this.IsRunning)
			{
				return false;
			}
			if (this.IsTempLocked)
			{
				return false;
			}
			if (this.optionsRoot.activeSelf)
			{
				List<DialogOption> list = Enumerable.ToList<DialogOption>(Enumerable.Where<DialogOption>(this._options, (DialogOption option) => option.Available));
				if (i >= 0 && i < list.Count)
				{
					this._selectedOptionId = new int?(list[i].Id);
				}
				return true;
			}
			return false;
		}
		public bool HandleConfirmAction()
		{
			if (this.IsTempLocked)
			{
				return false;
			}
			if (this.IsRunning)
			{
				this.OnClickNextLine();
				return true;
			}
			return false;
		}
		public bool HandleCancelAction()
		{
			if (this.IsTempLocked)
			{
				return false;
			}
			if (this.IsRunning && this.CanSkipAll)
			{
				UiManager.GetDialog<MessageDialog>().Show(new MessageContent
				{
					TextKey = "SkipDialog",
					Buttons = DialogButtons.ConfirmCancel,
					OnConfirm = new Action(this.SkipAll)
				});
				return true;
			}
			return false;
		}
		private void OnClickNextLine()
		{
			if (this.IsTempLocked)
			{
				return;
			}
			if (this._totalRevealTime > 0f)
			{
				this._currentRevealTime = this._totalRevealTime;
				return;
			}
			this._shouldShowNextLine = true;
		}
		private DialogOptionData GetOptionDataCache(int index)
		{
			if (this._optionDataCache == null)
			{
				this._optionDataCache = Enumerable.ToList<DialogOptionData>(Enumerable.Select<int, DialogOptionData>(Enumerable.Range(0, index + 1), (int _) => new DialogOptionData()));
			}
			else if (this._optionDataCache.Count <= index)
			{
				this._optionDataCache = Enumerable.ToList<DialogOptionData>(Enumerable.Concat<DialogOptionData>(this._optionDataCache, Enumerable.Select<int, DialogOptionData>(Enumerable.Range(0, index + 1 - this._optionDataCache.Count), (int _) => new DialogOptionData())));
			}
			return this._optionDataCache[index];
		}
		private void ShowCharacter(string slotName, Unit unit, string defaultSprite)
		{
			VnPanel.ProtraitSlot protraitSlot;
			if (!this._characterSlots.TryGetValue(slotName, ref protraitSlot))
			{
				Debug.LogError("[Dialog]: Cannot show character at slot '" + slotName + "': no such slot.");
			}
			if (protraitSlot != null)
			{
				protraitSlot.Load(unit.Id, defaultSprite);
			}
		}
		private IEnumerator HideCharacter(string slotName)
		{
			VnPanel.ProtraitSlot protraitSlot;
			if (!this._characterSlots.TryGetValue(slotName, ref protraitSlot))
			{
				Debug.LogError("[Dialog]: Cannot remove character at slot '" + slotName + "': no such slot.");
				yield break;
			}
			protraitSlot.Release();
			yield break;
		}
		private IEnumerator InternalShowEnemyTitle(string eName, string title)
		{
			yield return new WaitForSeconds(1f);
			this.enemyName.text = eName;
			this.enemyTitle.text = title;
			this.enemyTitleRoot.SetActive(true);
			Sequence sequence = DOTween.Sequence();
			sequence.Join(this.enemyName.DOFade(1f, 0.2f).From(0f, true, false));
			sequence.Join(this.enemyTitle.DOFade(1f, 0.2f).From(0f, true, false));
			sequence.Join(this.enemyTitleBack.DOFade(1f, 0.2f).From(0f, true, false));
			sequence.Join(this.enemyName.transform.DOLocalMoveX(10f, 0.4f, false).From(80f, true, false).SetEase(Ease.OutSine));
			sequence.Join(this.enemyTitle.transform.DOLocalMoveX(-20f, 0.4f, false).From(-160f, true, false).SetEase(Ease.OutSine));
			yield return sequence.SetUpdate(true).WaitForCompletion();
			Sequence sequence2 = DOTween.Sequence();
			sequence2.Join(this.enemyName.transform.DOLocalMoveX(-10f, 4f, false).From(10f, true, false).SetEase(Ease.OutSine));
			sequence2.Join(this.enemyTitle.transform.DOLocalMoveX(20f, 4f, false).From(-20f, true, false).SetEase(Ease.OutSine));
			yield return sequence2.SetUpdate(true).WaitForCompletion();
			Sequence sequence3 = DOTween.Sequence();
			sequence3.Join(this.enemyName.DOFade(0f, 0.2f).From(1f, true, false));
			sequence3.Join(this.enemyTitle.DOFade(0f, 0.2f).From(1f, true, false));
			sequence3.Join(this.enemyTitleBack.DOFade(0f, 0.2f).From(1f, true, false));
			yield return sequence3.SetUpdate(true).WaitForCompletion();
			this.enemyTitleRoot.SetActive(false);
			yield break;
		}
		private IEnumerator ShowEnemyTitle(string name, string title)
		{
			return this.InternalShowEnemyTitle(name, title);
		}
		private IEnumerator HighlightSlots([TupleElementNames(new string[] { "slotName", "sprite" })] params ValueTuple<string, string>[] highlightSlots)
		{
			HashSet<string> hashSet = new HashSet<string>(this._characterSlots.Keys);
			hashSet.ExceptWith(Enumerable.Select<ValueTuple<string, string>, string>(highlightSlots, ([TupleElementNames(new string[] { "slotName", "sprite" })] ValueTuple<string, string> pair) => pair.Item1));
			foreach (ValueTuple<string, string> valueTuple in highlightSlots)
			{
				string item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				VnPanel.ProtraitSlot protraitSlot;
				if (!this._characterSlots.TryGetValue(item, ref protraitSlot))
				{
					Debug.LogError("Cannot highlight slot '" + item + "': no such slot.");
				}
				else
				{
					if (item2 != null)
					{
						protraitSlot.Image.sprite = protraitSlot.Get(item2);
					}
					protraitSlot.Image.color = Color.white;
				}
			}
			using (HashSet<string>.Enumerator enumerator = hashSet.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string text = enumerator.Current;
					this._characterSlots[text].Image.color = Color.gray;
				}
				yield break;
			}
			yield break;
		}
		private void SetCharacterName()
		{
			if (this._nextLineCharacterName != null)
			{
				if (this._nextLineCharacterName.IsLeft)
				{
					this.leftCharacterNameRoot.SetActive(true);
					this.rightCharacterNameRoot.SetActive(false);
					this._leftCharacterNameUnit = this._nextLineCharacterName.Unit;
					this.leftCharacterNameText.text = (L10nManager.Info.PreferShortName ? this._leftCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Short) : this._leftCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default));
					this._rightCharacterNameUnit = null;
				}
				else
				{
					this.leftCharacterNameRoot.SetActive(false);
					this.rightCharacterNameRoot.SetActive(true);
					this._rightCharacterNameUnit = this._nextLineCharacterName.Unit;
					this.rightCharacterNameText.text = (L10nManager.Info.PreferShortName ? this._rightCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Short) : this._rightCharacterNameUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default));
					this._leftCharacterNameUnit = null;
				}
				this._nextLineCharacterName = null;
				return;
			}
			this.leftCharacterNameRoot.SetActive(false);
			this.rightCharacterNameRoot.SetActive(false);
			this._leftCharacterNameUnit = null;
			this._rightCharacterNameUnit = null;
		}
		private IEnumerator CoRevealText(string content, float speed, float ahead)
		{
			this._activeText.text = content;
			this._activeText.ForceMeshUpdate(false, false);
			int characterCount = this._activeText.textInfo.characterCount;
			if (characterCount == 0)
			{
				yield break;
			}
			this._totalRevealTime = Mathf.Min(((float)characterCount + ahead) / speed, 30f);
			this._currentRevealTime = 0f;
			do
			{
				VnPanel.<CoRevealText>g__SetCharColors|114_0(this._activeText, this._currentRevealTime / this._totalRevealTime, ahead);
				this._currentRevealTime += Time.unscaledDeltaTime;
				yield return null;
			}
			while (this._currentRevealTime < this._totalRevealTime);
			VnPanel.<CoRevealText>g__SetCharColors|114_0(this._activeText, 1f, ahead);
			this._totalRevealTime = (this._currentRevealTime = 0f);
			yield break;
		}
		private IEnumerator ShowOptions(DialogOption[] options)
		{
			OptionWidget[] array = this.optionWidgets;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
			foreach (ValueTuple<int, DialogOption> valueTuple in options.WithIndices<DialogOption>())
			{
				int item = valueTuple.Item1;
				DialogOption item2 = valueTuple.Item2;
				if (item2.Available)
				{
					if (item >= this.optionWidgets.Length)
					{
						Debug.LogWarning("[Dialog] Cannot show more options: " + item2.GetLocalizedText(this._dialogRunner));
					}
					else
					{
						OptionWidget optionWidget = this.optionWidgets[item];
						optionWidget.GetComponentInChildren<TextMeshProUGUI>().text = item2.GetLocalizedText(this._dialogRunner);
						optionWidget.gameObject.SetActive(true);
						optionWidget.GetComponentInChildren<Button>().interactable = item2.Data.IsActive;
						optionWidget.enabled = item2.Data.IsActive;
					}
				}
			}
			this.optionsRoot.SetActive(true);
			this._options = options;
			this._selectedOptionId = default(int?);
			yield return new WaitUntil(() => this._selectedOptionId != null);
			this._dialogRunner.SelectOption(this._selectedOptionId.Value);
			this.optionsRoot.SetActive(false);
			this._options = null;
			this._selectedOptionId = default(int?);
			yield break;
		}
		public Vector3 GetOptionWorldPosition(int index)
		{
			if (index < 0)
			{
				int? selectedOptionId = this._selectedOptionId;
				if (selectedOptionId != null)
				{
					int valueOrDefault = selectedOptionId.GetValueOrDefault();
					index = valueOrDefault;
				}
				else
				{
					index = 0;
				}
			}
			return this.optionsRoot.transform.GetChild(index).position;
		}
		private IEnumerator RunCommand(string command, [MaybeNull] RuntimeCommandHandler extraCommandHandler = null)
		{
			IEnumerator enumerator;
			if (extraCommandHandler != null && extraCommandHandler.TryHandleCommand(command, out enumerator))
			{
				if (enumerator != null)
				{
					yield return enumerator;
				}
				if (base.GameRun != null)
				{
					GameMaster.GameRunFailCheck();
				}
			}
			else if (this._commandHandler.TryHandleCommand(command, out enumerator))
			{
				if (enumerator != null)
				{
					yield return enumerator;
				}
				if (base.GameRun != null)
				{
					GameMaster.GameRunFailCheck();
				}
			}
			else
			{
				Debug.LogWarning("[Dialog] Command: \"" + command.Replace("\"", "\\\"") + "\" not handled");
			}
			yield break;
		}
		private AdventureSaveData SaveAdventureState(string jumpTargetNodeName)
		{
			if (this._runningCoroutine == null || this._currentAdventure == null)
			{
				throw new InvalidOperationException("[VnPanel] Cannot save adventure state while not in adventure");
			}
			AdventureSaveData adventureSaveData = new AdventureSaveData();
			adventureSaveData.AdventureId = this._currentAdventure.Id;
			adventureSaveData.NodeName = jumpTargetNodeName;
			adventureSaveData.StorageYaml = this._storage.Save();
			adventureSaveData.Slots = Enumerable.ToArray<AdvSlotSaveData>(Enumerable.Select<VnPanel.AdvSlot, AdvSlotSaveData>(this._advSlots, (VnPanel.AdvSlot slot) => slot.Save()));
			return adventureSaveData;
		}
		public Coroutine RunDialog(string vnName, DialogStorage storage, global::Yarn.Library library, [MaybeNull] RuntimeCommandHandler extraCommandHandler = null, [MaybeNull] string startNode = null, [MaybeNull] Adventure adventure = null, [MaybeNull] VnExtraSettings extraSettings = null)
		{
			if (this._runningCoroutine != null)
			{
				throw new InvalidOperationException("Already in dialog");
			}
			this._canSkipAll = false;
			this._allSkipping = false;
			this._extraSettings = extraSettings;
			this._storage = storage ?? new DialogStorage();
			this._runningCoroutine = base.StartCoroutine(this.CoRunDialog(vnName, this._storage, library, extraCommandHandler, startNode, adventure));
			return this._runningCoroutine;
		}
		public Coroutine RestoreAdventure(string vnName, Adventure adventure, AdventureSaveData saveData, DialogStorage storage, global::Yarn.Library library, [MaybeNull] RuntimeCommandHandler extraCommandHandler, [MaybeNull] VnExtraSettings extraSettings = null)
		{
			if (this._runningCoroutine != null)
			{
				throw new InvalidOperationException("Already in dialog");
			}
			this._storage = storage;
			this._canSkipAll = false;
			this._allSkipping = false;
			this._extraSettings = extraSettings;
			this.adventureFrame.gameObject.SetActive(true);
			this.adventureTitle.gameObject.SetActive(true);
			int num = saveData.Slots.Length;
			foreach (ValueTuple<int, VnPanel.AdvSlot> valueTuple in this._advSlots.WithIndices<VnPanel.AdvSlot>())
			{
				int item = valueTuple.Item1;
				VnPanel.AdvSlot item2 = valueTuple.Item2;
				if (item < num)
				{
					item2.Restore(saveData.Slots[item]);
				}
				else
				{
					item2.Clear();
				}
			}
			this._runningCoroutine = base.StartCoroutine(this.CoRunDialog(vnName, this._storage, library, extraCommandHandler, saveData.NodeName, adventure));
			return this._runningCoroutine;
		}
		public bool IsRunning
		{
			get
			{
				return this._runningCoroutine != null;
			}
		}
		public bool CanSkipAll
		{
			get
			{
				return this._canSkipAll;
			}
		}
		public void Stop()
		{
			base.StopAllCoroutines();
			this.End();
		}
		public void SkipAll()
		{
			if (!this._canSkipAll)
			{
				Debug.LogError("[VnPanel] Cannot skip all while not enabled");
				return;
			}
			this._allSkipping = true;
		}
		private IEnumerator CoRunDialog(string vnName, DialogStorage storage, global::Yarn.Library library, [MaybeNull] RuntimeCommandHandler extraCommandHandler = null, [MaybeNull] string startNode = null, [MaybeNull] Adventure adventure = null)
		{
			VnPanel.<>c__DisplayClass127_0 CS$<>8__locals1 = new VnPanel.<>c__DisplayClass127_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.task = DialogRunner.LoadAsync(vnName, storage, library);
			CS$<>8__locals1.runner = null;
			CS$<>8__locals1.exception = null;
			yield return UniTask.ToCoroutine(delegate
			{
				VnPanel.<>c__DisplayClass127_0.<<CoRunDialog>b__0>d <<CoRunDialog>b__0>d;
				<<CoRunDialog>b__0>d.<>t__builder = AsyncUniTaskMethodBuilder.Create();
				<<CoRunDialog>b__0>d.<>4__this = CS$<>8__locals1;
				<<CoRunDialog>b__0>d.<>1__state = -1;
				<<CoRunDialog>b__0>d.<>t__builder.Start<VnPanel.<>c__DisplayClass127_0.<<CoRunDialog>b__0>d>(ref <<CoRunDialog>b__0>d);
				return <<CoRunDialog>b__0>d.<>t__builder.Task;
			});
			if (CS$<>8__locals1.runner != null)
			{
				this._dialogRunner = CS$<>8__locals1.runner;
				this._dialogRunner.LineArgumentHandler = new LineArgumentHandler(this.HandleLineArgument);
				if (adventure != null)
				{
					this._currentAdventure = adventure;
					if (adventure is Debut)
					{
						this.NextButtonRect.sizeDelta = this._debutSize;
					}
					int music = adventure.Config.Music;
					if (adventure.Config.HideUlt)
					{
						UiManager.GetPanel<UltimateSkillPanel>().HideInDialog();
					}
					else
					{
						UiManager.GetPanel<UltimateSkillPanel>().ShowInDialog();
					}
					this.tempArtText.gameObject.SetActive(adventure.Config.TempArt);
					if (music != 0)
					{
						AudioManager.PlayAdventureBgm(music);
					}
					this.adventureTitleText.text = this._currentAdventure.Title;
				}
				else if (vnName == "Opening")
				{
					AudioManager.PlayAdventureBgm(1);
				}
				this._canvasGroup.alpha = 0f;
				this.root.SetActive(true);
				yield return this._canvasGroup.DOFade(1f, 0.3f).SetUpdate(true).WaitForCompletion();
				this._activeTextRoot.SetActive(true);
				VnExtraSettings vnExtraSettings = this._extraSettings;
				if (vnExtraSettings != null && vnExtraSettings.HideLoadingBeforeStart)
				{
					yield return UiManager.HideLoading(0.5f);
				}
				foreach (DialogPhase dialogPhase in this._dialogRunner.Phases(startNode))
				{
					if (this._allSkipping)
					{
						break;
					}
					DialogLinePhase dialogLinePhase = dialogPhase as DialogLinePhase;
					if (dialogLinePhase == null)
					{
						DialogOptionsPhase dialogOptionsPhase = dialogPhase as DialogOptionsPhase;
						if (dialogOptionsPhase == null)
						{
							DialogCommandPhase dialogCommandPhase = dialogPhase as DialogCommandPhase;
							if (dialogCommandPhase == null)
							{
								Debug.LogError("Unknown yarn phase type: " + dialogPhase.GetType().Name);
							}
							else if (!this.TryHandleSpecialCommand(dialogCommandPhase.Text))
							{
								yield return this.RunCommand(dialogCommandPhase.Text, extraCommandHandler);
							}
						}
						else
						{
							DialogOption[] options = dialogOptionsPhase.Options;
							foreach (ValueTuple<int, DialogOption> valueTuple in options.WithIndices<DialogOption>())
							{
								int item = valueTuple.Item1;
								DialogOption item2 = valueTuple.Item2;
								item2.Data = this.GetOptionDataCache(item);
								this.optionWidgets[item].SetOptionData(item2);
								if (item2.Data.Title != null)
								{
									SimpleTooltipSource simpleTooltipSource = SimpleTooltipSource.CreateDirect(this.optionWidgets[item].gameObject, item2.Data.Title.Localize(true), item2.Data.Content.Localize(true)).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
									this._optionTooltipList.Add(simpleTooltipSource);
								}
							}
							yield return this.ShowOptions(options);
							this._optionDataCache.Clear();
							foreach (SimpleTooltipSource simpleTooltipSource2 in this._optionTooltipList)
							{
								Object.Destroy(simpleTooltipSource2);
							}
							this._optionTooltipList.Clear();
						}
					}
					else
					{
						this._currentLineShowing = dialogLinePhase;
						this.SetCharacterName();
						if (this._nextLineAsVariable != null)
						{
							this._dialogRunner.VariableStorage.SetValue(this._nextLineAsVariable, dialogLinePhase.GetLocalizedText(this._dialogRunner));
							this._nextLineAsVariable = null;
						}
						else if (this._showNextLineInstantly)
						{
							this._activeText.text = dialogLinePhase.GetLocalizedText(this._dialogRunner);
							this._showNextLineInstantly = false;
						}
						else if (this.Skipping)
						{
							this._activeText.text = dialogLinePhase.GetLocalizedText(this._dialogRunner);
						}
						else
						{
							yield return this.CoRevealText(dialogLinePhase.GetLocalizedText(this._dialogRunner), L10nManager.Info.VnTextRevealSpeed, L10nManager.Info.VnTextRevealAhead);
							this._shouldShowNextLine = false;
							Func<bool> func;
							if ((func = CS$<>8__locals1.<>9__1) == null)
							{
								func = (CS$<>8__locals1.<>9__1 = () => CS$<>8__locals1.<>4__this._shouldShowNextLine || CS$<>8__locals1.<>4__this.Skipping || CS$<>8__locals1.<>4__this._noWait);
							}
							yield return new WaitUntil(func);
						}
						this._noWait = false;
					}
				}
				IEnumerator<DialogPhase> enumerator = null;
				if (vnName == "Opening" && adventure == null)
				{
					AudioManager.EnterLayer0();
				}
				vnExtraSettings = this._extraSettings;
				if (vnExtraSettings != null && vnExtraSettings.ShowLoadingAfterEnd)
				{
					yield return UiManager.ShowLoading(0.5f);
				}
				yield return this._canvasGroup.DOFade(0f, 0.3f).SetUpdate(true).WaitForCompletion();
				this.End();
				yield break;
			}
			if (CS$<>8__locals1.exception != null)
			{
				Debug.LogError(CS$<>8__locals1.exception);
			}
			yield break;
			yield break;
		}
		private void End()
		{
			this.ClearOptionSource();
			foreach (VnPanel.ProtraitSlot protraitSlot in this._characterSlots.Values)
			{
				protraitSlot.Release();
			}
			foreach (VnPanel.SimpleSlot simpleSlot in this._simpleSlots.Values)
			{
				simpleSlot.Clear();
			}
			foreach (VnPanel.AdvSlot advSlot in this._advSlots)
			{
				advSlot.Clear();
			}
			List<DialogOptionData> optionDataCache = this._optionDataCache;
			if (optionDataCache != null)
			{
				optionDataCache.Clear();
			}
			this.optionsRoot.SetActive(false);
			this._runningCoroutine = null;
			this._dialogRunner = null;
			this._currentLineShowing = null;
			this._extraSettings = null;
			this.HideContent();
		}
		public void HideContent()
		{
			this.AllScreenMode = false;
			this._currentAdventure = null;
			this._activeText.text = string.Empty;
			this._nextLineAsVariable = null;
			this._nextLineCharacterName = null;
			this.SetCharacterName();
			this.enemyTitleRoot.SetActive(false);
			this.adventureTitle.SetActive(false);
			this.adventureFrame.gameObject.SetActive(false);
			this.optionsRoot.SetActive(false);
			this.tempArtText.gameObject.SetActive(false);
			this.root.SetActive(false);
		}
		public void ShowOptionSource(OptionWidget source)
		{
			this.ClearOptionSource();
			float num = 50f;
			float num2 = 0f;
			source.IsThisSourceActive = true;
			this.optionSourceLayout.SetActive(true);
			this.optionSourceLayout.GetComponent<CanvasGroup>().DOKill(false);
			this.optionSourceLayout.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false);
			foreach (Card card in source.SourceCard)
			{
				CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardWidgetTemplate, this.optionSourceLayout.transform);
				cardWidget.gameObject.SetActive(true);
				cardWidget.Card = card;
				cardWidget.GetComponent<RectTransform>().anchoredPosition = new Vector2(num, 0f);
				this._cardWidgets.Add(cardWidget);
				cardWidget.gameObject.GetComponent<ShowingCard>().SetScale(0.7f, 0.7f);
				float num3;
				float num4;
				float num5;
				cardWidget.transform.localScale.Deconstruct(out num3, out num4, out num5);
				float num6 = num3;
				float num7 = num4;
				num += cardWidget.gameObject.GetComponent<RectTransform>().sizeDelta.x * num6 + 20f;
				num2 = cardWidget.gameObject.GetComponent<RectTransform>().sizeDelta.y * num7;
			}
			int num8 = 0;
			float num9 = num;
			foreach (Exhibit exhibit in source.SourceExhibit)
			{
				MuseumExhibitWidget museumExhibitWidget = Object.Instantiate<MuseumExhibitWidget>(this.exhibitWidgetTemplate, this.optionSourceLayout.transform);
				museumExhibitWidget.gameObject.SetActive(true);
				museumExhibitWidget.Exhibit = exhibit;
				museumExhibitWidget.GetComponent<RectTransform>().anchoredPosition = new Vector2(num9, (float)((num8 % 2 == 0) ? (-70) : 70));
				this._exhibitWidgets.Add(museumExhibitWidget);
				float num3;
				float num4;
				float num5;
				museumExhibitWidget.transform.localScale.Deconstruct(out num5, out num4, out num3);
				float num10 = num5;
				float num11 = num4;
				if (num8 == 0)
				{
					num += museumExhibitWidget.gameObject.GetComponent<RectTransform>().sizeDelta.x * num10 + 20f;
					num9 += museumExhibitWidget.gameObject.GetComponent<RectTransform>().sizeDelta.x * num10 * 0.5f + 10f;
				}
				else
				{
					num += museumExhibitWidget.gameObject.GetComponent<RectTransform>().sizeDelta.x * num10 * 0.5f + 10f;
					num9 += museumExhibitWidget.gameObject.GetComponent<RectTransform>().sizeDelta.x * num10 * 0.5f + 10f;
				}
				if (num8 % 2 != 0)
				{
					museumExhibitWidget.transform.SetAsFirstSibling();
				}
				num8++;
				num2 = ((museumExhibitWidget.gameObject.GetComponent<RectTransform>().sizeDelta.y * num11 > num2) ? (museumExhibitWidget.gameObject.GetComponent<RectTransform>().sizeDelta.y * num11) : num2);
			}
			this.optionSourceLayout.GetComponent<RectTransform>().sizeDelta = new Vector2(num + 30f, num2 + 100f);
		}
		public void ClearOptionSource()
		{
			this.optionSourceLayout.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).From(1f, true, false)
				.OnComplete(delegate
				{
					this.optionSourceLayout.SetActive(false);
				});
			foreach (MuseumExhibitWidget museumExhibitWidget in this._exhibitWidgets)
			{
				Object.Destroy(museumExhibitWidget.gameObject);
			}
			foreach (CardWidget cardWidget in this._cardWidgets)
			{
				Object.Destroy(cardWidget.gameObject);
			}
			this._exhibitWidgets.Clear();
			this._cardWidgets.Clear();
			OptionWidget[] array = this.optionWidgets;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].IsThisSourceActive = false;
			}
		}
		[RuntimeCommand("enableSkipAll", "")]
		[UsedImplicitly]
		public void EnableSkipAll()
		{
			this._canSkipAll = true;
		}
		[RuntimeCommand("wait", "")]
		[UsedImplicitly]
		public IEnumerator Wait(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			yield break;
		}
		[RuntimeCommand("jump", "")]
		[UsedImplicitly]
		public void Jump(string nodeName)
		{
			this._dialogRunner.Jump(nodeName);
		}
		[RuntimeCommand("saveAndJump", "")]
		[UsedImplicitly]
		public void SaveAndJump(string nodeName)
		{
			GameMaster.RequestSaveGameRunInAdventure(this.SaveAdventureState(nodeName));
			this._dialogRunner.Jump(nodeName);
		}
		[RuntimeCommand("varLine", "")]
		[UsedImplicitly]
		public void VarLine(string varName)
		{
			this._nextLineAsVariable = "$" + varName;
		}
		[RuntimeCommand("showInstantly", "")]
		[UsedImplicitly]
		public void ShowInstantly()
		{
			this._showNextLineInstantly = true;
		}
		[RuntimeCommand("noWait", "")]
		[UsedImplicitly]
		public void NoWait()
		{
			this._noWait = true;
		}
		[RuntimeCommand("optionCard", "")]
		[UsedImplicitly]
		public void OptionCard(int index, string cardId, bool? isUpgraded = null)
		{
			Card card = LBoL.Core.Library.CreateCard(cardId);
			if (isUpgraded == null)
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun != null)
				{
					gameRun.UpgradeNewDeckCardOnFlags(card);
				}
			}
			else if (isUpgraded.Value)
			{
				card.Upgrade();
			}
			this.GetOptionDataCache(index - 1).AddCard(card, false);
			GameMaster.RevealCard(cardId);
		}
		[RuntimeCommand("optionRandomCard", "")]
		[UsedImplicitly]
		public void OptionRandomCard(int index, string cardId, bool isUpgraded = false)
		{
			Card card = LBoL.Core.Library.CreateCard(cardId);
			if (isUpgraded)
			{
				card.Upgrade();
			}
			GameRunController gameRun = base.GameRun;
			if (gameRun != null)
			{
				gameRun.UpgradeNewDeckCardOnFlags(card);
			}
			this.GetOptionDataCache(index - 1).AddCard(card, true);
			GameMaster.RevealCard(cardId);
		}
		[RuntimeCommand("optionExhibit", "")]
		[UsedImplicitly]
		public void OptionExhibit(int index, string exhibitId, string message = null)
		{
			Exhibit exhibit = LBoL.Core.Library.CreateExhibit(exhibitId);
			if (message != null)
			{
				MethodInfo method = exhibit.GetType().GetMethod(message);
				if (method != null)
				{
					method.Invoke(exhibit, null);
				}
			}
			this.GetOptionDataCache(index - 1).AddExhibit(exhibit, false);
			GameMaster.RevealExhibit(exhibitId);
		}
		[RuntimeCommand("optionRandomExhibit", "")]
		[UsedImplicitly]
		public void OptionRandomExhibit(int index, string exhibitId)
		{
			this.GetOptionDataCache(index - 1).AddExhibit(LBoL.Core.Library.CreateExhibit(exhibitId), true);
			GameMaster.RevealExhibit(exhibitId);
		}
		[RuntimeCommand("optionActive", "")]
		[UsedImplicitly]
		public void OptionActive(int index, bool active)
		{
			this.GetOptionDataCache(index - 1).IsActive = active;
		}
		[RuntimeCommand("optionTooltip", "")]
		[UsedImplicitly]
		public void OptionTooltip(int index, string title, string content)
		{
			this.GetOptionDataCache(index - 1).AddTooltip(title, content);
		}
		[RuntimeCommand("showUsInSupply", "")]
		[UsedImplicitly]
		public void SowUsInSupply(bool hasPowerExhibit)
		{
			if (hasPowerExhibit)
			{
				UiManager.GetPanel<UltimateSkillPanel>().ShowInDialog();
				return;
			}
			UiManager.GetPanel<UltimateSkillPanel>().HideInDialog();
		}
		[RuntimeCommand("sfx", "")]
		[UsedImplicitly]
		public void PlaySfx(string audioName)
		{
			AudioManager.PlaySfx(audioName, -1f);
		}
		[RuntimeCommand("uiSound", "")]
		[UsedImplicitly]
		public void PlayUiSound(string audioName)
		{
			AudioManager.PlayUi(audioName, false);
		}
		[RuntimeCommand("shake", "")]
		[UsedImplicitly]
		public void Shake(int shakeLevel = 0)
		{
			GameDirector.ShakeUi(shakeLevel);
		}
		[RuntimeCommand("setAdventureImage", "")]
		[UsedImplicitly]
		public void SetAdventureImage(string suffix, int slotIndex = 0, float alpha = 1f)
		{
			if (this._currentAdventure == null)
			{
				Debug.Log("Cannot set adventure image without currentAdventureName, call <<setAdventure>> first");
				return;
			}
			VnPanel.AdvSlot advSlot;
			if (!this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
				return;
			}
			this.adventureFrame.gameObject.SetActive(true);
			this.adventureTitle.gameObject.SetActive(true);
			advSlot.SetImage(true, this._currentAdventure.Id + suffix, alpha, slotIndex == 4);
			advSlot.IsActive = true;
		}
		[RuntimeCommand("setAdventureImageAlpha", "")]
		[UsedImplicitly]
		public void SetAdventureImageAlpha(float alpha, int slotIndex = 0, bool front = true)
		{
			VnPanel.AdvSlot advSlot;
			if (this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				advSlot.SetAlpha(front, alpha);
				return;
			}
			Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
		}
		[RuntimeCommand("fadeAdventureImageAlpha", "")]
		[UsedImplicitly]
		public IEnumerator FadeAdventureImageAlpha(float alpha, float time, int slotIndex = 0, bool front = true)
		{
			VnPanel.AdvSlot advSlot;
			if (this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				yield return advSlot.FadeAsync(front, alpha, time);
			}
			else
			{
				Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
			}
			yield break;
		}
		[RuntimeCommand("setAdventureImageScale", "")]
		[UsedImplicitly]
		public void SetAdventureImageScale(float scale, int slotIndex = 0, bool front = true)
		{
			VnPanel.AdvSlot advSlot;
			if (this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				advSlot.SetScale(front, scale);
				return;
			}
			Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
		}
		[RuntimeCommand("zoomAdventureImage", "")]
		[UsedImplicitly]
		public IEnumerator ZoomAdventureImage(float scale, float time, int slotIndex = 0, bool front = true)
		{
			VnPanel.AdvSlot advSlot;
			if (this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				yield return advSlot.ZoomAsync(front, scale, time);
			}
			else
			{
				Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
			}
			yield break;
		}
		[RuntimeCommand("crossfadeAdventureImage", "")]
		[UsedImplicitly]
		public IEnumerator CrossfadeAdventureImage(string suffix, float time, int slotIndex = 0)
		{
			if (this._currentAdventure == null)
			{
				Debug.LogError("Cannot set adventure image while current adventure is null");
				yield break;
			}
			VnPanel.AdvSlot advSlot;
			if (this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				yield return advSlot.CrossfadeImage(this._currentAdventure.Id + suffix, time);
			}
			else
			{
				Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
			}
			yield break;
		}
		[RuntimeCommand("clearAdventureImage", "")]
		[UsedImplicitly]
		public void ClearAdventureImage(int slotIndex = 0)
		{
			VnPanel.AdvSlot advSlot;
			if (this._advSlots.TryGetValue(slotIndex, out advSlot))
			{
				advSlot.Clear();
				return;
			}
			Debug.LogError(string.Format("Cannot find adventure slot {0}", slotIndex));
		}
		[RuntimeCommand("showPlayer", "")]
		[UsedImplicitly]
		public void ShowPlayer(string slotName, string defaultSprite = "默认")
		{
			this.ShowCharacter(slotName, base.GameRun.Player, defaultSprite);
		}
		[RuntimeCommand("showEnemy", "")]
		[UsedImplicitly]
		public void ShowEnemy(string slotName, string enemyId, string defaultSprite = "默认")
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			this.ShowCharacter(slotName, enemyUnit, defaultSprite);
		}
		[RuntimeCommand("hide", "")]
		[UsedImplicitly]
		public IEnumerator Hide(string slotName)
		{
			return this.HideCharacter(slotName);
		}
		[RuntimeCommand("l", "")]
		[UsedImplicitly]
		public IEnumerator HighlightL(string spriteName = null)
		{
			return this.HighlightSlots(new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("l", spriteName)
			});
		}
		[RuntimeCommand("r", "")]
		[UsedImplicitly]
		public IEnumerator HighlightR(string spriteName = null)
		{
			return this.HighlightSlots(new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("r", spriteName)
			});
		}
		[RuntimeCommand("highlight", "")]
		[UsedImplicitly]
		public IEnumerator Highlight(params string[] slotsAndSprites)
		{
			ValueTuple<string, string>[] array = new ValueTuple<string, string>[slotsAndSprites.Length];
			for (int i = 0; i < slotsAndSprites.Length; i++)
			{
				VnPanel.<>c__DisplayClass164_0 CS$<>8__locals1 = new VnPanel.<>c__DisplayClass164_0();
				string text = slotsAndSprites[i];
				int num = text.IndexOf(':');
				VnPanel.<>c__DisplayClass164_0 CS$<>8__locals2 = CS$<>8__locals1;
				ValueTuple<string, string> valueTuple;
				if (num < 0)
				{
					valueTuple = new ValueTuple<string, string>(text, null);
				}
				else
				{
					string text2 = text.Substring(0, num);
					string text3 = text;
					int num2 = num + 1;
					valueTuple = new ValueTuple<string, string>(text2, text3.Substring(num2, text3.Length - num2));
				}
				CS$<>8__locals2.pair = valueTuple;
				if (Enumerable.Any<ValueTuple<string, string>>(array, ([TupleElementNames(new string[] { "slot", "sprite" })] ValueTuple<string, string> p) => p.Item1 == CS$<>8__locals1.pair.Item1))
				{
					throw new ArgumentException("Duplicated slot name: " + CS$<>8__locals1.pair.Item1);
				}
				array[i] = CS$<>8__locals1.pair;
			}
			return this.HighlightSlots(array);
		}
		[RuntimeCommand("lPlayerName", "")]
		[UsedImplicitly]
		public void LeftPlayerName()
		{
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(base.GameRun.Player, true);
		}
		[RuntimeCommand("rPlayerName", "")]
		[UsedImplicitly]
		public void RightPlayerName()
		{
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(base.GameRun.Player, false);
		}
		[RuntimeCommand("lEnemyName", "")]
		[UsedImplicitly]
		public void LeftEnemyName(string enemyId)
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(enemyUnit, true);
		}
		[RuntimeCommand("rEnemyName", "")]
		[UsedImplicitly]
		public void RightEnemyName(string enemyId)
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(enemyUnit, false);
		}
		[RuntimeCommand("enemyTitle", "")]
		[UsedImplicitly]
		public void EnemyTitle(string enemyId)
		{
			this.enemyTitleRoot.transform.localPosition = new Vector3(-650f, 400f);
			this.EnemyTitleHandler(enemyId);
		}
		[RuntimeCommand("enemyTitleR", "")]
		[UsedImplicitly]
		public void EnemyTitleR(string enemyId)
		{
			this.enemyTitleRoot.transform.localPosition = new Vector3(1000f, -440f);
			this.EnemyTitleHandler(enemyId);
		}
		[RuntimeCommand("playFinalStageEffect", "")]
		[UsedImplicitly]
		public void PlayFinalStageEffect()
		{
			Environment.PlayFinalStageEffect();
		}
		private void EnemyTitleHandler(string enemyId)
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			base.StartCoroutine(this.ShowEnemyTitle(enemyUnit.FullName, enemyUnit.Title));
		}
		[RuntimeCommand("enemyAppear", "")]
		[UsedImplicitly]
		public IEnumerator CharacterEnter(int index, float time = 1f)
		{
			GameDirector.RevealEnemy(index, false);
			GameDirector.EnemyDebutAnimation(index);
			yield return new WaitForSeconds(time);
			yield break;
		}
		[RuntimeCommand("stopBgm", "")]
		[UsedImplicitly]
		public void StopBgm()
		{
			AudioManager.FadeOutBgm(1f);
		}
		[RuntimeCommand("bgm", "")]
		[UsedImplicitly]
		public void PlayBgm(string bgmName)
		{
			AudioManager.PlayBgm(bgmName, 0f, false);
		}
		[RuntimeCommand("bgmStage", "")]
		[UsedImplicitly]
		public void PlayStageBgm()
		{
			AudioManager.EnterLayer0();
		}
		[RuntimeCommand("bgmElite", "")]
		[UsedImplicitly]
		public void PlayEliteBgm()
		{
			AudioManager.PlayEliteBgm(null);
		}
		[RuntimeCommand("bgmBoss", "")]
		[UsedImplicitly]
		public void PlayBossBgm(string bgmId)
		{
			AudioManager.PlayBossBgm(bgmId);
		}
		[RuntimeCommand("battle", "")]
		[UsedImplicitly]
		public IEnumerator RunBattle(string enemyGroupName, bool reopenVnPanel = true)
		{
			yield return UiManager.ShowLoading(0.1f).ToCoroutine(null);
			EnemyGroup group = LBoL.Core.Library.GenerateEnemyGroup(base.GameRun, enemyGroupName);
			GameDirector.MovePlayer(group.PlayerRootV2);
			foreach (EnemyUnit enemyUnit in group)
			{
				yield return GameDirector.LoadEnemyAsync(enemyUnit, group.FormationName, false, default(int?)).ToCoroutine(null, null);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			this.root.SetActive(false);
			UiManager.Hide<BackgroundPanel>(true);
			yield return UiManager.HideLoading(0.1f).ToCoroutine(null);
			yield return GameMaster.BattleFlow(group);
			GameDirector.ClearEnemies();
			if (reopenVnPanel)
			{
				this.root.SetActive(true);
			}
			yield break;
			yield break;
		}
		[RuntimeCommand("setVisible", "")]
		[UsedImplicitly]
		public void SetVisible(bool visible)
		{
			this.root.SetActive(visible);
		}
		[RuntimeCommand("showRewards", "")]
		[UsedImplicitly]
		public IEnumerator ShowRewards()
		{
			Station currentStation = base.GameRun.CurrentStation;
			RewardPanel rewardPanel = UiManager.GetPanel<RewardPanel>();
			rewardPanel.Show(new ShowRewardContent
			{
				Station = currentStation,
				ShowNextButton = true
			});
			yield return new WaitWhile(() => rewardPanel.IsVisible);
			this.SetNextButton(false, default(int?), null);
			yield break;
		}
		[RuntimeCommand("hideBackground", "")]
		[UsedImplicitly]
		public void HideBackground()
		{
			UiManager.Hide<BackgroundPanel>(true);
		}
		private bool TryHandleSpecialCommand(string command)
		{
			string text = command.TrimStart();
			if (text.StartsWith("log ", 4))
			{
				string text2 = text;
				Debug.Log(text2.Substring(4, text2.Length - 4));
				return true;
			}
			return false;
		}
		[RuntimeCommand("gainPower", "")]
		[UsedImplicitly]
		public void GainPower(int power)
		{
			base.GameRun.GainPower(power, false);
		}
		public void ResetComic()
		{
			VnPanel.SimpleSlot simpleSlot = this._simpleSlots["bg"];
			simpleSlot.IsActive = true;
			Image image = simpleSlot.Image;
			image.sprite = null;
			image.color = Color.white;
			image.DOFade(1f, 0.1f).From(0f, true, false);
			this._comicPhase = 0;
			this.AllScreenMode = true;
			this.ClearAllSlotsExpectBackground();
		}
		private void ClearAllSlotsExpectBackground()
		{
			foreach (VnPanel.SimpleSlot simpleSlot in this._simpleSlots.Values)
			{
				if (simpleSlot.Name != "bg")
				{
					simpleSlot.Clear();
				}
			}
		}
		[RuntimeCommand("nextComic", "")]
		[UsedImplicitly]
		public IEnumerator NextComic()
		{
			this._activeText.text = string.Empty;
			this._comicPhase++;
			switch (this._comicPhase)
			{
			case 1:
				this.LoadComic("c1", "11", true);
				break;
			case 2:
				this.LoadComic("c2", "12", true);
				break;
			case 3:
				this.LoadComic("c3", "13", true);
				if (!this.Skipping)
				{
					yield return new WaitForSeconds(0.5f);
				}
				this.LoadComic("c4", "13a", true);
				if (!this.Skipping)
				{
					this.PlayUiSound("PosterLaunch");
				}
				break;
			case 4:
				this.LoadComic("cd", "13b", false);
				this.PosterRotateIn(this.Skipping);
				break;
			case 5:
				this.ClearAllSlotsExpectBackground();
				this.LoadComic("c1", "21", true);
				if (!this.Skipping)
				{
					yield return new WaitForSeconds(1f);
				}
				this.LoadComic("c2", "22", true);
				break;
			case 6:
				this.LoadComic("c3", "23", true);
				if (!this.Skipping)
				{
					yield return new WaitForSeconds(1f);
				}
				this.LoadComic("c4", "24", true);
				if (!this.Skipping)
				{
					yield return new WaitForSeconds(1.5f);
				}
				this.LoadComic("c5", "24a", true);
				break;
			case 7:
				this.ClearAllSlotsExpectBackground();
				this.LoadComic("c1", "31", true);
				break;
			case 8:
				this.LoadComic("c2", "32", true);
				break;
			case 9:
				this.LoadComic("c3", "32a", true);
				break;
			case 10:
				this.ClearAllSlotsExpectBackground();
				this.LoadComic("c1", "41", true);
				this.LoadComic("c2", "41a", false);
				this.LoadComic("c3", "41b", true);
				this.OrbEnter(this.Skipping);
				if (!this.Skipping)
				{
					yield return new WaitForSeconds(1f);
				}
				this.LoadComic("c4", "42", true);
				break;
			case 11:
				this.LoadComic("c5", "43", true);
				this.LoadComic("c6", "43a", false);
				this.StartBranchBlink(this.Skipping);
				break;
			case 12:
				this.LoadComic("c7", "44", true);
				this.EndBranchBlink();
				break;
			case 13:
				this.ClearAllSlotsExpectBackground();
				this.LoadComic("c1", "51", true);
				break;
			case 14:
				this.LoadComic("c2", "52", true);
				if (!this.Skipping)
				{
					yield return new WaitForSeconds(1f);
				}
				this.LoadComic("c3", "53", true);
				break;
			}
			yield break;
		}
		private void LoadComic(string slotName, string comicName, bool fadeIn = true)
		{
			Sprite sprite = Resources.Load<Sprite>("Sprite/StoryComic/Opening/" + comicName);
			if (sprite == null)
			{
				Debug.LogError("Comic sprite not found: " + comicName);
			}
			bool flag = false;
			foreach (VnPanel.SimpleSlot simpleSlot in this._simpleSlots.Values)
			{
				if (simpleSlot.Name == slotName)
				{
					simpleSlot.Image.sprite = sprite;
					simpleSlot.Image.gameObject.SetActive(true);
					if (fadeIn)
					{
						simpleSlot.Image.DOFade(1f, 0.1f).From(0f, true, false);
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("Not found slot name: " + slotName);
			}
		}
		private void PosterRotateIn(bool skipping)
		{
			Image image = this._simpleSlots["cd"].Image;
			DOTween.Sequence().Join(image.transform.DOScale(1f, 0.3f).From(0.2f, true, false).SetEase(Ease.InSine)).Join(image.transform.DOLocalRotate(Vector3.zero, 0.3f, RotateMode.Fast).From(new Vector3(0f, 0f, 1000f), true, false).SetEase(Ease.InSine))
				.OnComplete(delegate
				{
					if (!skipping)
					{
						this.PlayUiSound("PosterHit");
					}
					this.simpleSlotRoot.DOShakePosition(0.2f, 200f, 20, 90f, false, true, ShakeRandomnessMode.Full);
				})
				.SetUpdate(true)
				.SetAutoKill(true);
		}
		private void OrbEnter(bool skipping)
		{
			Image image = this._simpleSlots["c2"].Image;
			image.transform.localPosition = new Vector3(350f, -280f);
			DOTween.Sequence().AppendCallback(delegate
			{
				if (!skipping)
				{
					this.PlayUiSound("OrbEnter");
				}
			}).AppendInterval(0.4f)
				.Append(image.transform.DOLocalMove(Vector3.zero, 0.4f, false).SetEase(Ease.OutCubic))
				.SetAutoKill(true)
				.SetUpdate(true);
		}
		private void StartBranchBlink(bool skipping)
		{
			Image image = this._simpleSlots["c6"].Image;
			DOTween.Sequence().Append(image.DOFade(1f, 0.1f).From(0f, true, false)).AppendCallback(delegate
			{
				if (!skipping)
				{
					this.PlayUiSound("BranchBlink");
				}
			})
				.AppendInterval(0.1f)
				.Append(image.DOFade(0f, 0.1f))
				.AppendInterval(0.1f)
				.SetLoops(3)
				.SetAutoKill(true)
				.SetUpdate(true);
		}
		private void EndBranchBlink()
		{
			this._simpleSlots["c6"].Image.gameObject.SetActive(false);
		}
		[CompilerGenerated]
		internal static void <CoRevealText>g__SetCharColors|114_0(TextMeshProUGUI text, float revealProgress, float ahead)
		{
			TMP_TextInfo textInfo = text.textInfo;
			int characterCount = textInfo.characterCount;
			float num = ((float)characterCount + ahead) * revealProgress;
			for (int i = 0; i < characterCount; i++)
			{
				TMP_CharacterInfo tmp_CharacterInfo = textInfo.characterInfo[i];
				if (tmp_CharacterInfo.isVisible)
				{
					byte b = (byte)(((num - (float)i) / ahead).Clamp01() * 255f);
					int materialReferenceIndex = tmp_CharacterInfo.materialReferenceIndex;
					int vertexIndex = tmp_CharacterInfo.vertexIndex;
					Color32[] colors = textInfo.meshInfo[materialReferenceIndex].colors32;
					colors[vertexIndex] = colors[vertexIndex].WithA(b);
					colors[vertexIndex + 1] = colors[vertexIndex + 1].WithA(b);
					colors[vertexIndex + 2] = colors[vertexIndex + 2].WithA(b);
					colors[vertexIndex + 3] = colors[vertexIndex + 3].WithA(b);
				}
			}
			text.UpdateVertexData();
		}
		[SerializeField]
		private GameObject root;
		[SerializeField]
		private GameObject textRoot;
		[SerializeField]
		private TextMeshProUGUI mainText;
		[SerializeField]
		private GameObject comicTextRoot;
		[SerializeField]
		private TextMeshProUGUI comicText;
		[SerializeField]
		private Button nextLineButton;
		[SerializeField]
		private GameObject optionsRoot;
		[SerializeField]
		private OptionWidget[] optionWidgets;
		[SerializeField]
		private Transform simpleSlotRoot;
		[SerializeField]
		private VnPanel.AdvSlotEntry[] advSlots;
		[SerializeField]
		private Transform characterSlotRoot;
		[SerializeField]
		private GameObject leftCharacterNameRoot;
		[SerializeField]
		private TextMeshProUGUI leftCharacterNameText;
		[SerializeField]
		private GameObject rightCharacterNameRoot;
		[SerializeField]
		private TextMeshProUGUI rightCharacterNameText;
		[SerializeField]
		private Image adventureFrame;
		[SerializeField]
		private GameObject adventureTitle;
		[SerializeField]
		private TextMeshProUGUI adventureTitleText;
		[SerializeField]
		private GameObject enemyTitleRoot;
		[SerializeField]
		private Image enemyTitleBack;
		[SerializeField]
		private TextMeshProUGUI enemyName;
		[SerializeField]
		private TextMeshProUGUI enemyTitle;
		[SerializeField]
		private GameObject optionSourceLayout;
		[SerializeField]
		private CardWidget cardWidgetTemplate;
		[SerializeField]
		private MuseumExhibitWidget exhibitWidgetTemplate;
		public Button nextButton;
		[SerializeField]
		private TextMeshProUGUI nextButtonText;
		[SerializeField]
		private Image skipIcon;
		[SerializeField]
		private TextMeshProUGUI tempArtText;
		private readonly List<CardWidget> _cardWidgets = new List<CardWidget>();
		private readonly List<MuseumExhibitWidget> _exhibitWidgets = new List<MuseumExhibitWidget>();
		private GameObject _activeTextRoot;
		private TextMeshProUGUI _activeText;
		private bool _allScreenMode;
		private string _nextString;
		private string _nextStageString;
		private string _skipString;
		private string _skipShopString;
		private bool _canSkipAll;
		private bool _allSkipping;
		private VnExtraSettings _extraSettings;
		private DialogStorage _storage;
		private readonly List<SimpleTooltipSource> _optionTooltipList = new List<SimpleTooltipSource>();
		private Action _nextButtonHandler;
		private readonly Vector2 _debutSize = new Vector2(3840f, 800f);
		private const float RevealMaxTime = 30f;
		private CanvasGroup _canvasGroup;
		private RuntimeCommandHandler _commandHandler;
		private readonly Dictionary<string, VnPanel.ProtraitSlot> _characterSlots = new Dictionary<string, VnPanel.ProtraitSlot>();
		private readonly Dictionary<string, VnPanel.SimpleSlot> _simpleSlots = new Dictionary<string, VnPanel.SimpleSlot>();
		private readonly List<VnPanel.AdvSlot> _advSlots = new List<VnPanel.AdvSlot>();
		private DialogRunner _dialogRunner;
		private DialogLinePhase _currentLineShowing;
		private Adventure _currentAdventure;
		private bool _shouldShowNextLine;
		private float _totalRevealTime;
		private float _currentRevealTime;
		[MaybeNull]
		private string _nextLineAsVariable;
		private bool _userSkipping;
		private List<DialogOptionData> _optionDataCache;
		private Coroutine _runningCoroutine;
		private DialogOption[] _options;
		private int? _selectedOptionId;
		private bool _showNextLineInstantly;
		private bool _noWait;
		private VnPanel.NextLineCharacterName _nextLineCharacterName;
		private Unit _leftCharacterNameUnit;
		private Unit _rightCharacterNameUnit;
		private int _comicPhase;
		[Serializable]
		public class AdvSlotEntry
		{
			public RawImage front;
			public RawImage back;
		}
		private class NextLineCharacterName
		{
			public NextLineCharacterName(Unit unit, bool left)
			{
				this.Unit = unit;
				this.IsLeft = left;
			}
			public readonly Unit Unit;
			public readonly bool IsLeft;
		}
		private class ProtraitSlot
		{
			public Image Image { get; }
			public string Name { get; }
			public ProtraitSlot(Image i, string n)
			{
				this.Image = i;
				this.Name = n;
			}
			public bool IsActive
			{
				get
				{
					return this.Image.gameObject.activeSelf;
				}
				set
				{
					this.Image.gameObject.SetActive(value);
				}
			}
			public void Load(string portraitName, string defaultSprite)
			{
				this._portraits = PortraitGroup.Load(portraitName);
				this.Image.sprite = this.Get(defaultSprite);
				this.IsActive = this.Get(defaultSprite) != null;
			}
			public void Release()
			{
				this.IsActive = false;
				this.Image.sprite = null;
				PortraitGroup portraits = this._portraits;
				if (portraits != null)
				{
					portraits.Release();
				}
				this._portraits = null;
			}
			public Sprite Get(string spriteName)
			{
				return this._portraits.Get(spriteName);
			}
			private PortraitGroup _portraits;
		}
		private class SimpleSlot
		{
			public Image Image { get; }
			public string Name { get; }
			public SimpleSlot(Image image, string n)
			{
				this.Image = image;
				this.Name = n;
			}
			public bool IsActive
			{
				get
				{
					return this.Image.gameObject.activeSelf;
				}
				set
				{
					this.Image.gameObject.SetActive(value);
				}
			}
			public void Clear()
			{
				this.IsActive = false;
				this.Image.sprite = null;
			}
		}
		private class AdvSlot
		{
			public AdvSlot(RawImage fore, RawImage back)
			{
				this._foreImage = fore;
				this._backImage = back;
			}
			private RawImage GetImage(bool front)
			{
				if (!front)
				{
					return this._backImage;
				}
				return this._foreImage;
			}
			public void SetImage(bool front, string path, float alpha, bool isSlotFour = false)
			{
				if (front)
				{
					this._path = path;
					this._alpha = alpha;
					this._scale = 1f;
				}
				Texture2D texture2D = ResourcesHelper.LoadAdventureImage(path);
				RawImage image = this.GetImage(front);
				image.texture = texture2D;
				image.color = new Color(1f, 1f, 1f, alpha);
				if (isSlotFour)
				{
					image.rectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
				}
			}
			public void SetAlpha(bool front, float alpha)
			{
				if (front)
				{
					this._alpha = alpha;
				}
				this.GetImage(front).color = new Color(1f, 1f, 1f, alpha);
			}
			public IEnumerator CrossfadeImage(string path, float time)
			{
				this._path = path;
				this._alpha = 1f;
				Texture2D texture = ResourcesHelper.LoadAdventureImage(path);
				if (this.IsActive)
				{
					this._backImage.texture = texture;
					this._backImage.gameObject.SetActive(true);
					yield return this._foreImage.DOFade(0f, time).WaitForCompletion();
					this._foreImage.texture = texture;
					this._foreImage.color = Color.white;
					this._backImage.texture = null;
					this._backImage.gameObject.SetActive(false);
				}
				else
				{
					this.IsActive = true;
					this._foreImage.texture = texture;
					yield return this._foreImage.DOFade(1f, time).From(0f, true, false).WaitForCompletion();
				}
				yield break;
			}
			public YieldInstruction FadeAsync(bool front, float alpha, float time)
			{
				if (front)
				{
					this._alpha = alpha;
				}
				return this.GetImage(front).DOFade(alpha, time).WaitForCompletion();
			}
			public void SetScale(bool front, float scale)
			{
				if (front)
				{
					this._scale = scale;
				}
				this.GetImage(front).transform.localScale = new Vector3(scale, scale, scale);
			}
			public YieldInstruction ZoomAsync(bool front, float scale, float time)
			{
				if (front)
				{
					this._scale = scale;
				}
				return this.GetImage(true).transform.DOScale(scale, time).SetEase(Ease.OutQuint).WaitForCompletion();
			}
			public bool IsActive
			{
				get
				{
					return this._foreImage.gameObject.activeSelf;
				}
				set
				{
					this._foreImage.gameObject.SetActive(value);
					if (!value)
					{
						this._backImage.gameObject.SetActive(false);
					}
				}
			}
			public void Clear()
			{
				this.IsActive = false;
				this._foreImage.texture = null;
				this._foreImage.gameObject.SetActive(false);
				this._backImage.texture = null;
				this._backImage.gameObject.SetActive(false);
			}
			public AdvSlotSaveData Save()
			{
				if (this._backImage.gameObject.activeSelf)
				{
					Debug.LogError("[VnPanel] Saving while back image is active");
				}
				if (this._foreImage.gameObject.activeSelf)
				{
					return new AdvSlotSaveData
					{
						Path = this._path,
						Alpha = this._alpha,
						Scale = this._scale
					};
				}
				return null;
			}
			public void Restore(AdvSlotSaveData saveData)
			{
				if (saveData != null)
				{
					this._path = saveData.Path;
					this._alpha = saveData.Alpha;
					this._scale = saveData.Scale;
					Texture2D texture2D = ResourcesHelper.LoadAdventureImage(this._path);
					this._foreImage.texture = texture2D;
					this._foreImage.color = new Color(1f, 1f, 1f, this._alpha);
					this._foreImage.transform.localScale = new Vector3(this._scale, this._scale, this._scale);
					this._foreImage.gameObject.SetActive(true);
					return;
				}
				this.Clear();
			}
			private readonly RawImage _foreImage;
			private readonly RawImage _backImage;
			private string _path;
			private float _scale;
			private float _alpha;
		}
	}
}
