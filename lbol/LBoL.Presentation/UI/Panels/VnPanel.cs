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
	// Token: 0x020000C3 RID: 195
	public class VnPanel : UiPanel
	{
		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06000B92 RID: 2962 RVA: 0x0003C269 File Offset: 0x0003A469
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.VisualNovel;
			}
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06000B93 RID: 2963 RVA: 0x0003C26C File Offset: 0x0003A46C
		// (set) Token: 0x06000B94 RID: 2964 RVA: 0x0003C274 File Offset: 0x0003A474
		public bool IsTempLocked { get; set; }

		// Token: 0x06000B95 RID: 2965 RVA: 0x0003C280 File Offset: 0x0003A480
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

		// Token: 0x06000B96 RID: 2966 RVA: 0x0003C2C4 File Offset: 0x0003A4C4
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

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000B97 RID: 2967 RVA: 0x0003C446 File Offset: 0x0003A646
		// (set) Token: 0x06000B98 RID: 2968 RVA: 0x0003C44E File Offset: 0x0003A64E
		public int? NextButtonStringIndex { get; private set; }

		// Token: 0x06000B99 RID: 2969 RVA: 0x0003C458 File Offset: 0x0003A658
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

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000B9A RID: 2970 RVA: 0x0003C4F1 File Offset: 0x0003A6F1
		// (set) Token: 0x06000B9B RID: 2971 RVA: 0x0003C4FC File Offset: 0x0003A6FC
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

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000B9C RID: 2972 RVA: 0x0003C5D7 File Offset: 0x0003A7D7
		// (set) Token: 0x06000B9D RID: 2973 RVA: 0x0003C5DF File Offset: 0x0003A7DF
		private RectTransform NextButtonRect { get; set; }

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000B9E RID: 2974 RVA: 0x0003C5E8 File Offset: 0x0003A7E8
		// (set) Token: 0x06000B9F RID: 2975 RVA: 0x0003C5F0 File Offset: 0x0003A7F0
		private Vector2 NextButtonDefaultSize { get; set; }

		// Token: 0x06000BA0 RID: 2976 RVA: 0x0003C5F9 File Offset: 0x0003A7F9
		private void SetUserSkipping(bool value)
		{
			this._userSkipping = value;
			this.skipIcon.gameObject.SetActive(value);
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000BA1 RID: 2977 RVA: 0x0003C613 File Offset: 0x0003A813
		private bool Skipping
		{
			get
			{
				return this._userSkipping || this._allSkipping;
			}
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x0003C628 File Offset: 0x0003A828
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

		// Token: 0x06000BA3 RID: 2979 RVA: 0x0003C900 File Offset: 0x0003AB00
		protected override void OnShowing()
		{
			GameMaster.ShowPoseAnimation = false;
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x0003C908 File Offset: 0x0003AB08
		protected override void OnHided()
		{
			GameMaster.ShowPoseAnimation = true;
			this.skipIcon.gameObject.SetActive(false);
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x0003C921 File Offset: 0x0003AB21
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

		// Token: 0x06000BA6 RID: 2982 RVA: 0x0003C950 File Offset: 0x0003AB50
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

		// Token: 0x06000BA7 RID: 2983 RVA: 0x0003C9FC File Offset: 0x0003ABFC
		public bool HandleNavigateAction(NavigateDirection dir)
		{
			return false;
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x0003CA00 File Offset: 0x0003AC00
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

		// Token: 0x06000BA9 RID: 2985 RVA: 0x0003CA85 File Offset: 0x0003AC85
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

		// Token: 0x06000BAA RID: 2986 RVA: 0x0003CAA4 File Offset: 0x0003ACA4
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

		// Token: 0x06000BAB RID: 2987 RVA: 0x0003CB01 File Offset: 0x0003AD01
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

		// Token: 0x06000BAC RID: 2988 RVA: 0x0003CB30 File Offset: 0x0003AD30
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

		// Token: 0x06000BAD RID: 2989 RVA: 0x0003CBE8 File Offset: 0x0003ADE8
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

		// Token: 0x06000BAE RID: 2990 RVA: 0x0003CC2A File Offset: 0x0003AE2A
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

		// Token: 0x06000BAF RID: 2991 RVA: 0x0003CC40 File Offset: 0x0003AE40
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

		// Token: 0x06000BB0 RID: 2992 RVA: 0x0003CC5D File Offset: 0x0003AE5D
		private IEnumerator ShowEnemyTitle(string name, string title)
		{
			return this.InternalShowEnemyTitle(name, title);
		}

		// Token: 0x06000BB1 RID: 2993 RVA: 0x0003CC67 File Offset: 0x0003AE67
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

		// Token: 0x06000BB2 RID: 2994 RVA: 0x0003CC80 File Offset: 0x0003AE80
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

		// Token: 0x06000BB3 RID: 2995 RVA: 0x0003CDB3 File Offset: 0x0003AFB3
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

		// Token: 0x06000BB4 RID: 2996 RVA: 0x0003CDD7 File Offset: 0x0003AFD7
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

		// Token: 0x06000BB5 RID: 2997 RVA: 0x0003CDF0 File Offset: 0x0003AFF0
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

		// Token: 0x06000BB6 RID: 2998 RVA: 0x0003CE37 File Offset: 0x0003B037
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

		// Token: 0x06000BB7 RID: 2999 RVA: 0x0003CE54 File Offset: 0x0003B054
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

		// Token: 0x06000BB8 RID: 3000 RVA: 0x0003CEE0 File Offset: 0x0003B0E0
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

		// Token: 0x06000BB9 RID: 3001 RVA: 0x0003CF4C File Offset: 0x0003B14C
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

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000BBA RID: 3002 RVA: 0x0003D044 File Offset: 0x0003B244
		public bool IsRunning
		{
			get
			{
				return this._runningCoroutine != null;
			}
		}

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000BBB RID: 3003 RVA: 0x0003D04F File Offset: 0x0003B24F
		public bool CanSkipAll
		{
			get
			{
				return this._canSkipAll;
			}
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x0003D057 File Offset: 0x0003B257
		public void Stop()
		{
			base.StopAllCoroutines();
			this.End();
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x0003D065 File Offset: 0x0003B265
		public void SkipAll()
		{
			if (!this._canSkipAll)
			{
				Debug.LogError("[VnPanel] Cannot skip all while not enabled");
				return;
			}
			this._allSkipping = true;
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x0003D081 File Offset: 0x0003B281
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

		// Token: 0x06000BBF RID: 3007 RVA: 0x0003D0C0 File Offset: 0x0003B2C0
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

		// Token: 0x06000BC0 RID: 3008 RVA: 0x0003D1E0 File Offset: 0x0003B3E0
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

		// Token: 0x06000BC1 RID: 3009 RVA: 0x0003D274 File Offset: 0x0003B474
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

		// Token: 0x06000BC2 RID: 3010 RVA: 0x0003D5D4 File Offset: 0x0003B7D4
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

		// Token: 0x06000BC3 RID: 3011 RVA: 0x0003D6DC File Offset: 0x0003B8DC
		[RuntimeCommand("enableSkipAll", "")]
		[UsedImplicitly]
		public void EnableSkipAll()
		{
			this._canSkipAll = true;
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x0003D6E5 File Offset: 0x0003B8E5
		[RuntimeCommand("wait", "")]
		[UsedImplicitly]
		public IEnumerator Wait(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			yield break;
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x0003D6F4 File Offset: 0x0003B8F4
		[RuntimeCommand("jump", "")]
		[UsedImplicitly]
		public void Jump(string nodeName)
		{
			this._dialogRunner.Jump(nodeName);
		}

		// Token: 0x06000BC6 RID: 3014 RVA: 0x0003D702 File Offset: 0x0003B902
		[RuntimeCommand("saveAndJump", "")]
		[UsedImplicitly]
		public void SaveAndJump(string nodeName)
		{
			GameMaster.RequestSaveGameRunInAdventure(this.SaveAdventureState(nodeName));
			this._dialogRunner.Jump(nodeName);
		}

		// Token: 0x06000BC7 RID: 3015 RVA: 0x0003D71C File Offset: 0x0003B91C
		[RuntimeCommand("varLine", "")]
		[UsedImplicitly]
		public void VarLine(string varName)
		{
			this._nextLineAsVariable = "$" + varName;
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x0003D72F File Offset: 0x0003B92F
		[RuntimeCommand("showInstantly", "")]
		[UsedImplicitly]
		public void ShowInstantly()
		{
			this._showNextLineInstantly = true;
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x0003D738 File Offset: 0x0003B938
		[RuntimeCommand("noWait", "")]
		[UsedImplicitly]
		public void NoWait()
		{
			this._noWait = true;
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x0003D744 File Offset: 0x0003B944
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

		// Token: 0x06000BCB RID: 3019 RVA: 0x0003D79C File Offset: 0x0003B99C
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

		// Token: 0x06000BCC RID: 3020 RVA: 0x0003D7E0 File Offset: 0x0003B9E0
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

		// Token: 0x06000BCD RID: 3021 RVA: 0x0003D82C File Offset: 0x0003BA2C
		[RuntimeCommand("optionRandomExhibit", "")]
		[UsedImplicitly]
		public void OptionRandomExhibit(int index, string exhibitId)
		{
			this.GetOptionDataCache(index - 1).AddExhibit(LBoL.Core.Library.CreateExhibit(exhibitId), true);
			GameMaster.RevealExhibit(exhibitId);
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x0003D849 File Offset: 0x0003BA49
		[RuntimeCommand("optionActive", "")]
		[UsedImplicitly]
		public void OptionActive(int index, bool active)
		{
			this.GetOptionDataCache(index - 1).IsActive = active;
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x0003D85A File Offset: 0x0003BA5A
		[RuntimeCommand("optionTooltip", "")]
		[UsedImplicitly]
		public void OptionTooltip(int index, string title, string content)
		{
			this.GetOptionDataCache(index - 1).AddTooltip(title, content);
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x0003D86C File Offset: 0x0003BA6C
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

		// Token: 0x06000BD1 RID: 3025 RVA: 0x0003D886 File Offset: 0x0003BA86
		[RuntimeCommand("sfx", "")]
		[UsedImplicitly]
		public void PlaySfx(string audioName)
		{
			AudioManager.PlaySfx(audioName, -1f);
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x0003D893 File Offset: 0x0003BA93
		[RuntimeCommand("uiSound", "")]
		[UsedImplicitly]
		public void PlayUiSound(string audioName)
		{
			AudioManager.PlayUi(audioName, false);
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x0003D89C File Offset: 0x0003BA9C
		[RuntimeCommand("shake", "")]
		[UsedImplicitly]
		public void Shake(int shakeLevel = 0)
		{
			GameDirector.ShakeUi(shakeLevel);
		}

		// Token: 0x06000BD4 RID: 3028 RVA: 0x0003D8A4 File Offset: 0x0003BAA4
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

		// Token: 0x06000BD5 RID: 3029 RVA: 0x0003D930 File Offset: 0x0003BB30
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

		// Token: 0x06000BD6 RID: 3030 RVA: 0x0003D96B File Offset: 0x0003BB6B
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

		// Token: 0x06000BD7 RID: 3031 RVA: 0x0003D998 File Offset: 0x0003BB98
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

		// Token: 0x06000BD8 RID: 3032 RVA: 0x0003D9D3 File Offset: 0x0003BBD3
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

		// Token: 0x06000BD9 RID: 3033 RVA: 0x0003D9FF File Offset: 0x0003BBFF
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

		// Token: 0x06000BDA RID: 3034 RVA: 0x0003DA24 File Offset: 0x0003BC24
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

		// Token: 0x06000BDB RID: 3035 RVA: 0x0003DA5D File Offset: 0x0003BC5D
		[RuntimeCommand("showPlayer", "")]
		[UsedImplicitly]
		public void ShowPlayer(string slotName, string defaultSprite = "默认")
		{
			this.ShowCharacter(slotName, base.GameRun.Player, defaultSprite);
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x0003DA74 File Offset: 0x0003BC74
		[RuntimeCommand("showEnemy", "")]
		[UsedImplicitly]
		public void ShowEnemy(string slotName, string enemyId, string defaultSprite = "默认")
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			this.ShowCharacter(slotName, enemyUnit, defaultSprite);
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x0003DA91 File Offset: 0x0003BC91
		[RuntimeCommand("hide", "")]
		[UsedImplicitly]
		public IEnumerator Hide(string slotName)
		{
			return this.HideCharacter(slotName);
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x0003DA9A File Offset: 0x0003BC9A
		[RuntimeCommand("l", "")]
		[UsedImplicitly]
		public IEnumerator HighlightL(string spriteName = null)
		{
			return this.HighlightSlots(new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("l", spriteName)
			});
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x0003DABA File Offset: 0x0003BCBA
		[RuntimeCommand("r", "")]
		[UsedImplicitly]
		public IEnumerator HighlightR(string spriteName = null)
		{
			return this.HighlightSlots(new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("r", spriteName)
			});
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x0003DADC File Offset: 0x0003BCDC
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

		// Token: 0x06000BE1 RID: 3041 RVA: 0x0003DB9B File Offset: 0x0003BD9B
		[RuntimeCommand("lPlayerName", "")]
		[UsedImplicitly]
		public void LeftPlayerName()
		{
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(base.GameRun.Player, true);
		}

		// Token: 0x06000BE2 RID: 3042 RVA: 0x0003DBB4 File Offset: 0x0003BDB4
		[RuntimeCommand("rPlayerName", "")]
		[UsedImplicitly]
		public void RightPlayerName()
		{
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(base.GameRun.Player, false);
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x0003DBD0 File Offset: 0x0003BDD0
		[RuntimeCommand("lEnemyName", "")]
		[UsedImplicitly]
		public void LeftEnemyName(string enemyId)
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(enemyUnit, true);
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x0003DBF4 File Offset: 0x0003BDF4
		[RuntimeCommand("rEnemyName", "")]
		[UsedImplicitly]
		public void RightEnemyName(string enemyId)
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			this._nextLineCharacterName = new VnPanel.NextLineCharacterName(enemyUnit, false);
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x0003DC15 File Offset: 0x0003BE15
		[RuntimeCommand("enemyTitle", "")]
		[UsedImplicitly]
		public void EnemyTitle(string enemyId)
		{
			this.enemyTitleRoot.transform.localPosition = new Vector3(-650f, 400f);
			this.EnemyTitleHandler(enemyId);
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x0003DC3D File Offset: 0x0003BE3D
		[RuntimeCommand("enemyTitleR", "")]
		[UsedImplicitly]
		public void EnemyTitleR(string enemyId)
		{
			this.enemyTitleRoot.transform.localPosition = new Vector3(1000f, -440f);
			this.EnemyTitleHandler(enemyId);
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x0003DC65 File Offset: 0x0003BE65
		[RuntimeCommand("playFinalStageEffect", "")]
		[UsedImplicitly]
		public void PlayFinalStageEffect()
		{
			Environment.PlayFinalStageEffect();
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x0003DC6C File Offset: 0x0003BE6C
		private void EnemyTitleHandler(string enemyId)
		{
			EnemyUnit enemyUnit = LBoL.Core.Library.CreateEnemyUnit(enemyId);
			base.StartCoroutine(this.ShowEnemyTitle(enemyUnit.FullName, enemyUnit.Title));
		}

		// Token: 0x06000BE9 RID: 3049 RVA: 0x0003DC99 File Offset: 0x0003BE99
		[RuntimeCommand("enemyAppear", "")]
		[UsedImplicitly]
		public IEnumerator CharacterEnter(int index, float time = 1f)
		{
			GameDirector.RevealEnemy(index, false);
			GameDirector.EnemyDebutAnimation(index);
			yield return new WaitForSeconds(time);
			yield break;
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x0003DCAF File Offset: 0x0003BEAF
		[RuntimeCommand("stopBgm", "")]
		[UsedImplicitly]
		public void StopBgm()
		{
			AudioManager.FadeOutBgm(1f);
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x0003DCBB File Offset: 0x0003BEBB
		[RuntimeCommand("bgm", "")]
		[UsedImplicitly]
		public void PlayBgm(string bgmName)
		{
			AudioManager.PlayBgm(bgmName, 0f, false);
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x0003DCC9 File Offset: 0x0003BEC9
		[RuntimeCommand("bgmStage", "")]
		[UsedImplicitly]
		public void PlayStageBgm()
		{
			AudioManager.EnterLayer0();
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x0003DCD0 File Offset: 0x0003BED0
		[RuntimeCommand("bgmElite", "")]
		[UsedImplicitly]
		public void PlayEliteBgm()
		{
			AudioManager.PlayEliteBgm(null);
		}

		// Token: 0x06000BEE RID: 3054 RVA: 0x0003DCD8 File Offset: 0x0003BED8
		[RuntimeCommand("bgmBoss", "")]
		[UsedImplicitly]
		public void PlayBossBgm(string bgmId)
		{
			AudioManager.PlayBossBgm(bgmId);
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x0003DCE0 File Offset: 0x0003BEE0
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

		// Token: 0x06000BF0 RID: 3056 RVA: 0x0003DCFD File Offset: 0x0003BEFD
		[RuntimeCommand("setVisible", "")]
		[UsedImplicitly]
		public void SetVisible(bool visible)
		{
			this.root.SetActive(visible);
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x0003DD0B File Offset: 0x0003BF0B
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

		// Token: 0x06000BF2 RID: 3058 RVA: 0x0003DD1A File Offset: 0x0003BF1A
		[RuntimeCommand("hideBackground", "")]
		[UsedImplicitly]
		public void HideBackground()
		{
			UiManager.Hide<BackgroundPanel>(true);
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x0003DD24 File Offset: 0x0003BF24
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

		// Token: 0x06000BF4 RID: 3060 RVA: 0x0003DD5F File Offset: 0x0003BF5F
		[RuntimeCommand("gainPower", "")]
		[UsedImplicitly]
		public void GainPower(int power)
		{
			base.GameRun.GainPower(power, false);
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x0003DD70 File Offset: 0x0003BF70
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

		// Token: 0x06000BF6 RID: 3062 RVA: 0x0003DDDC File Offset: 0x0003BFDC
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

		// Token: 0x06000BF7 RID: 3063 RVA: 0x0003DE48 File Offset: 0x0003C048
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

		// Token: 0x06000BF8 RID: 3064 RVA: 0x0003DE58 File Offset: 0x0003C058
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

		// Token: 0x06000BF9 RID: 3065 RVA: 0x0003DF3C File Offset: 0x0003C13C
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

		// Token: 0x06000BFA RID: 3066 RVA: 0x0003E000 File Offset: 0x0003C200
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

		// Token: 0x06000BFB RID: 3067 RVA: 0x0003E0A0 File Offset: 0x0003C2A0
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

		// Token: 0x06000BFC RID: 3068 RVA: 0x0003E14A File Offset: 0x0003C34A
		private void EndBranchBlink()
		{
			this._simpleSlots["c6"].Image.gameObject.SetActive(false);
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x0003E1F0 File Offset: 0x0003C3F0
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

		// Token: 0x0400090E RID: 2318
		[SerializeField]
		private GameObject root;

		// Token: 0x0400090F RID: 2319
		[SerializeField]
		private GameObject textRoot;

		// Token: 0x04000910 RID: 2320
		[SerializeField]
		private TextMeshProUGUI mainText;

		// Token: 0x04000911 RID: 2321
		[SerializeField]
		private GameObject comicTextRoot;

		// Token: 0x04000912 RID: 2322
		[SerializeField]
		private TextMeshProUGUI comicText;

		// Token: 0x04000913 RID: 2323
		[SerializeField]
		private Button nextLineButton;

		// Token: 0x04000914 RID: 2324
		[SerializeField]
		private GameObject optionsRoot;

		// Token: 0x04000915 RID: 2325
		[SerializeField]
		private OptionWidget[] optionWidgets;

		// Token: 0x04000916 RID: 2326
		[SerializeField]
		private Transform simpleSlotRoot;

		// Token: 0x04000917 RID: 2327
		[SerializeField]
		private VnPanel.AdvSlotEntry[] advSlots;

		// Token: 0x04000918 RID: 2328
		[SerializeField]
		private Transform characterSlotRoot;

		// Token: 0x04000919 RID: 2329
		[SerializeField]
		private GameObject leftCharacterNameRoot;

		// Token: 0x0400091A RID: 2330
		[SerializeField]
		private TextMeshProUGUI leftCharacterNameText;

		// Token: 0x0400091B RID: 2331
		[SerializeField]
		private GameObject rightCharacterNameRoot;

		// Token: 0x0400091C RID: 2332
		[SerializeField]
		private TextMeshProUGUI rightCharacterNameText;

		// Token: 0x0400091D RID: 2333
		[SerializeField]
		private Image adventureFrame;

		// Token: 0x0400091E RID: 2334
		[SerializeField]
		private GameObject adventureTitle;

		// Token: 0x0400091F RID: 2335
		[SerializeField]
		private TextMeshProUGUI adventureTitleText;

		// Token: 0x04000920 RID: 2336
		[SerializeField]
		private GameObject enemyTitleRoot;

		// Token: 0x04000921 RID: 2337
		[SerializeField]
		private Image enemyTitleBack;

		// Token: 0x04000922 RID: 2338
		[SerializeField]
		private TextMeshProUGUI enemyName;

		// Token: 0x04000923 RID: 2339
		[SerializeField]
		private TextMeshProUGUI enemyTitle;

		// Token: 0x04000924 RID: 2340
		[SerializeField]
		private GameObject optionSourceLayout;

		// Token: 0x04000925 RID: 2341
		[SerializeField]
		private CardWidget cardWidgetTemplate;

		// Token: 0x04000926 RID: 2342
		[SerializeField]
		private MuseumExhibitWidget exhibitWidgetTemplate;

		// Token: 0x04000927 RID: 2343
		public Button nextButton;

		// Token: 0x04000928 RID: 2344
		[SerializeField]
		private TextMeshProUGUI nextButtonText;

		// Token: 0x04000929 RID: 2345
		[SerializeField]
		private Image skipIcon;

		// Token: 0x0400092A RID: 2346
		[SerializeField]
		private TextMeshProUGUI tempArtText;

		// Token: 0x0400092B RID: 2347
		private readonly List<CardWidget> _cardWidgets = new List<CardWidget>();

		// Token: 0x0400092C RID: 2348
		private readonly List<MuseumExhibitWidget> _exhibitWidgets = new List<MuseumExhibitWidget>();

		// Token: 0x0400092D RID: 2349
		private GameObject _activeTextRoot;

		// Token: 0x0400092E RID: 2350
		private TextMeshProUGUI _activeText;

		// Token: 0x0400092F RID: 2351
		private bool _allScreenMode;

		// Token: 0x04000930 RID: 2352
		private string _nextString;

		// Token: 0x04000931 RID: 2353
		private string _nextStageString;

		// Token: 0x04000932 RID: 2354
		private string _skipString;

		// Token: 0x04000933 RID: 2355
		private string _skipShopString;

		// Token: 0x04000934 RID: 2356
		private bool _canSkipAll;

		// Token: 0x04000935 RID: 2357
		private bool _allSkipping;

		// Token: 0x04000936 RID: 2358
		private VnExtraSettings _extraSettings;

		// Token: 0x04000937 RID: 2359
		private DialogStorage _storage;

		// Token: 0x04000939 RID: 2361
		private readonly List<SimpleTooltipSource> _optionTooltipList = new List<SimpleTooltipSource>();

		// Token: 0x0400093A RID: 2362
		private Action _nextButtonHandler;

		// Token: 0x0400093E RID: 2366
		private readonly Vector2 _debutSize = new Vector2(3840f, 800f);

		// Token: 0x0400093F RID: 2367
		private const float RevealMaxTime = 30f;

		// Token: 0x04000940 RID: 2368
		private CanvasGroup _canvasGroup;

		// Token: 0x04000941 RID: 2369
		private RuntimeCommandHandler _commandHandler;

		// Token: 0x04000942 RID: 2370
		private readonly Dictionary<string, VnPanel.ProtraitSlot> _characterSlots = new Dictionary<string, VnPanel.ProtraitSlot>();

		// Token: 0x04000943 RID: 2371
		private readonly Dictionary<string, VnPanel.SimpleSlot> _simpleSlots = new Dictionary<string, VnPanel.SimpleSlot>();

		// Token: 0x04000944 RID: 2372
		private readonly List<VnPanel.AdvSlot> _advSlots = new List<VnPanel.AdvSlot>();

		// Token: 0x04000945 RID: 2373
		private DialogRunner _dialogRunner;

		// Token: 0x04000946 RID: 2374
		private DialogLinePhase _currentLineShowing;

		// Token: 0x04000947 RID: 2375
		private Adventure _currentAdventure;

		// Token: 0x04000948 RID: 2376
		private bool _shouldShowNextLine;

		// Token: 0x04000949 RID: 2377
		private float _totalRevealTime;

		// Token: 0x0400094A RID: 2378
		private float _currentRevealTime;

		// Token: 0x0400094B RID: 2379
		[MaybeNull]
		private string _nextLineAsVariable;

		// Token: 0x0400094C RID: 2380
		private bool _userSkipping;

		// Token: 0x0400094D RID: 2381
		private List<DialogOptionData> _optionDataCache;

		// Token: 0x0400094E RID: 2382
		private Coroutine _runningCoroutine;

		// Token: 0x0400094F RID: 2383
		private DialogOption[] _options;

		// Token: 0x04000950 RID: 2384
		private int? _selectedOptionId;

		// Token: 0x04000951 RID: 2385
		private bool _showNextLineInstantly;

		// Token: 0x04000952 RID: 2386
		private bool _noWait;

		// Token: 0x04000953 RID: 2387
		private VnPanel.NextLineCharacterName _nextLineCharacterName;

		// Token: 0x04000954 RID: 2388
		private Unit _leftCharacterNameUnit;

		// Token: 0x04000955 RID: 2389
		private Unit _rightCharacterNameUnit;

		// Token: 0x04000956 RID: 2390
		private int _comicPhase;

		// Token: 0x020002EE RID: 750
		[Serializable]
		public class AdvSlotEntry
		{
			// Token: 0x040012C1 RID: 4801
			public RawImage front;

			// Token: 0x040012C2 RID: 4802
			public RawImage back;
		}

		// Token: 0x020002EF RID: 751
		private class NextLineCharacterName
		{
			// Token: 0x060017A7 RID: 6055 RVA: 0x00068BB4 File Offset: 0x00066DB4
			public NextLineCharacterName(Unit unit, bool left)
			{
				this.Unit = unit;
				this.IsLeft = left;
			}

			// Token: 0x040012C3 RID: 4803
			public readonly Unit Unit;

			// Token: 0x040012C4 RID: 4804
			public readonly bool IsLeft;
		}

		// Token: 0x020002F0 RID: 752
		private class ProtraitSlot
		{
			// Token: 0x170004A2 RID: 1186
			// (get) Token: 0x060017A8 RID: 6056 RVA: 0x00068BD9 File Offset: 0x00066DD9
			public Image Image { get; }

			// Token: 0x170004A3 RID: 1187
			// (get) Token: 0x060017A9 RID: 6057 RVA: 0x00068BE1 File Offset: 0x00066DE1
			public string Name { get; }

			// Token: 0x060017AA RID: 6058 RVA: 0x00068BE9 File Offset: 0x00066DE9
			public ProtraitSlot(Image i, string n)
			{
				this.Image = i;
				this.Name = n;
			}

			// Token: 0x170004A4 RID: 1188
			// (get) Token: 0x060017AB RID: 6059 RVA: 0x00068BFF File Offset: 0x00066DFF
			// (set) Token: 0x060017AC RID: 6060 RVA: 0x00068C11 File Offset: 0x00066E11
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

			// Token: 0x060017AD RID: 6061 RVA: 0x00068C24 File Offset: 0x00066E24
			public void Load(string portraitName, string defaultSprite)
			{
				this._portraits = PortraitGroup.Load(portraitName);
				this.Image.sprite = this.Get(defaultSprite);
				this.IsActive = this.Get(defaultSprite) != null;
			}

			// Token: 0x060017AE RID: 6062 RVA: 0x00068C57 File Offset: 0x00066E57
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

			// Token: 0x060017AF RID: 6063 RVA: 0x00068C84 File Offset: 0x00066E84
			public Sprite Get(string spriteName)
			{
				return this._portraits.Get(spriteName);
			}

			// Token: 0x040012C7 RID: 4807
			private PortraitGroup _portraits;
		}

		// Token: 0x020002F1 RID: 753
		private class SimpleSlot
		{
			// Token: 0x170004A5 RID: 1189
			// (get) Token: 0x060017B0 RID: 6064 RVA: 0x00068C92 File Offset: 0x00066E92
			public Image Image { get; }

			// Token: 0x170004A6 RID: 1190
			// (get) Token: 0x060017B1 RID: 6065 RVA: 0x00068C9A File Offset: 0x00066E9A
			public string Name { get; }

			// Token: 0x060017B2 RID: 6066 RVA: 0x00068CA2 File Offset: 0x00066EA2
			public SimpleSlot(Image image, string n)
			{
				this.Image = image;
				this.Name = n;
			}

			// Token: 0x170004A7 RID: 1191
			// (get) Token: 0x060017B3 RID: 6067 RVA: 0x00068CB8 File Offset: 0x00066EB8
			// (set) Token: 0x060017B4 RID: 6068 RVA: 0x00068CCA File Offset: 0x00066ECA
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

			// Token: 0x060017B5 RID: 6069 RVA: 0x00068CDD File Offset: 0x00066EDD
			public void Clear()
			{
				this.IsActive = false;
				this.Image.sprite = null;
			}
		}

		// Token: 0x020002F2 RID: 754
		private class AdvSlot
		{
			// Token: 0x060017B6 RID: 6070 RVA: 0x00068CF2 File Offset: 0x00066EF2
			public AdvSlot(RawImage fore, RawImage back)
			{
				this._foreImage = fore;
				this._backImage = back;
			}

			// Token: 0x060017B7 RID: 6071 RVA: 0x00068D08 File Offset: 0x00066F08
			private RawImage GetImage(bool front)
			{
				if (!front)
				{
					return this._backImage;
				}
				return this._foreImage;
			}

			// Token: 0x060017B8 RID: 6072 RVA: 0x00068D1C File Offset: 0x00066F1C
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

			// Token: 0x060017B9 RID: 6073 RVA: 0x00068D99 File Offset: 0x00066F99
			public void SetAlpha(bool front, float alpha)
			{
				if (front)
				{
					this._alpha = alpha;
				}
				this.GetImage(front).color = new Color(1f, 1f, 1f, alpha);
			}

			// Token: 0x060017BA RID: 6074 RVA: 0x00068DC6 File Offset: 0x00066FC6
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

			// Token: 0x060017BB RID: 6075 RVA: 0x00068DE3 File Offset: 0x00066FE3
			public YieldInstruction FadeAsync(bool front, float alpha, float time)
			{
				if (front)
				{
					this._alpha = alpha;
				}
				return this.GetImage(front).DOFade(alpha, time).WaitForCompletion();
			}

			// Token: 0x060017BC RID: 6076 RVA: 0x00068E02 File Offset: 0x00067002
			public void SetScale(bool front, float scale)
			{
				if (front)
				{
					this._scale = scale;
				}
				this.GetImage(front).transform.localScale = new Vector3(scale, scale, scale);
			}

			// Token: 0x060017BD RID: 6077 RVA: 0x00068E27 File Offset: 0x00067027
			public YieldInstruction ZoomAsync(bool front, float scale, float time)
			{
				if (front)
				{
					this._scale = scale;
				}
				return this.GetImage(true).transform.DOScale(scale, time).SetEase(Ease.OutQuint).WaitForCompletion();
			}

			// Token: 0x170004A8 RID: 1192
			// (get) Token: 0x060017BE RID: 6078 RVA: 0x00068E52 File Offset: 0x00067052
			// (set) Token: 0x060017BF RID: 6079 RVA: 0x00068E64 File Offset: 0x00067064
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

			// Token: 0x060017C0 RID: 6080 RVA: 0x00068E8C File Offset: 0x0006708C
			public void Clear()
			{
				this.IsActive = false;
				this._foreImage.texture = null;
				this._foreImage.gameObject.SetActive(false);
				this._backImage.texture = null;
				this._backImage.gameObject.SetActive(false);
			}

			// Token: 0x060017C1 RID: 6081 RVA: 0x00068EDC File Offset: 0x000670DC
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

			// Token: 0x060017C2 RID: 6082 RVA: 0x00068F44 File Offset: 0x00067144
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

			// Token: 0x040012CA RID: 4810
			private readonly RawImage _foreImage;

			// Token: 0x040012CB RID: 4811
			private readonly RawImage _backImage;

			// Token: 0x040012CC RID: 4812
			private string _path;

			// Token: 0x040012CD RID: 4813
			private float _scale;

			// Token: 0x040012CE RID: 4814
			private float _alpha;
		}
	}
}
