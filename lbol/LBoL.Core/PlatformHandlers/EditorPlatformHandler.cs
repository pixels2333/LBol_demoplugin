using System;
namespace LBoL.Core.PlatformHandlers
{
	public sealed class EditorPlatformHandler : PlatformHandler
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
