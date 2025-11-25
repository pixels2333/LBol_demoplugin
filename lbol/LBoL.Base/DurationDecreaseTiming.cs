using System;
using JetBrains.Annotations;
namespace LBoL.Base
{
	[Flags]
	public enum DurationDecreaseTiming
	{
		[UsedImplicitly]
		Custom = 0,
		NormalTurnStart = 1,
		ExtraTurnStart = 2,
		[UsedImplicitly]
		TurnStart = 3,
		EndTurnForRound = 16,
		EndTurnForExtra = 32,
		[UsedImplicitly]
		TurnEnd = 48,
		RoundStart = 256,
		RoundEnd = 512
	}
}
