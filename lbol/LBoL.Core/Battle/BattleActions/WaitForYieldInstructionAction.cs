using System;
using System.Collections.Generic;
using UnityEngine;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class WaitForYieldInstructionAction : SimpleAction
	{
		public YieldInstruction Instruction { get; }
		public WaitForYieldInstructionAction(YieldInstruction yieldInstruction)
		{
			this.Instruction = yieldInstruction;
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Wait", delegate
			{
			}, true);
			yield break;
		}
	}
}
