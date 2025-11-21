using System;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200017A RID: 378
	public class EndShootAction : SimpleAction
	{
		// Token: 0x06000E64 RID: 3684 RVA: 0x0002740B File Offset: 0x0002560B
		public EndShootAction(Unit source)
		{
			this.SourceUnit = source;
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x06000E65 RID: 3685 RVA: 0x0002741A File Offset: 0x0002561A
		public Unit SourceUnit { get; }
	}
}
