using System;
namespace LBoL.Core.Battle.BattleActions
{
	public class StartRoundAction : SimpleEventBattleAction<GameEventArgs>
	{
		internal StartRoundAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.RoundStarting);
		}
		protected override void MainPhase()
		{
			base.Battle.StartRound();
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.RoundStarted);
		}
	}
}
