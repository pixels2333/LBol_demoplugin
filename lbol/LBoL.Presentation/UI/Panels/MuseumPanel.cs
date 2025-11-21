using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A4 RID: 164
	public class MuseumPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x0600089C RID: 2204 RVA: 0x00029DAC File Offset: 0x00027FAC
		public void Awake()
		{
			this._characterFilterList.Add("Neutral", this.characterToggleTemplate);
			this.characterToggleTemplate.GetComponent<CharacterToggleWidget>().SetCharacter("Tooltip.EntityTitle.Neutral".Localize(true), this.portraitList["Neutral"], true);
			foreach (PlayerUnit playerUnit in Enumerable.OrderBy<PlayerUnit, int>(Library.GetSelectablePlayers(), (PlayerUnit player) => player.Config.Order))
			{
				PlayerUnitConfig config = playerUnit.Config;
				Toggle toggle = Object.Instantiate<Toggle>(this.characterToggleTemplate, this.characterToggleLayout);
				this._characterFilterList.Add(config.Id, toggle);
				toggle.GetComponent<CharacterToggleWidget>().SetCharacter(playerUnit.Name, this.portraitList[config.Id], config.IsSelectable);
			}
			this.characterToggleTemplate.transform.SetAsLastSibling();
			List<ValueTuple<Type, ExhibitConfig>> list = Enumerable.ToList<ValueTuple<Type, ExhibitConfig>>(Enumerable.OrderBy<ValueTuple<Type, ExhibitConfig>, int>(Library.EnumerateExhibitTypes(), ([TupleElementNames(new string[] { "exhibitType", "config" })] ValueTuple<Type, ExhibitConfig> entry) => entry.Item2.Index));
			bool flag = true;
			int i;
			int j;
			for (i = 0; i < 9; i = j + 1)
			{
				List<ValueTuple<Type, ExhibitConfig>> list2 = Enumerable.ToList<ValueTuple<Type, ExhibitConfig>>(Enumerable.Where<ValueTuple<Type, ExhibitConfig>>(list, ([TupleElementNames(new string[] { "exhibitType", "config" })] ValueTuple<Type, ExhibitConfig> entry) => entry.Item2.Index / 100 == i && !entry.Item2.IsDebug));
				if (list2.Count >= 1)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(this.separatorLabelTemplate, this.exhibitsLayout);
					this._exhibitLabelDic.Add(i, gameObject);
					List<MuseumExhibitWidget> list3 = new List<MuseumExhibitWidget>();
					foreach (ValueTuple<Type, ExhibitConfig> valueTuple in list2)
					{
						Type item = valueTuple.Item1;
						MuseumExhibitWidget exhibitWidget = Object.Instantiate<MuseumExhibitWidget>(this.exhibitWidgetTemplate, this.exhibitsLayout);
						Exhibit exhibit = Library.CreateExhibit(item);
						exhibitWidget.Exhibit = exhibit;
						list3.Add(exhibitWidget);
						if (flag)
						{
							flag = false;
							exhibitWidget.exhibitWidget.gameObject.AddComponent<GamepadNavigationOrigin>();
						}
						exhibitWidget.exhibitWidget.ExhibitClicked += delegate
						{
							this.OnMuseumExhibitClick(exhibitWidget);
						};
					}
					this._exhibitWidgetDic.Add(i, list3);
				}
				j = i;
			}
			SimpleTooltipSource.CreateWithGeneralKey(this.cardStyleToggle.gameObject, "Museum.CardStyleFilter", "Museum.CardStyleTooltip");
			SimpleTooltipSource.CreateWithGeneralKey(this.revealCountRoot, "Museum.Revealed", "Museum.RevealDescription");
			this._canvasGroup = base.GetComponent<CanvasGroup>();
			this.clearTextFilter.button.onClick.AddListener(delegate
			{
				this.ClearTextFilter(true);
			});
			this.textFilter.onValueChanged.AddListener(new UnityAction<string>(this.TextFilter));
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x0002A0DC File Offset: 0x000282DC
		public bool Initialization()
		{
			if (Singleton<GameMaster>.Instance.CurrentProfile == null || this._initialized)
			{
				return false;
			}
			IEnumerable<Type> enumerable = Enumerable.Intersect<Type>(Enumerable.Intersect<Type>(this.FilterCardByMana(), this.FilterCardByType()), this.FilterCardByCharacter());
			List<Card> list = new List<Card>();
			int num = 0;
			foreach (Type type in enumerable)
			{
				Card card = Library.CreateCard(type);
				if ((GameMaster.ShowAllCardsInMuseum || card.Config.DebugLevel <= 1) && !card.Config.HideMesuem && (card.Config.DebugLevel != 1 || ResourcesHelper.TryGetCardImage(card.Id)))
				{
					if (MuseumPanel.IsCardLocked(card.Id) || !MuseumPanel.IsCardRevealed(card))
					{
						num++;
					}
					list.Add(card.CanUpgrade ? Library.CreateCard(type, this.cardUpgradedToggle.toggle.isOn) : card);
				}
			}
			list = MuseumPanel.SortCards(list);
			this.cardsScrollRect.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnCardScrollRectValueChanged));
			this.cardsScrollRect.ReloadData(new MuseumPanel.CardsRows(5, list));
			this.revealCountTmp.text = "Museum.Revealed".Localize(true) + string.Format(" {0}/{1}", list.Count - num, list.Count);
			this._lastCardData = list;
			this._lastUnrevealedCardCount = num;
			this.cardsTab.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.CardTabToggle));
			this.exhibitsTab.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.ExhibitTabToggle));
			this.achievementTab.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.AchievementTabToggle));
			this.SetDropdowns();
			this.orderDropdown.dropdown.onValueChanged.AddListener(delegate(int i)
			{
				this.CardOrder((MuseumPanel.OrderStatus)i);
			});
			this.revealDropdown.dropdown.onValueChanged.AddListener(delegate(int i)
			{
				this.CardOrder((MuseumPanel.RevealStatus)i);
			});
			this.orderSwitch.SetValueWithoutNotifier(MuseumPanel._increaseOrder, true);
			this.orderSwitch.AddListener(new UnityAction<bool>(this.CardOrder));
			this.cardUpgradedToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.RefreshCardsKeepPosition();
			});
			this.cardStyleToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.RefreshCards();
			});
			this.achievementScrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnAchievementScrollRectValueChanged));
			this.allAchievementToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.AchievementFilter(on, MuseumPanel.AchievementStatus.All);
			});
			this.unlockAchievementToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.AchievementFilter(on, MuseumPanel.AchievementStatus.Unlock);
			});
			this.notUnlockAchievementToggle.toggle.onValueChanged.AddListener(delegate(bool on)
			{
				this.AchievementFilter(on, MuseumPanel.AchievementStatus.NotUnlock);
			});
			this.SetupLifetimeGrid();
			this.RefreshLifetimeData();
			this.UpdateLifetimeGrid();
			this._initialized = true;
			return true;
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x0002A424 File Offset: 0x00028624
		private void SetupLifetimeGrid()
		{
			bool flag = Singleton<GameMaster>.Instance.CurrentProfile.BluePoint != 0;
			if (!this._setupLifetimeGrid || this._oldDataInGrid != flag)
			{
				if (this._lifetimeGird != null)
				{
					foreach (RectTransform rectTransform in this._lifetimeGird)
					{
						Object.Destroy(rectTransform.gameObject);
					}
				}
				this._lifetimeGird = new List<RectTransform>();
				this._lifetimeTotalBp = Object.Instantiate<RectTransform>(this.lifetimeTemplate, this.lifetimeGrid);
				this._lifetimeTotalBpWidget = this._lifetimeTotalBp.GetComponent<LifetimeWidget>();
				this._lifetimeTotalBpWidget.Initialize("TotalBp", false, flag);
				this._lifetimeTotalBp.gameObject.SetActive(true);
				this._lifetimeGird.Add(this._lifetimeTotalBp);
				this._lifetimeTime = Object.Instantiate<RectTransform>(this.lifetimeTemplate, this.lifetimeGrid);
				this._lifetimeTimeWidget = this._lifetimeTime.GetComponent<LifetimeWidget>();
				this._lifetimeTimeWidget.Initialize("Time", false, false);
				this._lifetimeTime.gameObject.SetActive(true);
				this._lifetimeGird.Add(this._lifetimeTime);
				this._lifetimeGame = Object.Instantiate<RectTransform>(this.lifetimeTemplate, this.lifetimeGrid);
				this._lifetimeGameWidget = this._lifetimeGame.GetComponent<LifetimeWidget>();
				this._lifetimeGameWidget.Initialize("Game", false, false);
				this._lifetimeGame.gameObject.SetActive(true);
				this._lifetimeGird.Add(this._lifetimeGame);
				this._lifetimeWin = Object.Instantiate<RectTransform>(this.lifetimeTemplate, this.lifetimeGrid);
				this._lifetimeWinWidget = this._lifetimeWin.GetComponent<LifetimeWidget>();
				this._lifetimeWinWidget.Initialize("Win", false, false);
				this._lifetimeWin.gameObject.SetActive(true);
				this._lifetimeGird.Add(this._lifetimeWin);
				this._lifetimePerfect = Object.Instantiate<RectTransform>(this.lifetimeTemplate, this.lifetimeGrid);
				this._lifetimePerfectWidget = this._lifetimePerfect.GetComponent<LifetimeWidget>();
				this._lifetimePerfectWidget.Initialize("Perfect", false, false);
				this._lifetimePerfect.gameObject.SetActive(true);
				this._lifetimeGird.Add(this._lifetimePerfect);
				this._lifetimePuzzle = Object.Instantiate<RectTransform>(this.lifetimeTemplate, this.lifetimeGrid);
				this._lifetimePuzzleWidget = this._lifetimePuzzle.GetComponent<LifetimeWidget>();
				this._lifetimePuzzleWidget.Initialize("Puzzle", flag, false);
				this._lifetimePuzzle.gameObject.SetActive(true);
				this._lifetimeGird.Add(this._lifetimePuzzle);
				this.lifetimeTemplate.gameObject.SetActive(false);
				this._oldDataInGrid = flag;
				this._setupLifetimeGrid = true;
			}
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x0002A6FC File Offset: 0x000288FC
		public void UpdateLifetimeGrid()
		{
			float num = 0f;
			foreach (RectTransform rectTransform in this._lifetimeGird)
			{
				if (!(rectTransform == null) && !(rectTransform.gameObject == null))
				{
					num += rectTransform.sizeDelta.y;
				}
			}
			this.lifetimeGrid.sizeDelta = new Vector2(this.lifetimeGrid.sizeDelta.x, num);
			if (this.lifetimeLayoutGroup != null)
			{
				this.lifetimeLayoutGroup.CalculateLayoutInputVertical();
				this.lifetimeLayoutGroup.SetLayoutVertical();
			}
			if (this.contentLayoutGroup != null)
			{
				this.contentLayoutGroup.CalculateLayoutInputVertical();
				this.contentLayoutGroup.SetLayoutVertical();
				this.contentLayoutGroup.GetComponent<ContentSizeFitter>().SetLayoutVertical();
			}
			this.lifetimeScrollRect.normalizedPosition = Vector2.one;
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x0002A7FC File Offset: 0x000289FC
		public override void OnLocaleChanged()
		{
			base.OnLocaleChanged();
			foreach (KeyValuePair<int, GameObject> keyValuePair in this._exhibitLabelDic)
			{
				int num;
				GameObject gameObject;
				keyValuePair.Deconstruct(ref num, ref gameObject);
				int num2 = num;
				this._exhibitLabelDic[num2].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Museum.ExhibitType.T{0}", num2).Localize(true);
			}
			foreach (KeyValuePair<string, Toggle> keyValuePair2 in this._characterFilterList)
			{
				if (keyValuePair2.Key == "Neutral")
				{
					keyValuePair2.Value.GetComponent<CharacterToggleWidget>().SetCharacter("Tooltip.EntityTitle.Neutral".Localize(true), this.portraitList["Neutral"], true);
				}
				else
				{
					PlayerUnit playerUnit = Library.CreatePlayerUnit(keyValuePair2.Key);
					keyValuePair2.Value.GetComponent<CharacterToggleWidget>().SetCharacter(playerUnit.Name, this.portraitList[keyValuePair2.Key], playerUnit.Config.IsSelectable);
				}
			}
			this.SetDropdowns();
			this._initialized = false;
			this.Initialization();
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x0002A964 File Offset: 0x00028B64
		public override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			base.OnInputDeviceChanged(inputDevice);
			this.OnCardScrollRectValueChanged(this.cardsScrollRect.scrollRect.normalizedPosition);
			this.OnAchievementScrollRectValueChanged(this.achievementScrollRect.normalizedPosition);
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x0002A994 File Offset: 0x00028B94
		private void OnCardScrollRectValueChanged(Vector2 vec)
		{
			bool flag = true;
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				flag = vec.y > 0.999f;
			}
			this.orderDropdown.GetComponentInChildren<Selectable>().interactable = flag;
			this.revealDropdown.GetComponentInChildren<Selectable>().interactable = flag;
			this.orderSwitch.GetComponent<Selectable>().interactable = flag;
			this.cardUpgradedToggle.GetComponent<Selectable>().interactable = flag;
			this.cardStyleToggle.GetComponent<Selectable>().interactable = flag;
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x0002AA14 File Offset: 0x00028C14
		private void OnAchievementScrollRectValueChanged(Vector2 vec)
		{
			bool flag = true;
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				flag = vec.y > 0.98f;
			}
			this.allAchievementToggle.GetComponent<Selectable>().interactable = flag;
			this.unlockAchievementToggle.GetComponent<Selectable>().interactable = flag;
			this.notUnlockAchievementToggle.GetComponent<Selectable>().interactable = flag;
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x0002AA74 File Offset: 0x00028C74
		public void OnGamepadPairButtonActive(float valueStep)
		{
			ToggleGroup componentInChildren = base.GetComponentInChildren<ToggleGroup>();
			int num = (int)((float)this.GetTabIndex() + valueStep);
			if (num < 0 || num >= componentInChildren.transform.childCount)
			{
				return;
			}
			Toggle component = componentInChildren.transform.GetChild(num).GetComponent<Toggle>();
			if (component != null)
			{
				component.isOn = true;
			}
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x0002AAC8 File Offset: 0x00028CC8
		private void SetExhibitNavigation(MuseumExhibitWidget museumExhibit, int key, int index)
		{
			Selectable component = museumExhibit.exhibitWidget.GetComponent<Button>();
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			if (index % 9 != 0 && (index - 5) % 9 != 0)
			{
				navigation.selectOnLeft = this._exhibitWidgetDic[key][index - 1].exhibitWidget.GetComponent<Button>();
			}
			if ((index + 1) % 9 != 0 && (index - 4) % 9 != 0 && index + 1 < this._exhibitWidgetDic[key].Count)
			{
				navigation.selectOnRight = this._exhibitWidgetDic[key][index + 1].exhibitWidget.GetComponent<Button>();
			}
			int num = index % 9;
			bool flag = true;
			int num2;
			if (num > 4)
			{
				num2 = index - num + 8;
				flag = false;
			}
			else
			{
				num2 = index - num + 4;
			}
			if (num2 + 1 >= this._exhibitWidgetDic[key].Count)
			{
				if (key + 1 <= this._exhibitWidgetDic.Count)
				{
					if (flag)
					{
						int num3 = num;
						if (num3 >= this._exhibitWidgetDic[key + 1].Count)
						{
							num3 = this._exhibitWidgetDic[key + 1].Count - 1;
						}
						navigation.selectOnDown = this._exhibitWidgetDic[key + 1][num3].exhibitWidget.GetComponent<Button>();
					}
					else
					{
						int num4 = num - 5;
						if (num4 >= this._exhibitWidgetDic[key + 1].Count)
						{
							num4 = this._exhibitWidgetDic[key + 1].Count - 1;
						}
						navigation.selectOnDown = this._exhibitWidgetDic[key + 1][num4].exhibitWidget.GetComponent<Button>();
					}
				}
			}
			else if (flag)
			{
				int num5 = num + 5;
				if (num5 >= 9)
				{
					num5 = 8;
				}
				int num6 = index - num + num5;
				if (num6 >= this._exhibitWidgetDic[key].Count)
				{
					num6 = this._exhibitWidgetDic[key].Count - 1;
				}
				navigation.selectOnDown = this._exhibitWidgetDic[key][num6].exhibitWidget.GetComponent<Button>();
			}
			else
			{
				int num7 = num + 4;
				int num8 = index - num + num7;
				if (num8 >= this._exhibitWidgetDic[key].Count)
				{
					num8 = this._exhibitWidgetDic[key].Count - 1;
				}
				navigation.selectOnDown = this._exhibitWidgetDic[key][num8].exhibitWidget.GetComponent<Button>();
			}
			if (index < 5)
			{
				if (key > 1)
				{
					int count = this._exhibitWidgetDic[key - 1].Count;
					if (count % 9 > 0 && count % 9 <= 5)
					{
						int num9 = count - count % 9 + index;
						if (num9 >= count)
						{
							num9 = count - 1;
						}
						navigation.selectOnUp = this._exhibitWidgetDic[key - 1][num9].exhibitWidget.GetComponent<Button>();
					}
					else
					{
						int num10 = count - count % 9 + 5 + index;
						if (count % 9 == 0)
						{
							num10 -= 9;
						}
						if (num10 >= count)
						{
							num10 = count - 1;
						}
						navigation.selectOnUp = this._exhibitWidgetDic[key - 1][num10].exhibitWidget.GetComponent<Button>();
					}
				}
			}
			else if (flag)
			{
				int num11 = num - 4;
				if (num11 >= 0)
				{
					num11 = -1;
				}
				int num12 = index - num + num11;
				navigation.selectOnUp = this._exhibitWidgetDic[key][num12].exhibitWidget.GetComponent<Button>();
			}
			else
			{
				int num13 = num - 5;
				int num14 = index - num + num13;
				navigation.selectOnUp = this._exhibitWidgetDic[key][num14].exhibitWidget.GetComponent<Button>();
			}
			component.navigation = navigation;
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x0002AE7C File Offset: 0x0002907C
		private void SetDropdowns()
		{
			this.orderDropdown.dropdown.ClearOptions();
			TMP_Dropdown dropdown = this.orderDropdown.dropdown;
			List<string> list = new List<string>();
			list.Add("Game.IndexOrder".Localize(true));
			list.Add("Game.RarityOrder".Localize(true));
			list.Add("Game.ManaAmountOrder".Localize(true));
			list.Add("Game.IllustratorOrder".Localize(true));
			dropdown.AddOptions(list);
			this.orderDropdown.dropdown.SetValueWithoutNotify((int)MuseumPanel._orderStatus);
			this.revealDropdown.dropdown.ClearOptions();
			TMP_Dropdown dropdown2 = this.revealDropdown.dropdown;
			List<string> list2 = new List<string>();
			list2.Add("Museum.AllCards".Localize(true));
			list2.Add("Museum.Revealed".Localize(true));
			list2.Add("Museum.UnRevealed".Localize(true));
			dropdown2.AddOptions(list2);
			this.revealDropdown.dropdown.SetValueWithoutNotify((int)MuseumPanel._revealStatus);
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x0002AF74 File Offset: 0x00029174
		private void SetExhibitWidgetPositionAnim()
		{
			int num = 9;
			float num2 = -100f;
			float num3 = 0f;
			foreach (KeyValuePair<int, List<MuseumExhibitWidget>> keyValuePair in this._exhibitWidgetDic)
			{
				int num4;
				List<MuseumExhibitWidget> list;
				keyValuePair.Deconstruct(ref num4, ref list);
				int num5 = num4;
				List<MuseumExhibitWidget> list2 = list;
				int num6 = list2.Count / num;
				this._exhibitLabelDic[num5].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Museum.ExhibitType.T{0}", num5).Localize(true);
				RectTransform component = this._exhibitLabelDic[num5].GetComponent<RectTransform>();
				float y = component.sizeDelta.y;
				component.sizeDelta = new Vector2(0f, y);
				component.anchoredPosition = new Vector2(0f, num2);
				num2 -= 100f;
				for (int i = 0; i < list2.Count; i++)
				{
					int num7 = i / num;
					int num8 = i % num;
					float num9 = ((num8 / 5 >= 1) ? (470f * (0.5f + (float)(num8 % 5))) : (470f * (float)num8));
					num9 += 100f;
					float num10 = ((num8 / 5 >= 1) ? 280f : 0f);
					num3 = (float)(-2 * num7) * 280f - num10 + num2;
					list2[i].GetComponent<CanvasGroup>().alpha = 1f;
					list2[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(num9, num3);
					this.SetExhibitNavigation(list2[i], num5, i);
				}
				num2 = num3 - 800f;
			}
			this.RefreshExhibits();
			this.exhibitsLayout.sizeDelta = new Vector2(this.exhibitsLayout.sizeDelta.x, -num2 + 100f);
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x0002B170 File Offset: 0x00029370
		private void RefreshExhibits()
		{
			foreach (KeyValuePair<int, List<MuseumExhibitWidget>> keyValuePair in this._exhibitWidgetDic)
			{
				int num;
				List<MuseumExhibitWidget> list;
				keyValuePair.Deconstruct(ref num, ref list);
				List<MuseumExhibitWidget> list2 = list;
				for (int i = 0; i < list2.Count; i++)
				{
					string id = list2[i].Exhibit.Id;
					list2[i].IsLock = MuseumPanel.IsExhibitLocked(id);
					list2[i].IsReveal = MuseumPanel.IsExhibitRevealed(id);
					list2[i].Refresh();
				}
			}
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x0002B228 File Offset: 0x00029428
		public void OnMuseumExhibitClick(MuseumExhibitWidget exhibit)
		{
			if (exhibit.IsLock)
			{
				return;
			}
			this.infoExhibitLockMask.gameObject.SetActive(exhibit.IsLock);
			this.infoExhibit.MainImage.color = ((exhibit.IsLock || !exhibit.IsReveal) ? Color.black : Color.white);
			this.museumExhibitTooltip.gameObject.SetActive(!exhibit.IsLock && exhibit.IsReveal);
			if (exhibit.IsLock)
			{
				this.infoExhibitName.text = "Museum.LockExhibit".Localize(true);
			}
			else if (!exhibit.IsReveal)
			{
				this.infoExhibitName.text = "Museum.UnRevealExhibit".Localize(true);
			}
			else
			{
				this.infoExhibitName.text = exhibit.Exhibit.Name;
				this.museumExhibitTooltip.SetExhibit(exhibit.Exhibit);
			}
			this.infoExhibit.Exhibit = exhibit.Exhibit;
			this.infoExhibitRarity.text = ("Rarity." + exhibit.Exhibit.Config.Rarity.ToString()).Localize(true);
			this.infoExhibitBg.sprite = this.boothList[exhibit.Exhibit.Config.Rarity] ?? this.boothList[Rarity.Common];
			this.infoPanelRoot.SetActive(true);
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x0002B394 File Offset: 0x00029594
		public void ResetCardTab()
		{
			foreach (KeyValuePair<ManaColor, Toggle> keyValuePair in this.manaFilterList)
			{
				keyValuePair.Value.SetIsOnWithoutNotify(false);
			}
			foreach (KeyValuePair<string, Toggle> keyValuePair2 in this._characterFilterList)
			{
				keyValuePair2.Value.SetIsOnWithoutNotify(true);
			}
			foreach (KeyValuePair<CardType, Toggle> keyValuePair3 in this.typeFilterList)
			{
				keyValuePair3.Value.SetIsOnWithoutNotify(true);
			}
			MuseumPanel._increaseOrder = true;
			this.orderSwitch.SetValueWithoutNotifier(true, true);
			MuseumPanel._orderStatus = MuseumPanel.OrderStatus.Index;
			MuseumPanel._revealStatus = MuseumPanel.RevealStatus.All;
			this.orderDropdown.dropdown.SetValueWithoutNotify(0);
			this.revealDropdown.dropdown.SetValueWithoutNotify(0);
			this.cardUpgradedToggle.toggle.SetIsOnWithoutNotify(false);
			this.cardStyleToggle.toggle.SetIsOnWithoutNotify(false);
			this.cardsScrollRect.scrollRect.normalizedPosition = Vector2.one;
			this.cardsScrollRect.OnValueChangedListener(Vector2.one);
			this.cardsFilterScrollRect.normalizedPosition = Vector2.one;
			this.ClearTextFilter(false);
			this.RefreshCards();
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x0002B51C File Offset: 0x0002971C
		public void ResetExhibitTab()
		{
			this.infoPanelRoot.SetActive(false);
			this.infoExhibitBg.sprite = this.boothList[Rarity.Common];
			this.exhibitScrollRect.normalizedPosition = Vector2.one;
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x0002B551 File Offset: 0x00029751
		public void RefreshCards()
		{
			this.InternalRefreshCards(false);
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x0002B55A File Offset: 0x0002975A
		public void RefreshCardsKeepPosition()
		{
			this.InternalRefreshCards(true);
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x0002B564 File Offset: 0x00029764
		private void InternalRefreshCards(bool keepPosition)
		{
			if (keepPosition)
			{
				this._scrollPosition = this.cardsScrollRect.scrollRect.normalizedPosition;
			}
			IEnumerable<Type> enumerable = Enumerable.Intersect<Type>(Enumerable.Intersect<Type>(this.FilterCardByMana(), this.FilterCardByType()), this.FilterCardByCharacter());
			List<Card> list = new List<Card>();
			int num = 0;
			foreach (Type type in enumerable)
			{
				Card card = Library.CreateCard(type);
				if ((GameMaster.ShowAllCardsInMuseum || card.Config.DebugLevel <= 1) && !card.Config.HideMesuem && (card.Config.DebugLevel != 1 || ResourcesHelper.TryGetCardImage(card.Id)) && (!this.cardStyleToggle.toggle.isOn || card.Config.SubIllustrator.Count != 0))
				{
					switch (MuseumPanel._revealStatus)
					{
					case MuseumPanel.RevealStatus.All:
						break;
					case MuseumPanel.RevealStatus.Revealed:
						if (!MuseumPanel.IsCardRevealed(card))
						{
							continue;
						}
						break;
					case MuseumPanel.RevealStatus.Unrevealed:
						if (MuseumPanel.IsCardRevealed(card))
						{
							continue;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					if (card.CanUpgrade && this.cardUpgradedToggle.toggle.isOn)
					{
						card.Upgrade();
					}
					string text = this.textFilter.text.ToLower();
					if (text.IsNullOrEmpty() || MuseumPanel.Filter(card, text))
					{
						if (MuseumPanel.IsCardLocked(card.Id) || !MuseumPanel.IsCardRevealed(card))
						{
							num++;
						}
						list.Add(card);
					}
				}
			}
			list = MuseumPanel.SortCards(list);
			bool flag = false;
			if (num != this._lastUnrevealedCardCount && this._lastUnrevealedCardCount != -1)
			{
				flag = true;
			}
			else if (this._lastCardData.Count == list.Count)
			{
				for (int i = 0; i < this._lastCardData.Count; i++)
				{
					if (this._lastCardData[i].Id != list[i].Id || this._lastCardData[i].IsUpgraded != list[i].IsUpgraded)
					{
						flag = true;
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				this.cardsScrollRect.ReloadData(new MuseumPanel.CardsRows(5, list));
			}
			this.revealCountTmp.text = "Museum.Revealed".Localize(true) + string.Format(" {0}/{1}", list.Count - num, list.Count);
			this._lastCardData = list;
			this.cardsScrollRect.scrollRect.normalizedPosition = (keepPosition ? this._scrollPosition : Vector2.one);
			this.cardsScrollRect.OnValueChangedListener(keepPosition ? this._scrollPosition : Vector2.one);
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x0002B840 File Offset: 0x00029A40
		private static List<Card> SortCards(IEnumerable<Card> cards)
		{
			List<Card> list2;
			switch (MuseumPanel._orderStatus)
			{
			case MuseumPanel.OrderStatus.Index:
			{
				List<Card> list;
				if (!MuseumPanel._increaseOrder)
				{
					list = Enumerable.ToList<Card>(Enumerable.OrderByDescending<Card, int>(cards, (Card card) => card.Config.Index));
				}
				else
				{
					list = Enumerable.ToList<Card>(Enumerable.OrderBy<Card, int>(cards, (Card card) => card.Config.Index));
				}
				list2 = list;
				break;
			}
			case MuseumPanel.OrderStatus.Rarity:
			{
				List<Card> list3;
				if (!MuseumPanel._increaseOrder)
				{
					list3 = Enumerable.ToList<Card>(Enumerable.ThenByDescending<Card, int>(Enumerable.OrderByDescending<Card, Rarity>(cards, (Card card) => card.Config.Rarity), (Card card) => card.Config.Index));
				}
				else
				{
					list3 = Enumerable.ToList<Card>(Enumerable.ThenBy<Card, int>(Enumerable.OrderBy<Card, Rarity>(cards, (Card card) => card.Config.Rarity), (Card card) => card.Config.Index));
				}
				list2 = list3;
				break;
			}
			case MuseumPanel.OrderStatus.Mana:
			{
				List<Card> list4;
				if (!MuseumPanel._increaseOrder)
				{
					list4 = Enumerable.ToList<Card>(Enumerable.ThenByDescending<Card, int>(Enumerable.ThenByDescending<Card, int>(Enumerable.ThenByDescending<Card, bool>(Enumerable.OrderByDescending<Card, bool>(cards, (Card card) => card.IsForbidden), (Card card) => card.Config.IsXCost), (Card card) => card.Cost.Amount), (Card card) => card.Config.Index));
				}
				else
				{
					list4 = Enumerable.ToList<Card>(Enumerable.ThenBy<Card, int>(Enumerable.ThenBy<Card, int>(Enumerable.ThenBy<Card, bool>(Enumerable.OrderBy<Card, bool>(cards, (Card card) => card.IsForbidden), (Card card) => card.Config.IsXCost), (Card card) => card.Cost.Amount), (Card card) => card.Config.Index));
				}
				list2 = list4;
				break;
			}
			case MuseumPanel.OrderStatus.Illustrator:
			{
				List<Card> list5;
				if (!MuseumPanel._increaseOrder)
				{
					list5 = Enumerable.ToList<Card>(Enumerable.ThenByDescending<Card, int>(Enumerable.OrderByDescending<Card, string>(cards, (Card card) => GameMaster.GetPreferredCardIllustrator(card) ?? card.Config.Illustrator), (Card card) => card.Config.Index));
				}
				else
				{
					list5 = Enumerable.ToList<Card>(Enumerable.ThenBy<Card, int>(Enumerable.OrderBy<Card, string>(cards, (Card card) => GameMaster.GetPreferredCardIllustrator(card) ?? card.Config.Illustrator), (Card card) => card.Config.Index));
				}
				list2 = list5;
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			return list2;
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x0002BB6C File Offset: 0x00029D6C
		private void CardOrder(bool increase)
		{
			MuseumPanel._increaseOrder = increase;
			this.RefreshCards();
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x0002BB7A File Offset: 0x00029D7A
		private void CardOrder(MuseumPanel.OrderStatus status)
		{
			MuseumPanel._orderStatus = status;
			this.RefreshCards();
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x0002BB88 File Offset: 0x00029D88
		private void CardOrder(MuseumPanel.RevealStatus status)
		{
			MuseumPanel._revealStatus = status;
			this.RefreshCards();
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x0002BB98 File Offset: 0x00029D98
		private IEnumerable<Type> FilterCardByCharacter()
		{
			List<Type> list = new List<Type>();
			string[] array = Enumerable.ToArray<string>(Enumerable.Select<KeyValuePair<string, Toggle>, string>(Enumerable.Where<KeyValuePair<string, Toggle>>(this._characterFilterList, (KeyValuePair<string, Toggle> kv) => !kv.Value.isOn), (KeyValuePair<string, Toggle> kv) => kv.Key));
			foreach (ValueTuple<Type, CardConfig> valueTuple in Library.EnumerateCardTypes())
			{
				Type item = valueTuple.Item1;
				CardConfig item2 = valueTuple.Item2;
				if (array.Length == 0)
				{
					list.Add(item);
				}
				if (Enumerable.Contains<string>(array, item2.Owner))
				{
					list.Add(item);
				}
				if (Enumerable.Contains<string>(array, "Neutral") && string.IsNullOrWhiteSpace(item2.Owner))
				{
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x0002BC88 File Offset: 0x00029E88
		private IEnumerable<Type> FilterCardByMana()
		{
			List<Type> list = new List<Type>();
			ManaColor[] array = Enumerable.ToArray<ManaColor>(Enumerable.Select<KeyValuePair<ManaColor, Toggle>, ManaColor>(Enumerable.Where<KeyValuePair<ManaColor, Toggle>>(this.manaFilterList, (KeyValuePair<ManaColor, Toggle> kv) => kv.Value.isOn), (KeyValuePair<ManaColor, Toggle> kv) => kv.Key));
			foreach (ValueTuple<Type, CardConfig> valueTuple in Library.EnumerateCardTypes())
			{
				Type item = valueTuple.Item1;
				CardConfig item2 = valueTuple.Item2;
				if (array.Length == 0)
				{
					list.Add(item);
				}
				if (Enumerable.Contains<ManaColor>(array, ManaColor.Philosophy))
				{
					if (item2.Colors.Count > 1 && Enumerable.ToArray<ManaColor>(Enumerable.Intersect<ManaColor>(item2.Colors, array)).Length >= array.Length - 1)
					{
						list.Add(item);
					}
				}
				else
				{
					if (Enumerable.Contains<ManaColor>(array, ManaColor.Colorless) && (item2.Colors.Count <= 0 || Enumerable.Contains<ManaColor>(item2.Colors, ManaColor.Colorless)))
					{
						list.Add(item);
					}
					if (Enumerable.ToArray<ManaColor>(Enumerable.Intersect<ManaColor>(item2.Colors, array)).Length >= 1)
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x0002BDD0 File Offset: 0x00029FD0
		private IEnumerable<Type> FilterCardByType()
		{
			List<Type> list = new List<Type>();
			CardType[] array = Enumerable.ToArray<CardType>(Enumerable.Select<KeyValuePair<CardType, Toggle>, CardType>(Enumerable.Where<KeyValuePair<CardType, Toggle>>(this.typeFilterList, (KeyValuePair<CardType, Toggle> kv) => !kv.Value.isOn), (KeyValuePair<CardType, Toggle> kv) => kv.Key));
			foreach (ValueTuple<Type, CardConfig> valueTuple in Library.EnumerateCardTypes())
			{
				Type item = valueTuple.Item1;
				CardConfig item2 = valueTuple.Item2;
				if (array.Length == 0)
				{
					list.Add(item);
				}
				else if (Enumerable.Contains<CardType>(array, item2.Type))
				{
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x0002BEA0 File Offset: 0x0002A0A0
		private static void HideTabRoot(GameObject root)
		{
			if (root.activeSelf)
			{
				root.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).OnComplete(delegate
				{
					root.gameObject.SetActive(false);
				});
			}
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x0002BEF3 File Offset: 0x0002A0F3
		private static void ShowTabRoot(GameObject root)
		{
			root.SetActive(true);
			root.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false);
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x0002BF20 File Offset: 0x0002A120
		protected override void OnShowing()
		{
			this.cardsTab.toggle.SetIsOnWithoutNotify(true);
			this.ResetToCardTab();
			this.cardsScrollRect.ShowWithDelay();
			this.SetExhibitWidgetPositionAnim();
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Museum);
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x0002BF72 File Offset: 0x0002A172
		protected override void OnHiding()
		{
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Idle);
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x0002BF91 File Offset: 0x0002A191
		void IInputActionHandler.OnCancel()
		{
			this.returnButton.onClick.Invoke();
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x0002BFA3 File Offset: 0x0002A1A3
		private void ResetToCardTab()
		{
			MuseumPanel.ShowTabRoot(this.cardsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.exhibitsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.achievementRoot.gameObject);
			this.ResetCardTab();
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x0002BFDC File Offset: 0x0002A1DC
		private void CardTabToggle(bool on)
		{
			if (!on)
			{
				return;
			}
			if (this.cardsRoot.gameObject.activeSelf)
			{
				return;
			}
			MuseumPanel.ShowTabRoot(this.cardsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.exhibitsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.achievementRoot.gameObject);
			this.ResetCardTab();
			GamepadNavigationManager.RefreshSelection();
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x0002C03C File Offset: 0x0002A23C
		private void ExhibitTabToggle(bool on)
		{
			if (!on)
			{
				return;
			}
			if (this.exhibitsRoot.gameObject.activeSelf)
			{
				return;
			}
			MuseumPanel.ShowTabRoot(this.exhibitsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.cardsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.achievementRoot.gameObject);
			this.ResetExhibitTab();
			this.RefreshExhibits();
			GamepadNavigationManager.RefreshSelection();
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x0002C0A4 File Offset: 0x0002A2A4
		private void AchievementTabToggle(bool on)
		{
			if (!on)
			{
				return;
			}
			if (this.achievementRoot.gameObject.activeSelf)
			{
				return;
			}
			MuseumPanel.ShowTabRoot(this.achievementRoot.gameObject);
			MuseumPanel.HideTabRoot(this.cardsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.exhibitsRoot.gameObject);
			this.ResetAchievement();
			if (this._currentProfileName != Singleton<GameMaster>.Instance.CurrentProfile.Name)
			{
				this._currentProfileName = Singleton<GameMaster>.Instance.CurrentProfile.Name;
				this.SetupLifetimeGrid();
				this.RefreshLifetimeData();
			}
			GamepadNavigationManager.RefreshSelection();
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x0002C140 File Offset: 0x0002A340
		public static bool IsCardLocked(string cardId)
		{
			return ExpHelper.GetCardUnlockLevel(cardId) > Singleton<GameMaster>.Instance.CurrentProfileLevel;
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0002C154 File Offset: 0x0002A354
		public static bool IsCardRevealed(Card card)
		{
			return !card.Config.Revealable || card.Config.DebugLevel > 0 || Singleton<GameMaster>.Instance.CurrentProfile.CardsRevealed.Contains(card.Id);
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x0002C18D File Offset: 0x0002A38D
		public static bool IsExhibitLocked(string exhibitId)
		{
			return ExpHelper.GetExhibitUnlockLevel(exhibitId) > Singleton<GameMaster>.Instance.CurrentProfileLevel;
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x0002C1A1 File Offset: 0x0002A3A1
		public static bool IsExhibitRevealed(string exhibitId)
		{
			return !ExhibitConfig.FromId(exhibitId).Revealable || Singleton<GameMaster>.Instance.CurrentProfile.ExhibitsRevealed.Contains(exhibitId);
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x0002C1C7 File Offset: 0x0002A3C7
		public int GetTabIndex()
		{
			return base.GetComponentInChildren<ToggleGroup>().GetFirstActiveToggle().transform.GetSiblingIndex();
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x0002C1DE File Offset: 0x0002A3DE
		private void ClearTextFilter(bool refresh = true)
		{
			this.textFilter.SetTextWithoutNotify("");
			this.clearTextFilter.gameObject.SetActive(false);
			if (refresh)
			{
				this.RefreshCards();
			}
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x0002C20A File Offset: 0x0002A40A
		private void TextFilter(string text)
		{
			this.clearTextFilter.gameObject.SetActive(!text.IsNullOrEmpty());
			this.RefreshCards();
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x0002C22C File Offset: 0x0002A42C
		private static bool Filter(Card card, string filter)
		{
			return ("CardType." + card.CardType.ToString()).Localize(true).IndexOf(filter, 1) >= 0 || card.Name.IndexOf(filter, 1) >= 0 || card.Description.IndexOf(filter, 1) >= 0;
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060008C7 RID: 2247 RVA: 0x0002C28C File Offset: 0x0002A48C
		// (set) Token: 0x060008C8 RID: 2248 RVA: 0x0002C293 File Offset: 0x0002A493
		private static MuseumPanel.AchievementStatus CurrentAchievementStatus { get; set; }

		// Token: 0x060008C9 RID: 2249 RVA: 0x0002C29C File Offset: 0x0002A49C
		private void ResetAchievement()
		{
			this.allAchievementToggle.toggle.SetIsOnWithoutNotify(true);
			this.unlockAchievementToggle.toggle.SetIsOnWithoutNotify(false);
			this.notUnlockAchievementToggle.toggle.SetIsOnWithoutNotify(false);
			MuseumPanel.CurrentAchievementStatus = MuseumPanel.AchievementStatus.All;
			this.RefreshAchievement();
			this.achievementScrollRect.normalizedPosition = Vector2.one;
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x0002C2F8 File Offset: 0x0002A4F8
		private void RefreshAchievement()
		{
			this.achievementParent.DestroyChildren();
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			int num = 0;
			int num2 = 0;
			foreach (AchievementConfig achievementConfig in AchievementConfig.AllConfig())
			{
				bool flag = currentProfile.Achievements.Contains(achievementConfig.Id);
				if (flag)
				{
					num++;
				}
				num2++;
				switch (MuseumPanel.CurrentAchievementStatus)
				{
				case MuseumPanel.AchievementStatus.All:
					break;
				case MuseumPanel.AchievementStatus.Unlock:
					if (!flag)
					{
						continue;
					}
					break;
				case MuseumPanel.AchievementStatus.NotUnlock:
					if (flag)
					{
						continue;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				AchievementWidget achievementWidget = Object.Instantiate<AchievementWidget>(this.achievementTemplate, this.achievementParent);
				if (achievementConfig.Id == "SeijaSpecial" && currentProfile.Achievements.Contains("Seija"))
				{
					achievementWidget.SetAchievement(achievementConfig, currentProfile.Achievements.Contains(achievementConfig.Id), true);
				}
				else
				{
					achievementWidget.SetAchievement(achievementConfig, currentProfile.Achievements.Contains(achievementConfig.Id), false);
				}
			}
			this.achievementCountText.text = "Museum.AchievementCount".Localize(true) + string.Format(" {0}/{1}", num, num2);
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x0002C450 File Offset: 0x0002A650
		private void AchievementFilter(bool on, MuseumPanel.AchievementStatus status)
		{
			if (!on)
			{
				return;
			}
			MuseumPanel.CurrentAchievementStatus = status;
			this.RefreshAchievement();
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x0002C464 File Offset: 0x0002A664
		private static string SecondToHour(int seconds)
		{
			int num2;
			int num3;
			int num = Math.DivRem(Math.DivRem(seconds, 60, ref num2), 60, ref num3);
			return string.Format("{0}:{1:00}:{2:00}", num, num3, num2);
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x0002C4A4 File Offset: 0x0002A6A4
		private void RefreshLifetimeData()
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			CharacterStatsSaveData[] characterStats = currentProfile.CharacterStats;
			this._lifetimeTotalBpWidget.SetTotalValue(currentProfile.TotalBluePoint);
			this._lifetimeTotalBpWidget.UpdateCharacterStatus(Enumerable.ToDictionary<CharacterStatsSaveData, string, string>(characterStats, (CharacterStatsSaveData characterStat) => characterStat.CharacterId, (CharacterStatsSaveData characterStat) => characterStat.TotalBluePoint.ToString()));
			this._lifetimeTimeWidget.SetTotalValue(MuseumPanel.SecondToHour(currentProfile.TotalPlaySeconds));
			this._lifetimeTimeWidget.UpdateCharacterStatus(Enumerable.ToDictionary<CharacterStatsSaveData, string, string>(characterStats, (CharacterStatsSaveData characterStat) => characterStat.CharacterId, (CharacterStatsSaveData characterStat) => MuseumPanel.SecondToHour(characterStat.TotalPlaySeconds)));
			this._lifetimeGameWidget.SetTotalValue(currentProfile.FailCount + currentProfile.SuccessCount + currentProfile.PerfectSuccessCount);
			this._lifetimeGameWidget.UpdateCharacterStatus(Enumerable.ToDictionary<CharacterStatsSaveData, string, string>(characterStats, (CharacterStatsSaveData characterStat) => characterStat.CharacterId, (CharacterStatsSaveData characterStat) => (characterStat.FailCount + characterStat.SuccessCount + characterStat.PerfectSuccessCount).ToString()));
			this._lifetimeWinWidget.SetTotalValue(currentProfile.SuccessCount + currentProfile.PerfectSuccessCount);
			this._lifetimeWinWidget.UpdateCharacterStatus(Enumerable.ToDictionary<CharacterStatsSaveData, string, string>(characterStats, (CharacterStatsSaveData characterStat) => characterStat.CharacterId, (CharacterStatsSaveData characterStat) => (characterStat.SuccessCount + characterStat.PerfectSuccessCount).ToString()));
			this._lifetimePerfectWidget.SetTotalValue(currentProfile.PerfectSuccessCount);
			this._lifetimePerfectWidget.UpdateCharacterStatus(Enumerable.ToDictionary<CharacterStatsSaveData, string, string>(characterStats, (CharacterStatsSaveData characterStat) => characterStat.CharacterId, (CharacterStatsSaveData characterStat) => characterStat.PerfectSuccessCount.ToString()));
			this._lifetimePuzzleWidget.SetTotalValue(currentProfile.TotalPuzzleCount);
			this._lifetimePuzzleWidget.UpdateCharacterStatus(Enumerable.ToDictionary<CharacterStatsSaveData, string, string>(characterStats, (CharacterStatsSaveData characterStat) => characterStat.CharacterId, (CharacterStatsSaveData characterStat) => characterStat.PuzzleCount.ToString()));
		}

		// Token: 0x0400062F RID: 1583
		private const int CardsPerRow = 5;

		// Token: 0x04000630 RID: 1584
		[Header("General")]
		[SerializeField]
		private CommonToggleWidget cardsTab;

		// Token: 0x04000631 RID: 1585
		[SerializeField]
		private CommonToggleWidget exhibitsTab;

		// Token: 0x04000632 RID: 1586
		[SerializeField]
		private CommonToggleWidget achievementTab;

		// Token: 0x04000633 RID: 1587
		[SerializeField]
		private Button returnButton;

		// Token: 0x04000634 RID: 1588
		[Header("Card")]
		[SerializeField]
		private RectTransform cardsRoot;

		// Token: 0x04000635 RID: 1589
		[SerializeField]
		private RecyclableScrollRectWidget cardsScrollRect;

		// Token: 0x04000636 RID: 1590
		[SerializeField]
		private ScrollRect cardsFilterScrollRect;

		// Token: 0x04000637 RID: 1591
		[SerializeField]
		private DropdownWidget orderDropdown;

		// Token: 0x04000638 RID: 1592
		[SerializeField]
		private DropdownWidget revealDropdown;

		// Token: 0x04000639 RID: 1593
		[SerializeField]
		private SwitchWidget orderSwitch;

		// Token: 0x0400063A RID: 1594
		[SerializeField]
		private CommonToggleWidget cardUpgradedToggle;

		// Token: 0x0400063B RID: 1595
		[SerializeField]
		private CommonToggleWidget cardStyleToggle;

		// Token: 0x0400063C RID: 1596
		[SerializeField]
		private RectTransform characterToggleLayout;

		// Token: 0x0400063D RID: 1597
		[SerializeField]
		private Toggle characterToggleTemplate;

		// Token: 0x0400063E RID: 1598
		[SerializeField]
		private GameObject revealCountRoot;

		// Token: 0x0400063F RID: 1599
		[SerializeField]
		private TextMeshProUGUI revealCountTmp;

		// Token: 0x04000640 RID: 1600
		[SerializeField]
		private AssociationList<ManaColor, Toggle> manaFilterList;

		// Token: 0x04000641 RID: 1601
		[SerializeField]
		private AssociationList<CardType, Toggle> typeFilterList;

		// Token: 0x04000642 RID: 1602
		[SerializeField]
		private AssociationList<string, Sprite> portraitList;

		// Token: 0x04000643 RID: 1603
		[SerializeField]
		private AssociationList<Rarity, Sprite> boothList;

		// Token: 0x04000644 RID: 1604
		[SerializeField]
		private CommonButtonWidget clearTextFilter;

		// Token: 0x04000645 RID: 1605
		[SerializeField]
		private TMP_InputField textFilter;

		// Token: 0x04000646 RID: 1606
		[Header("Exhibit")]
		[SerializeField]
		private RectTransform exhibitsRoot;

		// Token: 0x04000647 RID: 1607
		[SerializeField]
		private RectTransform exhibitsLayout;

		// Token: 0x04000648 RID: 1608
		[SerializeField]
		private ScrollRect exhibitScrollRect;

		// Token: 0x04000649 RID: 1609
		[SerializeField]
		private MuseumExhibitWidget exhibitWidgetTemplate;

		// Token: 0x0400064A RID: 1610
		[SerializeField]
		private GameObject separatorLabelTemplate;

		// Token: 0x0400064B RID: 1611
		[SerializeField]
		private GameObject infoPanelRoot;

		// Token: 0x0400064C RID: 1612
		[SerializeField]
		private ExhibitWidget infoExhibit;

		// Token: 0x0400064D RID: 1613
		[SerializeField]
		private TextMeshProUGUI infoExhibitName;

		// Token: 0x0400064E RID: 1614
		[SerializeField]
		private TextMeshProUGUI infoExhibitRarity;

		// Token: 0x0400064F RID: 1615
		[SerializeField]
		private Image infoExhibitBg;

		// Token: 0x04000650 RID: 1616
		[SerializeField]
		private Image infoExhibitLockMask;

		// Token: 0x04000651 RID: 1617
		[SerializeField]
		private MuseumExhibitTooltip museumExhibitTooltip;

		// Token: 0x04000652 RID: 1618
		[Header("Achievement")]
		[SerializeField]
		private RectTransform achievementRoot;

		// Token: 0x04000653 RID: 1619
		[SerializeField]
		private RectTransform achievementParent;

		// Token: 0x04000654 RID: 1620
		[SerializeField]
		private AchievementWidget achievementTemplate;

		// Token: 0x04000655 RID: 1621
		[SerializeField]
		private TextMeshProUGUI achievementCountText;

		// Token: 0x04000656 RID: 1622
		[SerializeField]
		private CommonToggleWidget allAchievementToggle;

		// Token: 0x04000657 RID: 1623
		[SerializeField]
		private CommonToggleWidget unlockAchievementToggle;

		// Token: 0x04000658 RID: 1624
		[SerializeField]
		private CommonToggleWidget notUnlockAchievementToggle;

		// Token: 0x04000659 RID: 1625
		[SerializeField]
		private ScrollRect achievementScrollRect;

		// Token: 0x0400065A RID: 1626
		[SerializeField]
		private RectTransform lifetimeGrid;

		// Token: 0x0400065B RID: 1627
		[SerializeField]
		private RectTransform lifetimeTemplate;

		// Token: 0x0400065C RID: 1628
		[SerializeField]
		private ScrollRect lifetimeScrollRect;

		// Token: 0x0400065D RID: 1629
		[SerializeField]
		private VerticalLayoutGroup lifetimeLayoutGroup;

		// Token: 0x0400065E RID: 1630
		[SerializeField]
		private VerticalLayoutGroup contentLayoutGroup;

		// Token: 0x0400065F RID: 1631
		private RectTransform _lifetimeTotalBp;

		// Token: 0x04000660 RID: 1632
		private RectTransform _lifetimeTime;

		// Token: 0x04000661 RID: 1633
		private RectTransform _lifetimeGame;

		// Token: 0x04000662 RID: 1634
		private RectTransform _lifetimeWin;

		// Token: 0x04000663 RID: 1635
		private RectTransform _lifetimePerfect;

		// Token: 0x04000664 RID: 1636
		private RectTransform _lifetimePuzzle;

		// Token: 0x04000665 RID: 1637
		private LifetimeWidget _lifetimeTotalBpWidget;

		// Token: 0x04000666 RID: 1638
		private LifetimeWidget _lifetimeTimeWidget;

		// Token: 0x04000667 RID: 1639
		private LifetimeWidget _lifetimeGameWidget;

		// Token: 0x04000668 RID: 1640
		private LifetimeWidget _lifetimeWinWidget;

		// Token: 0x04000669 RID: 1641
		private LifetimeWidget _lifetimePerfectWidget;

		// Token: 0x0400066A RID: 1642
		private LifetimeWidget _lifetimePuzzleWidget;

		// Token: 0x0400066B RID: 1643
		private const float ExhibitWidgetSpaceX = 470f;

		// Token: 0x0400066C RID: 1644
		private const float ExhibitWidgetSpaceY = 280f;

		// Token: 0x0400066D RID: 1645
		private const int ExhibitWidgetRowMax = 5;

		// Token: 0x0400066E RID: 1646
		private readonly Dictionary<string, Toggle> _characterFilterList = new Dictionary<string, Toggle>();

		// Token: 0x0400066F RID: 1647
		private readonly Dictionary<int, List<MuseumExhibitWidget>> _exhibitWidgetDic = new Dictionary<int, List<MuseumExhibitWidget>>();

		// Token: 0x04000670 RID: 1648
		private readonly Dictionary<int, GameObject> _exhibitLabelDic = new Dictionary<int, GameObject>();

		// Token: 0x04000671 RID: 1649
		private List<Card> _lastCardData = new List<Card>();

		// Token: 0x04000672 RID: 1650
		private int _lastUnrevealedCardCount;

		// Token: 0x04000673 RID: 1651
		private const string NeutralCharacter = "Neutral";

		// Token: 0x04000674 RID: 1652
		private bool _initialized;

		// Token: 0x04000675 RID: 1653
		private CanvasGroup _canvasGroup;

		// Token: 0x04000676 RID: 1654
		private List<RectTransform> _lifetimeGird = new List<RectTransform>();

		// Token: 0x04000677 RID: 1655
		private static bool _increaseOrder = true;

		// Token: 0x04000678 RID: 1656
		private static MuseumPanel.OrderStatus _orderStatus;

		// Token: 0x04000679 RID: 1657
		private static MuseumPanel.RevealStatus _revealStatus;

		// Token: 0x0400067A RID: 1658
		private bool _oldDataInGrid;

		// Token: 0x0400067B RID: 1659
		private bool _setupLifetimeGrid;

		// Token: 0x0400067C RID: 1660
		private Vector2 _scrollPosition;

		// Token: 0x0400067D RID: 1661
		private string _currentProfileName;

		// Token: 0x0400067F RID: 1663
		private const int AchievementPerRow = 6;

		// Token: 0x02000274 RID: 628
		private enum OrderStatus
		{
			// Token: 0x04001106 RID: 4358
			Index,
			// Token: 0x04001107 RID: 4359
			Rarity,
			// Token: 0x04001108 RID: 4360
			Mana,
			// Token: 0x04001109 RID: 4361
			Illustrator
		}

		// Token: 0x02000275 RID: 629
		private enum RevealStatus
		{
			// Token: 0x0400110B RID: 4363
			All,
			// Token: 0x0400110C RID: 4364
			Revealed,
			// Token: 0x0400110D RID: 4365
			Unrevealed
		}

		// Token: 0x02000276 RID: 630
		private class CardsRows : RecyclableScrollRectWidget.IRecyclableScrollRectDataSource
		{
			// Token: 0x0600159C RID: 5532 RVA: 0x00062CC2 File Offset: 0x00060EC2
			public CardsRows(int rowSize, IEnumerable<Card> cards)
			{
				this._rowSize = rowSize;
				this._cards = Enumerable.ToList<Card>(cards);
			}

			// Token: 0x0600159D RID: 5533 RVA: 0x00062CDD File Offset: 0x00060EDD
			public int GetItemCount()
			{
				if (this._cards == null)
				{
					return 0;
				}
				return (this._cards.Count + this._rowSize - 1) / this._rowSize;
			}

			// Token: 0x0600159E RID: 5534 RVA: 0x00062D04 File Offset: 0x00060F04
			public void SetCell(RecyclableScrollRectWidget.RecyclableCell cell, int index)
			{
				CardsRow cardsRow = (CardsRow)cell;
				int num = Math.Min(this._cards.Count, (index + 1) * this._rowSize);
				int num2 = Math.Min(num, index * this._rowSize);
				cardsRow.SetCards(index, (num2 == num) ? Enumerable.Empty<Card>() : Enumerable.Take<Card>(Enumerable.Skip<Card>(this._cards, num2), num));
			}

			// Token: 0x0400110E RID: 4366
			private readonly int _rowSize;

			// Token: 0x0400110F RID: 4367
			private readonly List<Card> _cards;
		}

		// Token: 0x02000277 RID: 631
		private enum AchievementStatus
		{
			// Token: 0x04001111 RID: 4369
			All,
			// Token: 0x04001112 RID: 4370
			Unlock,
			// Token: 0x04001113 RID: 4371
			NotUnlock
		}
	}
}
