using System;

namespace LBoL.Core.Battle
{
	// Token: 0x02000143 RID: 323
	[Flags]
	public enum CustomCounterResetTiming
	{
		// Token: 0x0400061B RID: 1563
		None = 0,
		// Token: 0x0400061C RID: 1564
		PlayerTurnStart = 1,
		// Token: 0x0400061D RID: 1565
		PlayerTurnEnd = 2,
		// Token: 0x0400061E RID: 1566
		PlayerActionStart = 4,
		// Token: 0x0400061F RID: 1567
		PlayerActionEnd = 8
	}
}
