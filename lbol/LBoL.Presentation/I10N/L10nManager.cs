using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using LBoL.Core;
using LBoL.Presentation.UI;
using UnityEngine;
namespace LBoL.Presentation.I10N
{
	public class L10nManager : Singleton<L10nManager>
	{
		public static L10nInfo Info { get; private set; }
		private static async UniTask ReloadAsync()
		{
			L10nManager.Info = L10nManager.InfoTable[Localization.CurrentLocale];
			await Localization.ReloadCommonAsync();
			await UnitNameTable.ReloadLocalizationAsync();
			await EntityNameTable.ReloadExtraLocalizationAsync();
			await Library.ReloadLocalizationsAsync();
			await Keywords.ReloadAsync();
			await PuzzleFlags.ReloadAsync();
			await Achievements.ReloadLocalizationAsync();
			foreach (UiBase uiBase in UiManager.EnumerateAll())
			{
				await uiBase.CustomLocalizationAsync();
			}
			IEnumerator<UiBase> enumerator = null;
			Action localeChanged = L10nManager.LocaleChanged;
			if (localeChanged != null)
			{
				localeChanged.Invoke();
			}
			CrossPlatformHelper.SetWindowTitle("Game.Title".Localize(true));
		}
		public static UniTask InitializeAsync()
		{
			return L10nManager.ReloadAsync();
		}
		public static async UniTask SetLocaleAsync(Locale locale)
		{
			if (Localization.CurrentLocale != locale)
			{
				try
				{
					Localization.SetCurrentLocale(locale);
					GameMaster.SaveSys();
				}
				catch (Exception ex)
				{
					Debug.LogError("[Localization] set locale to " + locale.ToTag() + " failed: " + ex.Message);
				}
				await L10nManager.ReloadAsync();
			}
		}
		public static async UniTask ReloadLocalization()
		{
			await L10nManager.ReloadAsync();
		}
		public static event Action LocaleChanged;
		// Note: this type is marked as 'beforefieldinit'.
		static L10nManager()
		{
			Dictionary<Locale, L10nInfo> dictionary = new Dictionary<Locale, L10nInfo>();
			dictionary[Locale.En] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferItalicInFlavor = true,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("en")
			};
			dictionary[Locale.ZhHans] = new L10nInfo
			{
				VnTextRevealSpeed = 20f,
				VnTextRevealAhead = 4f,
				Culture = new CultureInfo("zh-Hans")
			};
			dictionary[Locale.ZhHant] = new L10nInfo
			{
				VnTextRevealSpeed = 20f,
				VnTextRevealAhead = 4f,
				Culture = new CultureInfo("zh-Hant")
			};
			dictionary[Locale.Ja] = new L10nInfo
			{
				VnTextRevealSpeed = 20f,
				VnTextRevealAhead = 4f,
				Culture = new CultureInfo("ja")
			};
			dictionary[Locale.Ru] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("ru")
			};
			dictionary[Locale.Es] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferItalicInFlavor = true,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("es")
			};
			dictionary[Locale.Pl] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("pl")
			};
			dictionary[Locale.Pt] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("pt")
			};
			dictionary[Locale.Fr] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("fr")
			};
			dictionary[Locale.Tr] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("tr")
			};
			dictionary[Locale.Ko] = new L10nInfo
			{
				VnTextRevealSpeed = 20f,
				VnTextRevealAhead = 4f,
				PreferShortName = true,
				Culture = new CultureInfo("ko")
			};
			dictionary[Locale.Vi] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("vi")
			};
			dictionary[Locale.It] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("it")
			};
			dictionary[Locale.De] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("de")
			};
			dictionary[Locale.Uk] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("uk")
			};
			dictionary[Locale.Hu] = new L10nInfo
			{
				VnTextRevealSpeed = 50f,
				VnTextRevealAhead = 8f,
				PreferShortName = true,
				PreferWideTooltip = true,
				HideExhibitRarity = true,
				Culture = new CultureInfo("hu")
			};
			L10nManager.InfoTable = dictionary;
		}
		private static readonly Dictionary<Locale, L10nInfo> InfoTable;
	}
}
