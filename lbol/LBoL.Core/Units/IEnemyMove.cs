using System;
using System.Collections.Generic;
using LBoL.Core.Battle;

namespace LBoL.Core.Units
{
	// Token: 0x02000080 RID: 128
	public interface IEnemyMove
	{
		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x0600061D RID: 1565
		Intention Intention { get; }

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x0600061E RID: 1566
		IEnumerable<BattleAction> Actions { get; }

		// Token: 0x0600061F RID: 1567 RVA: 0x0001354B File Offset: 0x0001174B
		IEnemyMove AsHiddenIntention(bool hidden = true)
		{
			this.Intention.AsHidden(hidden);
			return this;
		}
	}
}
