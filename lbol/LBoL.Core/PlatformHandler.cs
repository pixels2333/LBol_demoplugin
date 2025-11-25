using System;
namespace LBoL.Core
{
	public abstract class PlatformHandler
	{
		public abstract bool Init();
		public abstract void Update();
		public abstract void Shutdown();
		public abstract Locale GetDefaultLocale();
		public abstract string GetSaveDataFolder();
		public abstract void SetMainMenuInfo(MainMenuStatus status);
		public abstract void SetGameRunInfo(GameRunController gameRun);
		public abstract void SetAchievement(string key);
		public abstract void ClearAchievement(string key);
	}
}
