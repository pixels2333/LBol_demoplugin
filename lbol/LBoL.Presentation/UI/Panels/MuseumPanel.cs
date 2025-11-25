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
	public class MuseumPanel : UiPanel, IInputActionHandler
	{
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
		public override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			base.OnInputDeviceChanged(inputDevice);
			this.OnCardScrollRectValueChanged(this.cardsScrollRect.scrollRect.normalizedPosition);
			this.OnAchievementScrollRectValueChanged(this.achievementScrollRect.normalizedPosition);
		}
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
		public void ResetExhibitTab()
		{
			this.infoPanelRoot.SetActive(false);
			this.infoExhibitBg.sprite = this.boothList[Rarity.Common];
			this.exhibitScrollRect.normalizedPosition = Vector2.one;
		}
		public void RefreshCards()
		{
			this.InternalRefreshCards(false);
		}
		public void RefreshCardsKeepPosition()
		{
			this.InternalRefreshCards(true);
		}
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
		private void CardOrder(bool increase)
		{
			MuseumPanel._increaseOrder = increase;
			this.RefreshCards();
		}
		private void CardOrder(MuseumPanel.OrderStatus status)
		{
			MuseumPanel._orderStatus = status;
			this.RefreshCards();
		}
		private void CardOrder(MuseumPanel.RevealStatus status)
		{
			MuseumPanel._revealStatus = status;
			this.RefreshCards();
		}
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
		private static void ShowTabRoot(GameObject root)
		{
			root.SetActive(true);
			root.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false);
		}
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
		protected override void OnHiding()
		{
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Idle);
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		void IInputActionHandler.OnCancel()
		{
			this.returnButton.onClick.Invoke();
		}
		private void ResetToCardTab()
		{
			MuseumPanel.ShowTabRoot(this.cardsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.exhibitsRoot.gameObject);
			MuseumPanel.HideTabRoot(this.achievementRoot.gameObject);
			this.ResetCardTab();
		}
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
		public static bool IsCardLocked(string cardId)
		{
			return ExpHelper.GetCardUnlockLevel(cardId) > Singleton<GameMaster>.Instance.CurrentProfileLevel;
		}
		public static bool IsCardRevealed(Card card)
		{
			return !card.Config.Revealable || card.Config.DebugLevel > 0 || Singleton<GameMaster>.Instance.CurrentProfile.CardsRevealed.Contains(card.Id);
		}
		public static bool IsExhibitLocked(string exhibitId)
		{
			return ExpHelper.GetExhibitUnlockLevel(exhibitId) > Singleton<GameMaster>.Instance.CurrentProfileLevel;
		}
		public static bool IsExhibitRevealed(string exhibitId)
		{
			return !ExhibitConfig.FromId(exhibitId).Revealable || Singleton<GameMaster>.Instance.CurrentProfile.ExhibitsRevealed.Contains(exhibitId);
		}
		public int GetTabIndex()
		{
			return base.GetComponentInChildren<ToggleGroup>().GetFirstActiveToggle().transform.GetSiblingIndex();
		}
		private void ClearTextFilter(bool refresh = true)
		{
			this.textFilter.SetTextWithoutNotify("");
			this.clearTextFilter.gameObject.SetActive(false);
			if (refresh)
			{
				this.RefreshCards();
			}
		}
		private void TextFilter(string text)
		{
			this.clearTextFilter.gameObject.SetActive(!text.IsNullOrEmpty());
			this.RefreshCards();
		}
		private static bool Filter(Card card, string filter)
		{
			return ("CardType." + card.CardType.ToString()).Localize(true).IndexOf(filter, 1) >= 0 || card.Name.IndexOf(filter, 1) >= 0 || card.Description.IndexOf(filter, 1) >= 0;
		}
		private static MuseumPanel.AchievementStatus CurrentAchievementStatus { get; set; }
		private void ResetAchievement()
		{
			this.allAchievementToggle.toggle.SetIsOnWithoutNotify(true);
			this.unlockAchievementToggle.toggle.SetIsOnWithoutNotify(false);
			this.notUnlockAchievementToggle.toggle.SetIsOnWithoutNotify(false);
			MuseumPanel.CurrentAchievementStatus = MuseumPanel.AchievementStatus.All;
			this.RefreshAchievement();
			this.achievementScrollRect.normalizedPosition = Vector2.one;
		}
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
		private void AchievementFilter(bool on, MuseumPanel.AchievementStatus status)
		{
			if (!on)
			{
				return;
			}
			MuseumPanel.CurrentAchievementStatus = status;
			this.RefreshAchievement();
		}
		private static string SecondToHour(int seconds)
		{
			int num2;
			int num3;
			int num = Math.DivRem(Math.DivRem(seconds, 60, ref num2), 60, ref num3);
			return string.Format("{0}:{1:00}:{2:00}", num, num3, num2);
		}
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
		private const int CardsPerRow = 5;
		[Header("General")]
		[SerializeField]
		private CommonToggleWidget cardsTab;
		[SerializeField]
		private CommonToggleWidget exhibitsTab;
		[SerializeField]
		private CommonToggleWidget achievementTab;
		[SerializeField]
		private Button returnButton;
		[Header("Card")]
		[SerializeField]
		private RectTransform cardsRoot;
		[SerializeField]
		private RecyclableScrollRectWidget cardsScrollRect;
		[SerializeField]
		private ScrollRect cardsFilterScrollRect;
		[SerializeField]
		private DropdownWidget orderDropdown;
		[SerializeField]
		private DropdownWidget revealDropdown;
		[SerializeField]
		private SwitchWidget orderSwitch;
		[SerializeField]
		private CommonToggleWidget cardUpgradedToggle;
		[SerializeField]
		private CommonToggleWidget cardStyleToggle;
		[SerializeField]
		private RectTransform characterToggleLayout;
		[SerializeField]
		private Toggle characterToggleTemplate;
		[SerializeField]
		private GameObject revealCountRoot;
		[SerializeField]
		private TextMeshProUGUI revealCountTmp;
		[SerializeField]
		private AssociationList<ManaColor, Toggle> manaFilterList;
		[SerializeField]
		private AssociationList<CardType, Toggle> typeFilterList;
		[SerializeField]
		private AssociationList<string, Sprite> portraitList;
		[SerializeField]
		private AssociationList<Rarity, Sprite> boothList;
		[SerializeField]
		private CommonButtonWidget clearTextFilter;
		[SerializeField]
		private TMP_InputField textFilter;
		[Header("Exhibit")]
		[SerializeField]
		private RectTransform exhibitsRoot;
		[SerializeField]
		private RectTransform exhibitsLayout;
		[SerializeField]
		private ScrollRect exhibitScrollRect;
		[SerializeField]
		private MuseumExhibitWidget exhibitWidgetTemplate;
		[SerializeField]
		private GameObject separatorLabelTemplate;
		[SerializeField]
		private GameObject infoPanelRoot;
		[SerializeField]
		private ExhibitWidget infoExhibit;
		[SerializeField]
		private TextMeshProUGUI infoExhibitName;
		[SerializeField]
		private TextMeshProUGUI infoExhibitRarity;
		[SerializeField]
		private Image infoExhibitBg;
		[SerializeField]
		private Image infoExhibitLockMask;
		[SerializeField]
		private MuseumExhibitTooltip museumExhibitTooltip;
		[Header("Achievement")]
		[SerializeField]
		private RectTransform achievementRoot;
		[SerializeField]
		private RectTransform achievementParent;
		[SerializeField]
		private AchievementWidget achievementTemplate;
		[SerializeField]
		private TextMeshProUGUI achievementCountText;
		[SerializeField]
		private CommonToggleWidget allAchievementToggle;
		[SerializeField]
		private CommonToggleWidget unlockAchievementToggle;
		[SerializeField]
		private CommonToggleWidget notUnlockAchievementToggle;
		[SerializeField]
		private ScrollRect achievementScrollRect;
		[SerializeField]
		private RectTransform lifetimeGrid;
		[SerializeField]
		private RectTransform lifetimeTemplate;
		[SerializeField]
		private ScrollRect lifetimeScrollRect;
		[SerializeField]
		private VerticalLayoutGroup lifetimeLayoutGroup;
		[SerializeField]
		private VerticalLayoutGroup contentLayoutGroup;
		private RectTransform _lifetimeTotalBp;
		private RectTransform _lifetimeTime;
		private RectTransform _lifetimeGame;
		private RectTransform _lifetimeWin;
		private RectTransform _lifetimePerfect;
		private RectTransform _lifetimePuzzle;
		private LifetimeWidget _lifetimeTotalBpWidget;
		private LifetimeWidget _lifetimeTimeWidget;
		private LifetimeWidget _lifetimeGameWidget;
		private LifetimeWidget _lifetimeWinWidget;
		private LifetimeWidget _lifetimePerfectWidget;
		private LifetimeWidget _lifetimePuzzleWidget;
		private const float ExhibitWidgetSpaceX = 470f;
		private const float ExhibitWidgetSpaceY = 280f;
		private const int ExhibitWidgetRowMax = 5;
		private readonly Dictionary<string, Toggle> _characterFilterList = new Dictionary<string, Toggle>();
		private readonly Dictionary<int, List<MuseumExhibitWidget>> _exhibitWidgetDic = new Dictionary<int, List<MuseumExhibitWidget>>();
		private readonly Dictionary<int, GameObject> _exhibitLabelDic = new Dictionary<int, GameObject>();
		private List<Card> _lastCardData = new List<Card>();
		private int _lastUnrevealedCardCount;
		private const string NeutralCharacter = "Neutral";
		private bool _initialized;
		private CanvasGroup _canvasGroup;
		private List<RectTransform> _lifetimeGird = new List<RectTransform>();
		private static bool _increaseOrder = true;
		private static MuseumPanel.OrderStatus _orderStatus;
		private static MuseumPanel.RevealStatus _revealStatus;
		private bool _oldDataInGrid;
		private bool _setupLifetimeGrid;
		private Vector2 _scrollPosition;
		private string _currentProfileName;
		private const int AchievementPerRow = 6;
		private enum OrderStatus
		{
			Index,
			Rarity,
			Mana,
			Illustrator
		}
		private enum RevealStatus
		{
			All,
			Revealed,
			Unrevealed
		}
		private class CardsRows : RecyclableScrollRectWidget.IRecyclableScrollRectDataSource
		{
			public CardsRows(int rowSize, IEnumerable<Card> cards)
			{
				this._rowSize = rowSize;
				this._cards = Enumerable.ToList<Card>(cards);
			}
			public int GetItemCount()
			{
				if (this._cards == null)
				{
					return 0;
				}
				return (this._cards.Count + this._rowSize - 1) / this._rowSize;
			}
			public void SetCell(RecyclableScrollRectWidget.RecyclableCell cell, int index)
			{
				CardsRow cardsRow = (CardsRow)cell;
				int num = Math.Min(this._cards.Count, (index + 1) * this._rowSize);
				int num2 = Math.Min(num, index * this._rowSize);
				cardsRow.SetCards(index, (num2 == num) ? Enumerable.Empty<Card>() : Enumerable.Take<Card>(Enumerable.Skip<Card>(this._cards, num2), num));
			}
			private readonly int _rowSize;
			private readonly List<Card> _cards;
		}
		private enum AchievementStatus
		{
			All,
			Unlock,
			NotUnlock
		}
	}
}
