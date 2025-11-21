using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200018D RID: 397
	public sealed class LoseAllExhibitsAction : SimpleEventBattleAction<ExhibitsEventArgs>
	{
		// Token: 0x06000EC0 RID: 3776 RVA: 0x00027F16 File Offset: 0x00026116
		public LoseAllExhibitsAction()
		{
			base.Args = new ExhibitsEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x00027F30 File Offset: 0x00026130
		protected override void MainPhase()
		{
			base.Args.Exhibits = base.Battle.LoseAllExhibits();
		}
	}
}
