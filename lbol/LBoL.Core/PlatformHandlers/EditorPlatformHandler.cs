using System;

namespace LBoL.Core.PlatformHandlers
{
	// Token: 0x020000F5 RID: 245
	public sealed class EditorPlatformHandler : PlatformHandler
	{
		// Token: 0x0600096A RID: 2410 RVA: 0x0001B8C0 File Offset: 0x00019AC0
		public override bool Init()
		{
			return true;
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x0001B8C3 File Offset: 0x00019AC3
		public override void Update()
		{
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x0001B8C5 File Offset: 0x00019AC5
		public override void Shutdown()
		{
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x0001B8C7 File Offset: 0x00019AC7
		public override Locale GetDefaultLocale()
		{
			return Locale.ZhHans;
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x0001B8CA File Offset: 0x00019ACA
		public override string GetSaveDataFolder()
		{
			return "SaveData";
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x0001B8D1 File Offset: 0x00019AD1
		public override void SetMainMenuInfo(MainMenuStatus status)
		{
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x0001B8D3 File Offset: 0x00019AD3
		public override void SetGameRunInfo(GameRunController gameRun)
		{
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x0001B8D5 File Offset: 0x00019AD5
		public override void SetAchievement(string key)
		{
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x0001B8D7 File Offset: 0x00019AD7
		public override void ClearAchievement(string key)
		{
		}
	}
}
