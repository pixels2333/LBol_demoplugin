using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A8 RID: 424
	public class StartRoundAction : SimpleEventBattleAction<GameEventArgs>
	{
		// Token: 0x06000F45 RID: 3909 RVA: 0x000291B5 File Offset: 0x000273B5
		internal StartRoundAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x000291CF File Offset: 0x000273CF
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.RoundStarting);
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x000291E2 File Offset: 0x000273E2
		protected override void MainPhase()
		{
			base.Battle.StartRound();
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x000291EF File Offset: 0x000273EF
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.RoundStarted);
		}
	}
}
