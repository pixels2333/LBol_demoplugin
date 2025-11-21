using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000168 RID: 360
	public sealed class ConsumePowerAction : EventBattleAction<PowerEventArgs>
	{
		// Token: 0x06000DEF RID: 3567 RVA: 0x0002652F File Offset: 0x0002472F
		public ConsumePowerAction(int power)
		{
			base.Args = new PowerEventArgs
			{
				Power = power,
				CanCancel = false
			};
		}

		// Token: 0x06000DF0 RID: 3568 RVA: 0x00026550 File Offset: 0x00024750
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.GameRun.InternalConsumePower(base.Args.Power);
			}, true);
			yield break;
		}
	}
}
