using System;

namespace LBoL.Core
{
	// Token: 0x02000061 RID: 97
	public abstract class PlatformHandler
	{
		// Token: 0x06000440 RID: 1088
		public abstract bool Init();

		// Token: 0x06000441 RID: 1089
		public abstract void Update();

		// Token: 0x06000442 RID: 1090
		public abstract void Shutdown();

		// Token: 0x06000443 RID: 1091
		public abstract Locale GetDefaultLocale();

		// Token: 0x06000444 RID: 1092
		public abstract string GetSaveDataFolder();

		// Token: 0x06000445 RID: 1093
		public abstract void SetMainMenuInfo(MainMenuStatus status);

		// Token: 0x06000446 RID: 1094
		public abstract void SetGameRunInfo(GameRunController gameRun);

		// Token: 0x06000447 RID: 1095
		public abstract void SetAchievement(string key);

		// Token: 0x06000448 RID: 1096
		public abstract void ClearAchievement(string key);
	}
}
