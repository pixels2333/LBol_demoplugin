using System;
using System.Globalization;

namespace LBoL.Core.PlatformHandlers
{
	// Token: 0x020000F6 RID: 246
	public sealed class StandalonePlatformHandler : PlatformHandler
	{
		// Token: 0x06000974 RID: 2420 RVA: 0x0001B8E1 File Offset: 0x00019AE1
		public override bool Init()
		{
			return true;
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0001B8E4 File Offset: 0x00019AE4
		public override void Update()
		{
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0001B8E6 File Offset: 0x00019AE6
		public override void Shutdown()
		{
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x0001B8E8 File Offset: 0x00019AE8
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

		// Token: 0x06000978 RID: 2424 RVA: 0x0001B931 File Offset: 0x00019B31
		public override string GetSaveDataFolder()
		{
			return "SaveData";
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x0001B938 File Offset: 0x00019B38
		public override void SetMainMenuInfo(MainMenuStatus status)
		{
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x0001B93A File Offset: 0x00019B3A
		public override void SetGameRunInfo(GameRunController gameRun)
		{
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x0001B93C File Offset: 0x00019B3C
		public override void SetAchievement(string key)
		{
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x0001B93E File Offset: 0x00019B3E
		public override void ClearAchievement(string key)
		{
		}
	}
}
