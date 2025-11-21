using System;
using System.Collections.Generic;
using LBoL.Core.Battle;

namespace LBoL.Core.Units
{
	// Token: 0x02000088 RID: 136
	public class SimpleEnemyMove : IEnemyMove
	{
		// Token: 0x0600068D RID: 1677 RVA: 0x00014134 File Offset: 0x00012334
		public SimpleEnemyMove(Intention intention, IEnumerable<BattleAction> actions)
		{
			this.Intention = intention;
			this.Actions = actions;
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0001415C File Offset: 0x0001235C
		public SimpleEnemyMove(Intention intention, BattleAction action)
		{
			IEnumerable<BattleAction> enumerable = new BattleAction[] { action };
			this.Intention = intention;
			this.Actions = enumerable;
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x0001418A File Offset: 0x0001238A
		public SimpleEnemyMove(Intention intention)
		{
			this.Intention = intention;
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000690 RID: 1680 RVA: 0x00014199 File Offset: 0x00012399
		public Intention Intention { get; }

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06000691 RID: 1681 RVA: 0x000141A1 File Offset: 0x000123A1
		public IEnumerable<BattleAction> Actions { get; }
	}
}
