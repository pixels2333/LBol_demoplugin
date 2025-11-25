using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class LoseAllExhibitsAction : SimpleEventBattleAction<ExhibitsEventArgs>
	{
		public LoseAllExhibitsAction()
		{
			base.Args = new ExhibitsEventArgs
			{
				CanCancel = false
			};
		}
		protected override void MainPhase()
		{
			base.Args.Exhibits = base.Battle.LoseAllExhibits();
		}
	}
}
