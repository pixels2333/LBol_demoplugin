using System;

namespace LBoL.Core
{
	// Token: 0x0200004A RID: 74
	public interface IGameRunAchievementHandler
	{
		// Token: 0x0600034C RID: 844
		void IncreaseStats(ProfileStatsKey statsKey);

		// Token: 0x0600034D RID: 845
		void UnlockAchievement(AchievementKey achievementKey);
	}
}
