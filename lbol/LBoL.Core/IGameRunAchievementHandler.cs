using System;
namespace LBoL.Core
{
	public interface IGameRunAchievementHandler
	{
		void IncreaseStats(ProfileStatsKey statsKey);
		void UnlockAchievement(AchievementKey achievementKey);
	}
}
