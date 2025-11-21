using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000186 RID: 390
	public sealed class GainPowerAction : SimpleEventBattleAction<PowerEventArgs>
	{
		// Token: 0x06000EA1 RID: 3745 RVA: 0x00027C25 File Offset: 0x00025E25
		public GainPowerAction(int power)
		{
			base.Args = new PowerEventArgs
			{
				Power = power
			};
		}

		// Token: 0x06000EA2 RID: 3746 RVA: 0x00027C3F File Offset: 0x00025E3F
		protected override void PreEventPhase()
		{
		}

		// Token: 0x06000EA3 RID: 3747 RVA: 0x00027C44 File Offset: 0x00025E44
		protected override void MainPhase()
		{
			int num = base.Battle.GameRun.InternalGainPower(base.Args.Power);
			if (num != base.Args.Power)
			{
				base.Args.Power = num;
				base.Args.IsModified = true;
			}
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x00027C93 File Offset: 0x00025E93
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.Player.PowerGained);
		}
	}
}
