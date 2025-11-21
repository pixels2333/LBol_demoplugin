using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A4 RID: 420
	public sealed class StartAllEnemyTurnAction : SimpleEventBattleAction<GameEventArgs>
	{
		// Token: 0x06000F29 RID: 3881 RVA: 0x00028DF2 File Offset: 0x00026FF2
		internal StartAllEnemyTurnAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x00028E0C File Offset: 0x0002700C
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnStarting);
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x00028E1F File Offset: 0x0002701F
		protected override void MainPhase()
		{
			base.Battle.StartAllEnemyTurn();
		}

		// Token: 0x06000F2C RID: 3884 RVA: 0x00028E2C File Offset: 0x0002702C
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnStarted);
		}
	}
}
