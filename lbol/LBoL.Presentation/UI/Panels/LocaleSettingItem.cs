using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
namespace LBoL.Presentation.UI.Panels
{
	public sealed class LocaleSettingItem
	{
		public string Name { get; private set; }
		public Locale Locale { get; private set; }
		private static string GetCurrentLocaleName(string name, Locale locale)
		{
			Locale currentLocale = Localization.CurrentLocale;
			if (locale == currentLocale)
			{
				return name;
			}
			string text = locale.ToLocaleName();
			if (text != "")
			{
				return name + " / " + text;
			}
			return name;
		}
		public static IReadOnlyList<LocaleSettingItem> All
		{
			get
			{
				return Enumerable.ToList<LocaleSettingItem>(Enumerable.Select<LocaleSettingItem, LocaleSettingItem>(LocaleSettingItem._locales, (LocaleSettingItem item) => new LocaleSettingItem
				{
					Name = LocaleSettingItem.GetCurrentLocaleName(item.Name, item.Locale),
					Locale = item.Locale
				})).AsReadOnly();
			}
		}
		public static IReadOnlyList<LocaleSettingItem> AllNoLocale
		{
			get
			{
				return LocaleSettingItem._locales.AsReadOnly();
			}
		}
		// Note: this type is marked as 'beforefieldinit'.
		static LocaleSettingItem()
		{
			List<LocaleSettingItem> list = new List<LocaleSettingItem>();
			list.Add(new LocaleSettingItem
			{
				Name = "English",
				Locale = Locale.En
			});
			list.Add(new LocaleSettingItem
			{
				Name = "简体中文",
				Locale = Locale.ZhHans
			});
			list.Add(new LocaleSettingItem
			{
				Name = "繁體中文",
				Locale = Locale.ZhHant
			});
			list.Add(new LocaleSettingItem
			{
				Name = "日本語",
				Locale = Locale.Ja
			});
			list.Add(new LocaleSettingItem
			{
				Name = "한국어",
				Locale = Locale.Ko
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Русский",
				Locale = Locale.Ru
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Polski",
				Locale = Locale.Pl
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Français",
				Locale = Locale.Fr
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Español",
				Locale = Locale.Es
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Português",
				Locale = Locale.Pt
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Tiếng Việt",
				Locale = Locale.Vi
			});
			list.Add(new LocaleSettingItem
			{
				Name = "Українська",
				Locale = Locale.Uk
			});
			LocaleSettingItem._locales = list;
		}
		private static readonly List<LocaleSettingItem> _locales;
	}
}
