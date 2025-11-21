using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000176 RID: 374
	public class EndBattleAction : SimpleEventBattleAction<GameEventArgs>
	{
		// Token: 0x06000E43 RID: 3651 RVA: 0x00027219 File Offset: 0x00025419
		internal EndBattleAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x00027233 File Offset: 0x00025433
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.BattleEnding);
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x00027246 File Offset: 0x00025446
		protected override void MainPhase()
		{
			base.Battle.EndBattle();
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x00027253 File Offset: 0x00025453
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.BattleEnded);
		}
	}
}
