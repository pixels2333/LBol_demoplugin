using System;
using System.Collections.Generic;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B6 RID: 438
	public sealed class WaitForYieldInstructionAction : SimpleAction
	{
		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x06000F8E RID: 3982 RVA: 0x0002999F File Offset: 0x00027B9F
		public YieldInstruction Instruction { get; }

		// Token: 0x06000F8F RID: 3983 RVA: 0x000299A7 File Offset: 0x00027BA7
		public WaitForYieldInstructionAction(YieldInstruction yieldInstruction)
		{
			this.Instruction = yieldInstruction;
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x000299B6 File Offset: 0x00027BB6
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Wait", delegate
			{
			}, true);
			yield break;
		}
	}
}
