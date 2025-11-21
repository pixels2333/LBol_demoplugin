using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000191 RID: 401
	public sealed class LosePowerAction : SimpleEventBattleAction<PowerEventArgs>
	{
		// Token: 0x06000ECD RID: 3789 RVA: 0x00028111 File Offset: 0x00026311
		public LosePowerAction(int power)
		{
			base.Args = new PowerEventArgs
			{
				Power = power
			};
		}

		// Token: 0x06000ECE RID: 3790 RVA: 0x0002812B File Offset: 0x0002632B
		protected override void PreEventPhase()
		{
		}

		// Token: 0x06000ECF RID: 3791 RVA: 0x00028130 File Offset: 0x00026330
		protected override void MainPhase()
		{
			int num = base.Battle.GameRun.InternalLosePower(base.Args.Power);
			if (num != base.Args.Power)
			{
				base.Args.Power = num;
				base.Args.IsModified = true;
			}
		}

		// Token: 0x06000ED0 RID: 3792 RVA: 0x0002817F File Offset: 0x0002637F
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.Player.PowerLost);
		}
	}
}
