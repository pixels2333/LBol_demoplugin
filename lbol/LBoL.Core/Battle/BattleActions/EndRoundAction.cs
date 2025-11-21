using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000179 RID: 377
	public class EndRoundAction : SimpleEventBattleAction<GameEventArgs>
	{
		// Token: 0x06000E60 RID: 3680 RVA: 0x000273BE File Offset: 0x000255BE
		internal EndRoundAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x000273D8 File Offset: 0x000255D8
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.RoundEnding);
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x000273EB File Offset: 0x000255EB
		protected override void MainPhase()
		{
			base.Battle.EndRound();
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x000273F8 File Offset: 0x000255F8
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.RoundEnded);
		}
	}
}
