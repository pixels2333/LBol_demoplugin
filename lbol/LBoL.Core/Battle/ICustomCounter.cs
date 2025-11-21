using System;

namespace LBoL.Core.Battle
{
	// Token: 0x02000144 RID: 324
	public interface ICustomCounter
	{
		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x06000D15 RID: 3349
		CustomCounterResetTiming AutoResetTiming { get; }

		// Token: 0x06000D16 RID: 3350
		void Increase(BattleController battle);

		// Token: 0x06000D17 RID: 3351
		void Reset(BattleController battle);
	}
}
