using System;
namespace LBoL.Core.Battle
{
	[Flags]
	public enum CustomCounterResetTiming
	{
		None = 0,
		PlayerTurnStart = 1,
		PlayerTurnEnd = 2,
		PlayerActionStart = 4,
		PlayerActionEnd = 8
	}
}
