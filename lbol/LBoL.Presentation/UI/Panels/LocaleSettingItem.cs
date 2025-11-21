using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A0 RID: 160
	public sealed class LocaleSettingItem
	{
		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000842 RID: 2114 RVA: 0x00027D1B File Offset: 0x00025F1B
		// (set) Token: 0x06000843 RID: 2115 RVA: 0x00027D23 File Offset: 0x00025F23
		public string Name { get; private set; }

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000844 RID: 2116 RVA: 0x00027D2C File Offset: 0x00025F2C
		// (set) Token: 0x06000845 RID: 2117 RVA: 0x00027D34 File Offset: 0x00025F34
		public Locale Locale { get; private set; }

		// Token: 0x06000846 RID: 2118 RVA: 0x00027D40 File Offset: 0x00025F40
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

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000847 RID: 2119 RVA: 0x00027D7D File Offset: 0x00025F7D
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

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000848 RID: 2120 RVA: 0x00027DB2 File Offset: 0x00025FB2
		public static IReadOnlyList<LocaleSettingItem> AllNoLocale
		{
			get
			{
				return LocaleSettingItem._locales.AsReadOnly();
			}
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x00027DC8 File Offset: 0x00025FC8
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

		// Token: 0x040005E9 RID: 1513
		private static readonly List<LocaleSettingItem> _locales;
	}
}
