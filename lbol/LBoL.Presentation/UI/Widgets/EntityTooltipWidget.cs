using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.I10N;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000054 RID: 84
	public class EntityTooltipWidget : MonoBehaviour
	{
		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060004CE RID: 1230 RVA: 0x00013F2B File Offset: 0x0001212B
		public RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x00013F38 File Offset: 0x00012138
		private void Awake()
		{
			Vector2 sizeDelta = this.root.sizeDelta;
			this._initWidth = sizeDelta.x;
			this._initHeight = sizeDelta.y - this.dwNameTemplate.rectTransform.sizeDelta.y - this.dwDescriptionTemplate.rectTransform.sizeDelta.y - this.flavorText.rectTransform.sizeDelta.y;
			this._dwOffsetBase = this.dwNameTemplate.transform.localPosition;
			this.dwNameTemplate.gameObject.SetActive(false);
			this.dwDescriptionTemplate.gameObject.SetActive(false);
			this.extraTextRoot.gameObject.SetActive(false);
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x00013FF9 File Offset: 0x000121F9
		private void OnEnable()
		{
			if (!this.instantShow)
			{
				this.canvasGroup.DOFade(1f, 0.1f).From(0f, true, false).SetDelay(0.2f)
					.SetUpdate(true);
			}
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x00014035 File Offset: 0x00012235
		private void OnDisable()
		{
			this.canvasGroup.DOKill(false);
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x00014044 File Offset: 0x00012244
		private bool SetContent(string title, Rarity rarity, IEnumerable<Keyword> keyWords, IEnumerable<IDisplayWord> displayWords, [CanBeNull] string flvText = null, [CanBeNull] string mainText = null)
		{
			bool flag = false;
			float num = this._initWidth;
			float num2 = this.InternalSetContent(title, rarity, keyWords, displayWords, flvText, mainText);
			if (num2 > 900f || GameMaster.PreferWideTooltips)
			{
				num = this._initWidth * 1.5f;
				num2 = this.ResetWidth(num);
				flag = true;
			}
			this.root.sizeDelta = new Vector2(num, (num2 > this.minHeight) ? num2 : this.minHeight);
			return flag;
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x000140B4 File Offset: 0x000122B4
		private float InternalSetContent(string title, Rarity rarity, IEnumerable<Keyword> keyWords, IEnumerable<IDisplayWord> displayWords, string flvText, string mainText)
		{
			float num = this._initHeight;
			Vector2 dwOffsetBase = this._dwOffsetBase;
			this.titleText.text = title;
			this.rarityText.text = ("Rarity." + rarity.ToString()).Localize(true);
			Sprite sprite;
			if (this.raritySpriteTable.TryGetValue(rarity, out sprite))
			{
				this.rarityBar.sprite = sprite;
			}
			else
			{
				Debug.Log(string.Format("Sprite for rarity '{0}' not found", rarity));
				this.rarityBar.sprite = null;
			}
			if (mainText != null)
			{
				this.mainDescription.text = mainText;
				float y = UiUtils.GetPreferredSize(this.mainDescription, this._initWidth, float.MaxValue).y;
				num += y;
				dwOffsetBase.y -= y;
			}
			bool showVerboseKeywords = GameMaster.ShowVerboseKeywords;
			foreach (Keyword keyword in keyWords)
			{
				KeywordDisplayWord displayWord = Keywords.GetDisplayWord(keyword);
				if (!displayWord.IsHidden && (!displayWord.IsVerbose || showVerboseKeywords))
				{
					TextMeshProUGUI textMeshProUGUI = Object.Instantiate<TextMeshProUGUI>(this.dwNameTemplate, this.root);
					textMeshProUGUI.gameObject.SetActive(true);
					textMeshProUGUI.text = displayWord.Name;
					textMeshProUGUI.transform.localPosition = dwOffsetBase;
					float y2 = UiUtils.GetPreferredSize(textMeshProUGUI, this._initWidth, float.MaxValue).y;
					num += y2;
					dwOffsetBase.y -= y2;
					TextMeshProUGUI textMeshProUGUI2 = Object.Instantiate<TextMeshProUGUI>(this.dwDescriptionTemplate, this.root);
					textMeshProUGUI2.gameObject.SetActive(true);
					textMeshProUGUI2.text = displayWord.Description;
					textMeshProUGUI2.transform.localPosition = dwOffsetBase;
					float y3 = UiUtils.GetPreferredSize(textMeshProUGUI2, this._initWidth, float.MaxValue).y;
					num += y3;
					dwOffsetBase.y -= y3;
					this._dws.Add(textMeshProUGUI);
					this._dws.Add(textMeshProUGUI2);
				}
			}
			foreach (IDisplayWord displayWord2 in displayWords)
			{
				TextMeshProUGUI textMeshProUGUI3 = Object.Instantiate<TextMeshProUGUI>(this.dwNameTemplate, this.root);
				textMeshProUGUI3.gameObject.SetActive(true);
				textMeshProUGUI3.text = displayWord2.Name;
				textMeshProUGUI3.transform.localPosition = dwOffsetBase;
				float y4 = UiUtils.GetPreferredSize(textMeshProUGUI3, this._initWidth, float.MaxValue).y;
				num += y4;
				dwOffsetBase.y -= y4;
				TextMeshProUGUI textMeshProUGUI4 = Object.Instantiate<TextMeshProUGUI>(this.dwDescriptionTemplate, this.root);
				textMeshProUGUI4.gameObject.SetActive(true);
				textMeshProUGUI4.text = displayWord2.Description;
				textMeshProUGUI4.transform.localPosition = dwOffsetBase;
				float y5 = UiUtils.GetPreferredSize(textMeshProUGUI4, this._initWidth, float.MaxValue).y;
				num += y5;
				dwOffsetBase.y -= y5;
				this._dws.Add(textMeshProUGUI3);
				this._dws.Add(textMeshProUGUI4);
			}
			this.flavorText.text = flvText ?? "";
			if (L10nManager.Info.PreferItalicInFlavor)
			{
				this.flavorText.fontStyle |= FontStyles.Italic;
			}
			else if ((this.flavorText.fontStyle & FontStyles.Italic) != FontStyles.Normal)
			{
				this.flavorText.fontStyle ^= FontStyles.Italic;
			}
			float num2 = UiUtils.GetPreferredSize(this.flavorText, this._initWidth, float.MaxValue).y;
			num2 = Mathf.Max(10f, num2);
			this.flavorText.rectTransform.sizeDelta = new Vector2(0f, num2);
			num += num2;
			dwOffsetBase.y -= num2 + 10f;
			return num;
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x000144D8 File Offset: 0x000126D8
		private float ResetWidth(float width)
		{
			float num = this._initHeight;
			Vector2 dwOffsetBase = this._dwOffsetBase;
			if (this.mainDescription.text != null)
			{
				float y = UiUtils.GetPreferredSize(this.mainDescription, width, float.MaxValue).y;
				num += y;
				dwOffsetBase.y -= y;
			}
			foreach (TextMeshProUGUI textMeshProUGUI in this._dws)
			{
				textMeshProUGUI.transform.localPosition = dwOffsetBase;
				float y2 = UiUtils.GetPreferredSize(textMeshProUGUI, width, float.MaxValue).y;
				num += y2;
				dwOffsetBase.y -= y2;
			}
			float num2 = UiUtils.GetPreferredSize(this.flavorText, width, float.MaxValue).y;
			num2 = Mathf.Max(10f, num2);
			this.flavorText.rectTransform.sizeDelta = new Vector2(0f, num2);
			num += num2;
			dwOffsetBase.y -= num2 + 10f;
			return num;
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x000145F0 File Offset: 0x000127F0
		public void SetCard(Card card, bool topLeftAlign = false)
		{
			this.mainDescription.enabled = card.DetailText != null;
			IReadOnlyList<ManaColor> colors = card.Config.Colors;
			string text = "";
			bool flag = false;
			int count = colors.Count;
			if (count <= 2)
			{
				switch (count)
				{
				case 0:
					text = "Colorless";
					break;
				case 1:
				{
					ManaColor manaColor = colors[0];
					if (manaColor == ManaColor.Any || manaColor == ManaColor.Philosophy)
					{
						Debug.LogError(string.Format("Invalid card color for {0}: {1}", card.DebugName, manaColor));
						flag = true;
					}
					text = manaColor.ToLongName();
					break;
				}
				case 2:
				{
					ManaColor manaColor2 = colors[0];
					ManaColor manaColor3 = colors[1];
					if (manaColor2 == manaColor3)
					{
						Debug.LogError(string.Format("Invalid card color for {0}: Two same color {1}", card.DebugName, manaColor2));
						flag = true;
					}
					else if (manaColor2 == ManaColor.Any || manaColor2 == ManaColor.Philosophy || manaColor3 == ManaColor.Any || manaColor3 == ManaColor.Philosophy)
					{
						Debug.LogError(string.Format("Invalid card color for {0}: {1} {2}", card.DebugName, manaColor2, manaColor3));
						flag = true;
					}
					else if (manaColor2 == ManaColor.Colorless || manaColor3 == ManaColor.Colorless)
					{
						text = "MultiColor";
					}
					else
					{
						if (manaColor2 > manaColor3)
						{
							ManaColor manaColor4 = manaColor3;
							ManaColor manaColor5 = manaColor2;
							manaColor2 = manaColor4;
							manaColor3 = manaColor5;
						}
						if (GameMaster.IsLoopOrder)
						{
							ManaColors.GetLoopOrder(manaColor2, manaColor3, out manaColor2, out manaColor3);
						}
						else if (manaColor2 > manaColor3)
						{
							ManaColor manaColor6 = manaColor3;
							ManaColor manaColor5 = manaColor2;
							manaColor2 = manaColor6;
							manaColor3 = manaColor5;
						}
						text = manaColor2.ToShortName().ToString() + manaColor3.ToShortName().ToString();
					}
					break;
				}
				}
			}
			else
			{
				text = "MultiColor";
			}
			string text2;
			if (flag)
			{
				text2 = "<Invalid Color>";
			}
			else
			{
				text = "Tooltip.EntityTitle." + text;
				string owner = card.Config.Owner;
				if (string.IsNullOrEmpty(owner))
				{
					text2 = "Tooltip.EntityTitle.NeutralTitle".LocalizeFormat(new object[]
					{
						text.Localize(true),
						"Tooltip.EntityTitle.Neutral".Localize(true)
					});
				}
				else
				{
					UnitName name = UnitNameTable.GetName(owner, null);
					text2 = "Tooltip.EntityTitle.CharacterTitle".LocalizeFormat(new object[]
					{
						L10nManager.Info.PreferShortName ? name.ToString("s") : name.ToString(),
						"Tooltip.EntityTitle.Character".Localize(true),
						text.Localize(true)
					});
				}
			}
			this.SetContent(text2, card.Config.Rarity, card.EnumerateCardKeywords(), card.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), card.FlavorText, card.DetailText);
			float num = 0f;
			if (card.IsUpgraded)
			{
				this.upgradeText.gameObject.SetActive(true);
				this.upgradeText.color = new Color32(95, byte.MaxValue, 153, byte.MaxValue);
				this.upgradeText.text = "Cards.IsUpgraded".Localize(true);
			}
			else if (!card.CanUpgrade)
			{
				this.upgradeText.gameObject.SetActive(true);
				this.upgradeText.color = new Color32(240, 84, 85, byte.MaxValue);
				this.upgradeText.text = "Cards.UnUpgradable".Localize(true);
			}
			else
			{
				this.upgradeText.gameObject.SetActive(false);
			}
			this.poolText.gameObject.SetActive(!card.Config.IsPooled);
			if (this.upgradeText.gameObject.activeSelf || this.poolText.gameObject.activeSelf)
			{
				this.extraTextRoot.gameObject.SetActive(true);
				num = this.extraTextRoot.sizeDelta.y;
			}
			else
			{
				this.extraTextRoot.gameObject.SetActive(false);
			}
			if (!topLeftAlign && this.withExtraHeight)
			{
				this.root.localPosition = new Vector3(0f, num / 2f, 0f);
			}
			else
			{
				this.root.localPosition = Vector3.zero;
			}
			this.RectTransform.sizeDelta = this.root.sizeDelta + new Vector2(0f, num);
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x00014A08 File Offset: 0x00012C08
		public void SetStatusEffect(StatusEffect effect)
		{
			string text = effect.Name;
			if (effect.ShowPlusByLimit && effect.Limit == 1)
			{
				text += "+";
			}
			this.SetContent(text, Rarity.Common, new List<Keyword>(), effect.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), null, effect.Description);
			TextMeshProUGUI textMeshProUGUI = this.rarityText;
			string text2;
			switch (effect.Type)
			{
			case StatusEffectType.Positive:
				text2 = "StatusEffectType.Positive".Localize(true);
				break;
			case StatusEffectType.Negative:
				text2 = "StatusEffectType.Negative".Localize(true);
				break;
			case StatusEffectType.Special:
				text2 = "StatusEffectType.Special".Localize(true);
				break;
			default:
				throw new ArgumentOutOfRangeException("Type", effect.Type, null);
			}
			textMeshProUGUI.text = text2;
			textMeshProUGUI = this.rarityText;
			Color32 color;
			switch (effect.Type)
			{
			case StatusEffectType.Positive:
				color = new Color32(124, 241, 88, byte.MaxValue);
				break;
			case StatusEffectType.Negative:
				color = new Color32(231, 58, 75, byte.MaxValue);
				break;
			case StatusEffectType.Special:
				color = new Color32(116, 193, byte.MaxValue, byte.MaxValue);
				break;
			default:
				throw new ArgumentOutOfRangeException("Type", effect.Type, null);
			}
			textMeshProUGUI.color = color;
			this.root.localPosition = Vector3.zero;
			this.RectTransform.sizeDelta = this.root.sizeDelta;
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00014B78 File Offset: 0x00012D78
		public void SetExhibit(Exhibit exhibit)
		{
			bool flag = this.SetContent(exhibit.Name, exhibit.Config.Rarity, Enumerable.Empty<Keyword>(), exhibit.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), exhibit.FlavorText, exhibit.Description);
			this.rarityText.gameObject.SetActive(!L10nManager.Info.HideExhibitRarity || flag);
			this.root.localPosition = Vector3.zero;
			this.RectTransform.sizeDelta = this.root.sizeDelta;
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x00014C00 File Offset: 0x00012E00
		public void SetUltimateSkill(UltimateSkill skill)
		{
			string text = skill.Description;
			text += "<color=#C0C0D0A0>\n\n";
			text += string.Format("Ultimate.ConsumePower".Localize(true), skill.PowerCost);
			text += "\n";
			text += string.Format("Ultimate.MaxPower".Localize(true), skill.MaxPowerLevel);
			text += "\n";
			text += ("Ultimate." + skill.UsRepeatableType.ToString()).Localize(true);
			text += "</color>";
			this.SetContent("Tooltip.UsTitle".LocalizeFormat(new object[] { skill.Title, skill.Content }), Rarity.Shining, Enumerable.Empty<Keyword>(), skill.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), null, text);
			this.rarityText.gameObject.SetActive(false);
			this.root.localPosition = Vector3.zero;
			this.RectTransform.sizeDelta = this.root.sizeDelta;
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00014D28 File Offset: 0x00012F28
		public void SetDoll(Doll doll)
		{
			this.SetContent(doll.Name, Rarity.Common, Enumerable.Empty<Keyword>(), doll.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), null, doll.Description);
			this.rarityText.gameObject.SetActive(false);
			this.root.localPosition = Vector3.zero;
			this.RectTransform.sizeDelta = this.root.sizeDelta;
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x00014D94 File Offset: 0x00012F94
		public void SetAchievement(string title, string des)
		{
			this.SetContent(title, Rarity.Common, Enumerable.Empty<Keyword>(), ArraySegment<IDisplayWord>.Empty, null, des);
			this.rarityText.gameObject.SetActive(false);
			this.root.localPosition = Vector3.zero;
			this.RectTransform.sizeDelta = this.root.sizeDelta;
		}

		// Token: 0x04000293 RID: 659
		[SerializeField]
		private RectTransform root;

		// Token: 0x04000294 RID: 660
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x04000295 RID: 661
		[SerializeField]
		private Image rarityBar;

		// Token: 0x04000296 RID: 662
		[SerializeField]
		private TextMeshProUGUI titleText;

		// Token: 0x04000297 RID: 663
		[SerializeField]
		private TextMeshProUGUI rarityText;

		// Token: 0x04000298 RID: 664
		[SerializeField]
		private TextMeshProUGUI mainDescription;

		// Token: 0x04000299 RID: 665
		[SerializeField]
		private TextMeshProUGUI dwNameTemplate;

		// Token: 0x0400029A RID: 666
		[SerializeField]
		private TextMeshProUGUI dwDescriptionTemplate;

		// Token: 0x0400029B RID: 667
		[SerializeField]
		private TextMeshProUGUI flavorText;

		// Token: 0x0400029C RID: 668
		[SerializeField]
		private RectTransform extraTextRoot;

		// Token: 0x0400029D RID: 669
		[SerializeField]
		private TextMeshProUGUI upgradeText;

		// Token: 0x0400029E RID: 670
		[SerializeField]
		private TextMeshProUGUI poolText;

		// Token: 0x0400029F RID: 671
		[SerializeField]
		private float minHeight;

		// Token: 0x040002A0 RID: 672
		[SerializeField]
		private AssociationList<Rarity, Sprite> raritySpriteTable;

		// Token: 0x040002A1 RID: 673
		private float _initWidth;

		// Token: 0x040002A2 RID: 674
		private float _initHeight;

		// Token: 0x040002A3 RID: 675
		private Vector2 _dwOffsetBase;

		// Token: 0x040002A4 RID: 676
		public bool withExtraHeight = true;

		// Token: 0x040002A5 RID: 677
		private const float FadeInDuration = 0.1f;

		// Token: 0x040002A6 RID: 678
		private const float FadeInDelay = 0.2f;

		// Token: 0x040002A7 RID: 679
		[Header("Appearance")]
		public bool instantShow;

		// Token: 0x040002A8 RID: 680
		private const float MaxHeight = 900f;

		// Token: 0x040002A9 RID: 681
		private readonly List<TextMeshProUGUI> _dws = new List<TextMeshProUGUI>();
	}
}
