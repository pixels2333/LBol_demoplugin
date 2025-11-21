using System;

namespace LBoL.Core
{
	// Token: 0x02000063 RID: 99
	[Flags]
	public enum PuzzleFlag
	{
		// Token: 0x04000249 RID: 585
		None = 0,
		// Token: 0x0400024A RID: 586
		HalfDrug = 1,
		// Token: 0x0400024B RID: 587
		LowMaxHp = 2,
		// Token: 0x0400024C RID: 588
		StartMisfortune = 4,
		// Token: 0x0400024D RID: 589
		LowStageRegen = 8,
		// Token: 0x0400024E RID: 590
		LowUpgradeRate = 16,
		// Token: 0x0400024F RID: 591
		PayForUpgrade = 32,
		// Token: 0x04000250 RID: 592
		NightMana = 64,
		// Token: 0x04000251 RID: 593
		PoorBoss = 128,
		// Token: 0x04000252 RID: 594
		LowPower = 256,
		// Token: 0x04000253 RID: 595
		ShopPriceRise = 512
	}
}
