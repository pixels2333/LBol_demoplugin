using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000175 RID: 373
	public class EndAllEnemyTurnAction : SimpleEventBattleAction<GameEventArgs>
	{
		// Token: 0x06000E3F RID: 3647 RVA: 0x000271CC File Offset: 0x000253CC
		internal EndAllEnemyTurnAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000E40 RID: 3648 RVA: 0x000271E6 File Offset: 0x000253E6
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnEnding);
		}

		// Token: 0x06000E41 RID: 3649 RVA: 0x000271F9 File Offset: 0x000253F9
		protected override void MainPhase()
		{
			base.Battle.EndAllEnemyTurn();
		}

		// Token: 0x06000E42 RID: 3650 RVA: 0x00027206 File Offset: 0x00025406
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnEnded);
		}
	}
}
