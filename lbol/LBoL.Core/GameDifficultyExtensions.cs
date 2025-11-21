using System;

namespace LBoL.Core
{
	// Token: 0x02000013 RID: 19
	public static class GameDifficultyExtensions
	{
		// Token: 0x060000A8 RID: 168 RVA: 0x00003525 File Offset: 0x00001725
		public static bool IsHigherThan(this GameDifficulty source, GameDifficulty compareTo)
		{
			return source > compareTo;
		}
	}
}
