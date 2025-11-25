using System;
using System.Globalization;
namespace LBoL.Core.PlatformHandlers
{
	public sealed class StandalonePlatformHandler : PlatformHandler
	{
		public override bool Init()
		{
			return true;
		}
		public override void Update()
		{
		}
		public override void Shutdown()
		{
		}
		public override Locale GetDefaultLocale()
		{
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			while (!cultureInfo.Equals(CultureInfo.InvariantCulture))
			{
				Locale? locale = cultureInfo.Name.TryParseLocaleTag();
				if (locale != null)
				{
					return locale.GetValueOrDefault();
				}
				cultureInfo = cultureInfo.Parent;
			}
			return Locale.ZhHans;
		}
		public override string GetSaveDataFolder()
		{
			return "SaveData";
		}
		public override void SetMainMenuInfo(MainMenuStatus status)
		{
		}
		public override void SetGameRunInfo(GameRunController gameRun)
		{
		}
		public override void SetAchievement(string key)
		{
		}
		public override void ClearAchievement(string key)
		{
		}
	}
}
