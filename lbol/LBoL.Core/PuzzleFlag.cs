using System;
namespace LBoL.Core
{
	[Flags]
	public enum PuzzleFlag
	{
		None = 0,
		HalfDrug = 1,
		LowMaxHp = 2,
		StartMisfortune = 4,
		LowStageRegen = 8,
		LowUpgradeRate = 16,
		PayForUpgrade = 32,
		NightMana = 64,
		PoorBoss = 128,
		LowPower = 256,
		ShopPriceRise = 512
	}
}
