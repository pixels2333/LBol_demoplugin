using System;
namespace LBoL.Core.SaveData
{
	[Serializable]
	public sealed class CharacterStatsSaveData
	{
		public string CharacterId;
		public GameDifficulty? HighestPerfectSuccessDifficulty;
		public GameDifficulty? HighestSuccessDifficulty;
		public int HighestBluePoint;
		public int TotalBluePoint;
		public int TotalPlaySeconds;
		public int PerfectSuccessCount;
		public int SuccessCount;
		public int FailCount;
		public int PuzzleCount;
	}
}
