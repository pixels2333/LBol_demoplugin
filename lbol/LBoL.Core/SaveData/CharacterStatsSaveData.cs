using System;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D1 RID: 209
	[Serializable]
	public sealed class CharacterStatsSaveData
	{
		// Token: 0x040003D8 RID: 984
		public string CharacterId;

		// Token: 0x040003D9 RID: 985
		public GameDifficulty? HighestPerfectSuccessDifficulty;

		// Token: 0x040003DA RID: 986
		public GameDifficulty? HighestSuccessDifficulty;

		// Token: 0x040003DB RID: 987
		public int HighestBluePoint;

		// Token: 0x040003DC RID: 988
		public int TotalBluePoint;

		// Token: 0x040003DD RID: 989
		public int TotalPlaySeconds;

		// Token: 0x040003DE RID: 990
		public int PerfectSuccessCount;

		// Token: 0x040003DF RID: 991
		public int SuccessCount;

		// Token: 0x040003E0 RID: 992
		public int FailCount;

		// Token: 0x040003E1 RID: 993
		public int PuzzleCount;
	}
}
