using System;
namespace LBoL.Core
{
	public static class GameDifficultyExtensions
	{
		public static bool IsHigherThan(this GameDifficulty source, GameDifficulty compareTo)
		{
			return source > compareTo;
		}
	}
}
