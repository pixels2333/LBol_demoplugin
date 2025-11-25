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
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LocalizedText : MonoBehaviour
	{
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
		private void OnEnable()
		{
			this.OnLocaleChanged();
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}
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
		private static readonly Dictionary<Locale, float> FinalFallbackResizeTable;
		private TextMeshProUGUI _textComponent;
		[SerializeField]
		private string key;
		[SerializeField]
		private bool isGameData;
		[SerializeField]
		private AssociationList<Locale, float> resizeTable;
		[SerializeField]
		private AssociationList<Locale, float> spacingTable;
		[SerializeField]
		private Material replaceMaterial;
		private float _originSize;
		private float _originSpacing;
		private TMP_FontAsset _originFont;
		private Material _originMaterial;
	}
}
