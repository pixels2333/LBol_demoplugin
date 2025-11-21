using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LBoL.Base;
using LBoL.Core;
using LBoL.Presentation.UI;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.I10N
{
	// Token: 0x020000F5 RID: 245
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LocalizedText : MonoBehaviour
	{
		// Token: 0x06000DE8 RID: 3560 RVA: 0x00042C28 File Offset: 0x00040E28
		private float GetResize(Locale locale)
		{
			float num;
			if (this.resizeTable.TryGetValue(locale, out num))
			{
				return num;
			}
			if (this.isGameData)
			{
				return 1f;
			}
			Locale locale2;
			switch (locale)
			{
			case Locale.En:
				locale2 = Locale.En;
				break;
			case Locale.ZhHans:
				locale2 = Locale.ZhHans;
				break;
			case Locale.ZhHant:
				locale2 = Locale.ZhHans;
				break;
			case Locale.Ja:
				locale2 = Locale.ZhHans;
				break;
			case Locale.Ru:
				locale2 = Locale.En;
				break;
			case Locale.Es:
				locale2 = Locale.En;
				break;
			case Locale.Pl:
				locale2 = Locale.En;
				break;
			case Locale.Pt:
				locale2 = Locale.En;
				break;
			case Locale.Fr:
				locale2 = Locale.En;
				break;
			case Locale.Tr:
				locale2 = Locale.En;
				break;
			case Locale.Ko:
				locale2 = Locale.ZhHans;
				break;
			case Locale.Vi:
				locale2 = Locale.En;
				break;
			case Locale.It:
				locale2 = Locale.En;
				break;
			case Locale.De:
				locale2 = Locale.En;
				break;
			case Locale.Uk:
				locale2 = Locale.Ru;
				break;
			case Locale.Hu:
				locale2 = Locale.En;
				break;
			default:
				throw new ArgumentOutOfRangeException("locale", locale, null);
			}
			Locale locale3 = locale2;
			float num2;
			if (locale3 != locale && this.resizeTable.TryGetValue(locale3, out num2))
			{
				return num2;
			}
			return LocalizedText.FinalFallbackResizeTable[locale];
		}

		// Token: 0x06000DE9 RID: 3561 RVA: 0x00042D14 File Offset: 0x00040F14
		private void Awake()
		{
			this._textComponent = base.GetComponent<TextMeshProUGUI>();
			this._originSize = this._textComponent.fontSize;
			this._originSpacing = this._textComponent.characterSpacing;
			this._originFont = this._textComponent.font;
			this._originMaterial = this._textComponent.fontSharedMaterial;
			if (this.replaceMaterial != null)
			{
				this.SetFontMaterial(this._originMaterial);
			}
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x00042D8B File Offset: 0x00040F8B
		private void OnEnable()
		{
			this.OnLocaleChanged();
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x00042DA4 File Offset: 0x00040FA4
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x00042DB8 File Offset: 0x00040FB8
		private void SetFontMaterial(Material mat)
		{
			if (this.replaceMaterial == null)
			{
				this._textComponent.fontSharedMaterial = mat;
				return;
			}
			Material material = Object.Instantiate<Material>(this._textComponent.fontSharedMaterial);
			this._textComponent.fontSharedMaterial = LocalizationManager.Instance.CopyFontMatStyle(material, this.replaceMaterial);
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x00042E10 File Offset: 0x00041010
		private void OnLocaleChanged()
		{
			Locale currentLocale = Localization.CurrentLocale;
			if (!string.IsNullOrWhiteSpace(this.key))
			{
				this._textComponent.text = Localization.Localize(this.key, true);
			}
			float num = 1f;
			if (!this.isGameData)
			{
				ValueTuple<TMP_FontAsset, Material, float>? currentLocaleFontAndMaterial = LocalizationManager.Instance.GetCurrentLocaleFontAndMaterial(this._originFont, this._originMaterial);
				if (currentLocaleFontAndMaterial != null)
				{
					this._textComponent.font = currentLocaleFontAndMaterial.Value.Item1;
					this.SetFontMaterial(currentLocaleFontAndMaterial.Value.Item2);
					num = currentLocaleFontAndMaterial.Value.Item3;
				}
			}
			this._textComponent.fontSize = this._originSize * this.GetResize(currentLocale) * num;
			this._textComponent.characterSpacing = CollectionExtensions.GetValueOrDefault<Locale, float>(this.spacingTable, currentLocale, this._originSpacing);
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x00042EE4 File Offset: 0x000410E4
		public bool Validate(List<LocaleFontReplaceEntry> table)
		{
			bool flag = true;
			if (Enumerable.FirstOrDefault<LocaleFontReplaceEntry>(table, (LocaleFontReplaceEntry e) => e.font == base.GetComponent<TextMeshProUGUI>().font) == null)
			{
				Debug.LogWarning(this.<Validate>g__GetPath|17_0() + " 的字体未在LocalizationManager内定义");
				flag = false;
			}
			if (Enumerable.FirstOrDefault<LocaleFontReplaceEntry>(table, (LocaleFontReplaceEntry e) => e.material.name == base.GetComponent<TextMeshProUGUI>().fontSharedMaterial.name.Replace(" (Instance)", "")) == null)
			{
				Debug.LogWarning(this.<Validate>g__GetPath|17_0() + " 的材质未在LocalizationManager内定义");
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000DF0 RID: 3568 RVA: 0x00042F54 File Offset: 0x00041154
		// Note: this type is marked as 'beforefieldinit'.
		static LocalizedText()
		{
			Dictionary<Locale, float> dictionary = new Dictionary<Locale, float>();
			dictionary.Add(Locale.En, 0.8f);
			dictionary.Add(Locale.ZhHans, 1f);
			dictionary.Add(Locale.ZhHant, 1f);
			dictionary.Add(Locale.Ja, 1f);
			dictionary.Add(Locale.Ru, 0.8f);
			dictionary.Add(Locale.Es, 0.8f);
			dictionary.Add(Locale.Pl, 0.8f);
			dictionary.Add(Locale.Pt, 0.8f);
			dictionary.Add(Locale.Fr, 0.8f);
			dictionary.Add(Locale.Tr, 0.8f);
			dictionary.Add(Locale.Ko, 1f);
			dictionary.Add(Locale.Vi, 0.8f);
			dictionary.Add(Locale.It, 0.8f);
			dictionary.Add(Locale.De, 0.8f);
			dictionary.Add(Locale.Uk, 0.8f);
			dictionary.Add(Locale.Hu, 0.8f);
			LocalizedText.FinalFallbackResizeTable = dictionary;
		}

		// Token: 0x06000DF1 RID: 3569 RVA: 0x00043034 File Offset: 0x00041234
		[CompilerGenerated]
		private string <Validate>g__GetPath|17_0()
		{
			GameObject gameObject = base.gameObject;
			StringBuilder stringBuilder = new StringBuilder(gameObject.name);
			Transform transform = gameObject.transform.parent;
			while (transform != null)
			{
				stringBuilder.Insert(0, transform.name + "/");
				transform = transform.parent;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x04000A69 RID: 2665
		private static readonly Dictionary<Locale, float> FinalFallbackResizeTable;

		// Token: 0x04000A6A RID: 2666
		private TextMeshProUGUI _textComponent;

		// Token: 0x04000A6B RID: 2667
		[SerializeField]
		private string key;

		// Token: 0x04000A6C RID: 2668
		[SerializeField]
		private bool isGameData;

		// Token: 0x04000A6D RID: 2669
		[SerializeField]
		private AssociationList<Locale, float> resizeTable;

		// Token: 0x04000A6E RID: 2670
		[SerializeField]
		private AssociationList<Locale, float> spacingTable;

		// Token: 0x04000A6F RID: 2671
		[SerializeField]
		private Material replaceMaterial;

		// Token: 0x04000A70 RID: 2672
		private float _originSize;

		// Token: 0x04000A71 RID: 2673
		private float _originSpacing;

		// Token: 0x04000A72 RID: 2674
		private TMP_FontAsset _originFont;

		// Token: 0x04000A73 RID: 2675
		private Material _originMaterial;
	}
}
