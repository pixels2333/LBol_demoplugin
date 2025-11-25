using System;
using System.Collections.Generic;
namespace LBoL.Core
{
	public static class LocaleExtensions
	{
		public static string ToTag(this Locale locale)
		{
			string text;
			switch (locale)
			{
			case Locale.En:
				text = "en";
				break;
			case Locale.ZhHans:
				text = "zh-Hans";
				break;
			case Locale.ZhHant:
				text = "zh-Hant";
				break;
			case Locale.Ja:
				text = "ja";
				break;
			case Locale.Ru:
				text = "ru";
				break;
			case Locale.Es:
				text = "es";
				break;
			case Locale.Pl:
				text = "pl";
				break;
			case Locale.Pt:
				text = "pt";
				break;
			case Locale.Fr:
				text = "fr";
				break;
			case Locale.Tr:
				text = "tr";
				break;
			case Locale.Ko:
				text = "ko";
				break;
			case Locale.Vi:
				text = "vi";
				break;
			case Locale.It:
				text = "it";
				break;
			case Locale.De:
				text = "de";
				break;
			case Locale.Uk:
				text = "uk";
				break;
			case Locale.Hu:
				text = "hu";
				break;
			default:
				throw new ArgumentOutOfRangeException("locale", locale, null);
			}
			return text;
		}
		public static string ToAlpha2Name(this Locale locale)
		{
			string text;
			switch (locale)
			{
			case Locale.En:
				text = "en";
				break;
			case Locale.ZhHans:
				text = "zh";
				break;
			case Locale.ZhHant:
				text = "zh";
				break;
			case Locale.Ja:
				text = "ja";
				break;
			case Locale.Ru:
				text = "ru";
				break;
			case Locale.Es:
				text = "es";
				break;
			case Locale.Pl:
				text = "pl";
				break;
			case Locale.Pt:
				text = "pt";
				break;
			case Locale.Fr:
				text = "fr";
				break;
			case Locale.Tr:
				text = "tr";
				break;
			case Locale.Ko:
				text = "ko";
				break;
			case Locale.Vi:
				text = "vi";
				break;
			case Locale.It:
				text = "it";
				break;
			case Locale.De:
				text = "de";
				break;
			case Locale.Uk:
				text = "uk";
				break;
			case Locale.Hu:
				text = "hu";
				break;
			default:
				throw new ArgumentOutOfRangeException("locale", locale, null);
			}
			return text;
		}
		public static Locale? TryParseLocaleTag(this string tag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(tag);
			if (num <= 1194886160U)
			{
				if (num <= 1092248970U)
				{
					if (num <= 89862570U)
					{
						if (num != 5974475U)
						{
							if (num == 89862570U)
							{
								if (tag == "zh-Hans")
								{
									return new Locale?(Locale.ZhHans);
								}
							}
						}
						else if (tag == "zh-Hant")
						{
							return new Locale?(Locale.ZhHant);
						}
					}
					else if (num != 1011465184U)
					{
						if (num == 1092248970U)
						{
							if (tag == "en")
							{
								return new Locale?(Locale.En);
							}
						}
					}
					else if (tag == "vi")
					{
						return new Locale?(Locale.Vi);
					}
				}
				else if (num <= 1162757945U)
				{
					if (num != 1111292255U)
					{
						if (num == 1162757945U)
						{
							if (tag == "pl")
							{
								return new Locale?(Locale.Pl);
							}
						}
					}
					else if (tag == "ko")
					{
						return new Locale?(Locale.Ko);
					}
				}
				else if (num != 1176137065U)
				{
					if (num == 1194886160U)
					{
						if (tag == "it")
						{
							return new Locale?(Locale.It);
						}
					}
				}
				else if (tag == "es")
				{
					return new Locale?(Locale.Es);
				}
			}
			else if (num <= 1461901041U)
			{
				if (num <= 1213488160U)
				{
					if (num != 1195724803U)
					{
						if (num == 1213488160U)
						{
							if (tag == "ru")
							{
								return new Locale?(Locale.Ru);
							}
						}
					}
					else if (tag == "tr")
					{
						return new Locale?(Locale.Tr);
					}
				}
				else if (num != 1278921350U)
				{
					if (num == 1461901041U)
					{
						if (tag == "fr")
						{
							return new Locale?(Locale.Fr);
						}
					}
				}
				else if (tag == "hu")
				{
					return new Locale?(Locale.Hu);
				}
			}
			else if (num <= 1565420801U)
			{
				if (num != 1545391778U)
				{
					if (num == 1565420801U)
					{
						if (tag == "pt")
						{
							return new Locale?(Locale.Pt);
						}
					}
				}
				else if (tag == "de")
				{
					return new Locale?(Locale.De);
				}
			}
			else if (num != 1581462945U)
			{
				if (num == 1816099348U)
				{
					if (tag == "ja")
					{
						return new Locale?(Locale.Ja);
					}
				}
			}
			else if (tag == "uk")
			{
				return new Locale?(Locale.Uk);
			}
			return default(Locale?);
		}
		public static string ToLocaleName(this Locale locale)
		{
			Dictionary<Locale, string> dictionary;
			string text;
			if (LocaleExtensions.LocaleNamesTable.TryGetValue(Localization.CurrentLocale, ref dictionary) && dictionary.TryGetValue(locale, ref text))
			{
				return text;
			}
			return string.Empty;
		}
		// Note: this type is marked as 'beforefieldinit'.
		static LocaleExtensions()
		{
			Dictionary<Locale, Dictionary<Locale, string>> dictionary = new Dictionary<Locale, Dictionary<Locale, string>>();
			Locale locale = Locale.En;
			Dictionary<Locale, string> dictionary2 = new Dictionary<Locale, string>();
			dictionary2.Add(Locale.En, "English");
			dictionary2.Add(Locale.ZhHans, "Simplified Chinese");
			dictionary2.Add(Locale.ZhHant, "Traditional Chinese");
			dictionary2.Add(Locale.Ja, "Japanese");
			dictionary2.Add(Locale.Ru, "Russian");
			dictionary2.Add(Locale.Es, "Spanish");
			dictionary2.Add(Locale.Pl, "Polish");
			dictionary2.Add(Locale.Pt, "Portuguese");
			dictionary2.Add(Locale.Fr, "French");
			dictionary2.Add(Locale.Tr, "Turkish");
			dictionary2.Add(Locale.Ko, "Korean");
			dictionary2.Add(Locale.Vi, "Vietnamese");
			dictionary2.Add(Locale.It, "Italian");
			dictionary2.Add(Locale.De, "German");
			dictionary2.Add(Locale.Uk, "Ukrainian");
			dictionary2.Add(Locale.Hu, "Hungarian");
			dictionary.Add(locale, dictionary2);
			Locale locale2 = Locale.ZhHans;
			Dictionary<Locale, string> dictionary3 = new Dictionary<Locale, string>();
			dictionary3.Add(Locale.En, "英语");
			dictionary3.Add(Locale.ZhHans, "简体中文");
			dictionary3.Add(Locale.ZhHant, "繁体中文");
			dictionary3.Add(Locale.Ja, "日语");
			dictionary3.Add(Locale.Ru, "俄语");
			dictionary3.Add(Locale.Es, "西班牙语");
			dictionary3.Add(Locale.Pl, "波兰语");
			dictionary3.Add(Locale.Pt, "葡萄牙语");
			dictionary3.Add(Locale.Fr, "法语");
			dictionary3.Add(Locale.Tr, "土耳其语");
			dictionary3.Add(Locale.Ko, "韩语");
			dictionary3.Add(Locale.Vi, "越南语");
			dictionary3.Add(Locale.It, "意大利语");
			dictionary3.Add(Locale.De, "德语");
			dictionary3.Add(Locale.Uk, "乌克兰语");
			dictionary3.Add(Locale.Hu, "匈牙利语");
			dictionary.Add(locale2, dictionary3);
			Locale locale3 = Locale.ZhHant;
			Dictionary<Locale, string> dictionary4 = new Dictionary<Locale, string>();
			dictionary4.Add(Locale.En, "英文");
			dictionary4.Add(Locale.ZhHans, "簡體中文");
			dictionary4.Add(Locale.ZhHant, "繁體中文");
			dictionary4.Add(Locale.Ja, "日文");
			dictionary4.Add(Locale.Ru, "俄文");
			dictionary4.Add(Locale.Es, "西班牙文");
			dictionary4.Add(Locale.Pl, "波蘭文");
			dictionary4.Add(Locale.Pt, "葡萄牙文");
			dictionary4.Add(Locale.Fr, "法文");
			dictionary4.Add(Locale.Tr, "土耳其文");
			dictionary4.Add(Locale.Ko, "韓文");
			dictionary4.Add(Locale.Vi, "越南文");
			dictionary4.Add(Locale.It, "意大利文");
			dictionary4.Add(Locale.De, "德文");
			dictionary4.Add(Locale.Uk, "烏克蘭文");
			dictionary4.Add(Locale.Hu, "匈牙利文");
			dictionary.Add(locale3, dictionary4);
			Locale locale4 = Locale.Ja;
			Dictionary<Locale, string> dictionary5 = new Dictionary<Locale, string>();
			dictionary5.Add(Locale.En, "英語");
			dictionary5.Add(Locale.ZhHans, "簡体字中国語");
			dictionary5.Add(Locale.ZhHant, "繁体字中国語");
			dictionary5.Add(Locale.Ja, "日本語");
			dictionary5.Add(Locale.Ru, "ロシア語");
			dictionary5.Add(Locale.Es, "スペイン語");
			dictionary5.Add(Locale.Pl, "ポーランド語");
			dictionary5.Add(Locale.Pt, "ポルトガル語");
			dictionary5.Add(Locale.Fr, "フランス語");
			dictionary5.Add(Locale.Tr, "トルコ語");
			dictionary5.Add(Locale.Ko, "韓国語");
			dictionary5.Add(Locale.Vi, "ベトナム語");
			dictionary5.Add(Locale.It, "イタリア語");
			dictionary5.Add(Locale.De, "ドイツ語");
			dictionary5.Add(Locale.Uk, "ウクライナ語");
			dictionary5.Add(Locale.Hu, "ハンガリー語");
			dictionary.Add(locale4, dictionary5);
			Locale locale5 = Locale.Ru;
			Dictionary<Locale, string> dictionary6 = new Dictionary<Locale, string>();
			dictionary6.Add(Locale.En, "Английский");
			dictionary6.Add(Locale.ZhHans, "Упрощённый китайский");
			dictionary6.Add(Locale.ZhHant, "Традиционный китайский");
			dictionary6.Add(Locale.Ja, "Японский");
			dictionary6.Add(Locale.Ru, "Русский");
			dictionary6.Add(Locale.Es, "Испанский");
			dictionary6.Add(Locale.Pl, "Польский");
			dictionary6.Add(Locale.Pt, "Португальский");
			dictionary6.Add(Locale.Fr, "Французский");
			dictionary6.Add(Locale.Tr, "Турецкий");
			dictionary6.Add(Locale.Ko, "Корейский");
			dictionary6.Add(Locale.Vi, "Вьетнамский");
			dictionary6.Add(Locale.It, "Итальянский");
			dictionary6.Add(Locale.De, "Немецкий");
			dictionary6.Add(Locale.Uk, "Украинский");
			dictionary6.Add(Locale.Hu, "Венгерский");
			dictionary.Add(locale5, dictionary6);
			Locale locale6 = Locale.Es;
			Dictionary<Locale, string> dictionary7 = new Dictionary<Locale, string>();
			dictionary7.Add(Locale.En, "Inglés");
			dictionary7.Add(Locale.ZhHans, "Chino simplificado");
			dictionary7.Add(Locale.ZhHant, "Chino tradicional");
			dictionary7.Add(Locale.Ja, "Japonés");
			dictionary7.Add(Locale.Ru, "Ruso");
			dictionary7.Add(Locale.Es, "Español");
			dictionary7.Add(Locale.Pl, "Polaco");
			dictionary7.Add(Locale.Pt, "Portugués");
			dictionary7.Add(Locale.Fr, "Francés");
			dictionary7.Add(Locale.Tr, "Turco");
			dictionary7.Add(Locale.Ko, "Coreano");
			dictionary7.Add(Locale.Vi, "Vietnamita");
			dictionary7.Add(Locale.It, "Italiano");
			dictionary7.Add(Locale.De, "Alemán");
			dictionary7.Add(Locale.Uk, "Ucraniano");
			dictionary7.Add(Locale.Hu, "Húngaro");
			dictionary.Add(locale6, dictionary7);
			Locale locale7 = Locale.Pl;
			Dictionary<Locale, string> dictionary8 = new Dictionary<Locale, string>();
			dictionary8.Add(Locale.En, "Angielski");
			dictionary8.Add(Locale.ZhHans, "Chiński uproszczony");
			dictionary8.Add(Locale.ZhHant, "Chiński tradycyjny");
			dictionary8.Add(Locale.Ja, "Japoński");
			dictionary8.Add(Locale.Ru, "Rosyjski");
			dictionary8.Add(Locale.Es, "Hiszpański");
			dictionary8.Add(Locale.Pl, "Polski");
			dictionary8.Add(Locale.Pt, "Portugalski");
			dictionary8.Add(Locale.Fr, "Francuski");
			dictionary8.Add(Locale.Tr, "Turecki");
			dictionary8.Add(Locale.Ko, "Koreański");
			dictionary8.Add(Locale.Vi, "Wietnamski");
			dictionary8.Add(Locale.It, "Włoski");
			dictionary8.Add(Locale.De, "Niemiecki");
			dictionary8.Add(Locale.Uk, "Ukraiński");
			dictionary8.Add(Locale.Hu, "Węgierski");
			dictionary.Add(locale7, dictionary8);
			Locale locale8 = Locale.Pt;
			Dictionary<Locale, string> dictionary9 = new Dictionary<Locale, string>();
			dictionary9.Add(Locale.En, "Inglês");
			dictionary9.Add(Locale.ZhHans, "Chinês simplificado");
			dictionary9.Add(Locale.ZhHant, "Chinês tradicional");
			dictionary9.Add(Locale.Ja, "Japonês");
			dictionary9.Add(Locale.Ru, "Russo");
			dictionary9.Add(Locale.Es, "Espanhol");
			dictionary9.Add(Locale.Pl, "Polonês");
			dictionary9.Add(Locale.Pt, "Português");
			dictionary9.Add(Locale.Fr, "Francês");
			dictionary9.Add(Locale.Tr, "Turco");
			dictionary9.Add(Locale.Ko, "Coreano");
			dictionary9.Add(Locale.Vi, "Vietnamita");
			dictionary9.Add(Locale.It, "Italiano");
			dictionary9.Add(Locale.De, "Alemão");
			dictionary9.Add(Locale.Uk, "Ucraniano");
			dictionary9.Add(Locale.Hu, "Húngaro");
			dictionary.Add(locale8, dictionary9);
			Locale locale9 = Locale.Fr;
			Dictionary<Locale, string> dictionary10 = new Dictionary<Locale, string>();
			dictionary10.Add(Locale.En, "Anglais");
			dictionary10.Add(Locale.ZhHans, "Chinois simplifié");
			dictionary10.Add(Locale.ZhHant, "Chinois traditionnel");
			dictionary10.Add(Locale.Ja, "Japonais");
			dictionary10.Add(Locale.Ru, "Russe");
			dictionary10.Add(Locale.Es, "Espagnol");
			dictionary10.Add(Locale.Pl, "Polonais");
			dictionary10.Add(Locale.Pt, "Portugais");
			dictionary10.Add(Locale.Fr, "Français");
			dictionary10.Add(Locale.Tr, "Turc");
			dictionary10.Add(Locale.Ko, "Coréen");
			dictionary10.Add(Locale.Vi, "Vietnamien");
			dictionary10.Add(Locale.It, "Italien");
			dictionary10.Add(Locale.De, "Allemand");
			dictionary10.Add(Locale.Uk, "Ukrainien");
			dictionary10.Add(Locale.Hu, "Hongrois");
			dictionary.Add(locale9, dictionary10);
			Locale locale10 = Locale.Tr;
			Dictionary<Locale, string> dictionary11 = new Dictionary<Locale, string>();
			dictionary11.Add(Locale.En, "İngilizce");
			dictionary11.Add(Locale.ZhHans, "Basitleştirilmiş Çince");
			dictionary11.Add(Locale.ZhHant, "Geleneksel Çince");
			dictionary11.Add(Locale.Ja, "Japonca");
			dictionary11.Add(Locale.Ru, "Rusça");
			dictionary11.Add(Locale.Es, "İspanyolca");
			dictionary11.Add(Locale.Pl, "Lehçe");
			dictionary11.Add(Locale.Pt, "Portekizce");
			dictionary11.Add(Locale.Fr, "Fransızca");
			dictionary11.Add(Locale.Tr, "Türkçe");
			dictionary11.Add(Locale.Ko, "Korece");
			dictionary11.Add(Locale.Vi, "Vietnamca");
			dictionary11.Add(Locale.It, "İtalyanca");
			dictionary11.Add(Locale.De, "Almanca");
			dictionary11.Add(Locale.Uk, "Ukraynaca");
			dictionary11.Add(Locale.Hu, "Macarca");
			dictionary.Add(locale10, dictionary11);
			Locale locale11 = Locale.Ko;
			Dictionary<Locale, string> dictionary12 = new Dictionary<Locale, string>();
			dictionary12.Add(Locale.En, "영어");
			dictionary12.Add(Locale.ZhHans, "간체 중국어");
			dictionary12.Add(Locale.ZhHant, "번체 중국어");
			dictionary12.Add(Locale.Ja, "일본어");
			dictionary12.Add(Locale.Ru, "러시아어");
			dictionary12.Add(Locale.Es, "스페인어");
			dictionary12.Add(Locale.Pl, "폴란드어");
			dictionary12.Add(Locale.Pt, "포르투갈어");
			dictionary12.Add(Locale.Fr, "프랑스어");
			dictionary12.Add(Locale.Tr, "터키어");
			dictionary12.Add(Locale.Ko, "한국어");
			dictionary12.Add(Locale.Vi, "베트남어");
			dictionary12.Add(Locale.It, "이탈리아어");
			dictionary12.Add(Locale.De, "독일어");
			dictionary12.Add(Locale.Uk, "우크라이나어");
			dictionary12.Add(Locale.Hu, "헝가리어");
			dictionary.Add(locale11, dictionary12);
			Locale locale12 = Locale.Vi;
			Dictionary<Locale, string> dictionary13 = new Dictionary<Locale, string>();
			dictionary13.Add(Locale.En, "Tiếng Anh");
			dictionary13.Add(Locale.ZhHans, "Tiếng Trung Giản thể");
			dictionary13.Add(Locale.ZhHant, "Tiếng Trung Phồn thể");
			dictionary13.Add(Locale.Ja, "Tiếng Nhật");
			dictionary13.Add(Locale.Ru, "Tiếng Nga");
			dictionary13.Add(Locale.Es, "Tiếng Tây Ban Nha");
			dictionary13.Add(Locale.Pl, "Tiếng Ba Lan");
			dictionary13.Add(Locale.Pt, "Tiếng Bồ Đào Nha");
			dictionary13.Add(Locale.Fr, "Tiếng Pháp");
			dictionary13.Add(Locale.Tr, "Tiếng Thổ Nhĩ Kỳ");
			dictionary13.Add(Locale.Ko, "Tiếng Hàn");
			dictionary13.Add(Locale.Vi, "Tiếng Việt");
			dictionary13.Add(Locale.It, "Tiếng Ý");
			dictionary13.Add(Locale.De, "Tiếng Đức");
			dictionary13.Add(Locale.Uk, "Tiếng Ukraina");
			dictionary13.Add(Locale.Hu, "Tiếng Hungary");
			dictionary.Add(locale12, dictionary13);
			Locale locale13 = Locale.It;
			Dictionary<Locale, string> dictionary14 = new Dictionary<Locale, string>();
			dictionary14.Add(Locale.En, "Inglese");
			dictionary14.Add(Locale.ZhHans, "Cinese semplificato");
			dictionary14.Add(Locale.ZhHant, "Cinese tradizionale");
			dictionary14.Add(Locale.Ja, "Giapponese");
			dictionary14.Add(Locale.Ru, "Russo");
			dictionary14.Add(Locale.Es, "Spagnolo");
			dictionary14.Add(Locale.Pl, "Polacco");
			dictionary14.Add(Locale.Pt, "Portoghese");
			dictionary14.Add(Locale.Fr, "Francese");
			dictionary14.Add(Locale.Tr, "Turco");
			dictionary14.Add(Locale.Ko, "Coreano");
			dictionary14.Add(Locale.Vi, "Vietnamita");
			dictionary14.Add(Locale.It, "Italiano");
			dictionary14.Add(Locale.De, "Tedesco");
			dictionary14.Add(Locale.Uk, "Ucraino");
			dictionary14.Add(Locale.Hu, "Ungherese");
			dictionary.Add(locale13, dictionary14);
			Locale locale14 = Locale.De;
			Dictionary<Locale, string> dictionary15 = new Dictionary<Locale, string>();
			dictionary15.Add(Locale.En, "Englisch");
			dictionary15.Add(Locale.ZhHans, "Vereinfachtes Chinesisch");
			dictionary15.Add(Locale.ZhHant, "Traditionelles Chinesisch");
			dictionary15.Add(Locale.Ja, "Japanisch");
			dictionary15.Add(Locale.Ru, "Russisch");
			dictionary15.Add(Locale.Es, "Spanisch");
			dictionary15.Add(Locale.Pl, "Polnisch");
			dictionary15.Add(Locale.Pt, "Portugiesisch");
			dictionary15.Add(Locale.Fr, "Französisch");
			dictionary15.Add(Locale.Tr, "Türkisch");
			dictionary15.Add(Locale.Ko, "Koreanisch");
			dictionary15.Add(Locale.Vi, "Vietnamesisch");
			dictionary15.Add(Locale.It, "Italienisch");
			dictionary15.Add(Locale.De, "Deutsch");
			dictionary15.Add(Locale.Uk, "Ukrainisch");
			dictionary15.Add(Locale.Hu, "Ungarisch");
			dictionary.Add(locale14, dictionary15);
			Locale locale15 = Locale.Uk;
			Dictionary<Locale, string> dictionary16 = new Dictionary<Locale, string>();
			dictionary16.Add(Locale.En, "Англійська");
			dictionary16.Add(Locale.ZhHans, "Спрощена китайська");
			dictionary16.Add(Locale.ZhHant, "Традиційна китайська");
			dictionary16.Add(Locale.Ja, "Японська");
			dictionary16.Add(Locale.Ru, "Російська");
			dictionary16.Add(Locale.Es, "Іспанська");
			dictionary16.Add(Locale.Pl, "Польська");
			dictionary16.Add(Locale.Pt, "Португальська");
			dictionary16.Add(Locale.Fr, "Французька");
			dictionary16.Add(Locale.Tr, "Турецька");
			dictionary16.Add(Locale.Ko, "Корейська");
			dictionary16.Add(Locale.Vi, "В’єтнамська");
			dictionary16.Add(Locale.It, "Італійська");
			dictionary16.Add(Locale.De, "Німецька");
			dictionary16.Add(Locale.Uk, "Українська");
			dictionary16.Add(Locale.Hu, "Угорська");
			dictionary.Add(locale15, dictionary16);
			Locale locale16 = Locale.Hu;
			Dictionary<Locale, string> dictionary17 = new Dictionary<Locale, string>();
			dictionary17.Add(Locale.En, "Angol");
			dictionary17.Add(Locale.ZhHans, "Egyszerűsített kínai");
			dictionary17.Add(Locale.ZhHant, "Hagyományos kínai");
			dictionary17.Add(Locale.Ja, "Japán");
			dictionary17.Add(Locale.Ru, "Orosz");
			dictionary17.Add(Locale.Es, "Spanyol");
			dictionary17.Add(Locale.Pl, "Lengyel");
			dictionary17.Add(Locale.Pt, "Portugál");
			dictionary17.Add(Locale.Fr, "Francia");
			dictionary17.Add(Locale.Tr, "Török");
			dictionary17.Add(Locale.Ko, "Koreai");
			dictionary17.Add(Locale.Vi, "Vietnámi");
			dictionary17.Add(Locale.It, "Olasz");
			dictionary17.Add(Locale.De, "Német");
			dictionary17.Add(Locale.Uk, "Ukrán");
			dictionary17.Add(Locale.Hu, "Magyar");
			dictionary.Add(locale16, dictionary17);
			LocaleExtensions.LocaleNamesTable = dictionary;
		}
		private static readonly Dictionary<Locale, Dictionary<Locale, string>> LocaleNamesTable;
	}
}
