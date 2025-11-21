using System;
using JetBrains.Annotations;

namespace LBoL.Base
{
	// Token: 0x02000009 RID: 9
	[Flags]
	public enum DurationDecreaseTiming
	{
		// Token: 0x04000019 RID: 25
		[UsedImplicitly]
		Custom = 0,
		// Token: 0x0400001A RID: 26
		NormalTurnStart = 1,
		// Token: 0x0400001B RID: 27
		ExtraTurnStart = 2,
		// Token: 0x0400001C RID: 28
		[UsedImplicitly]
		TurnStart = 3,
		// Token: 0x0400001D RID: 29
		EndTurnForRound = 16,
		// Token: 0x0400001E RID: 30
		EndTurnForExtra = 32,
		// Token: 0x0400001F RID: 31
		[UsedImplicitly]
		TurnEnd = 48,
		// Token: 0x04000020 RID: 32
		RoundStart = 256,
		// Token: 0x04000021 RID: 33
		RoundEnd = 512
	}
}
