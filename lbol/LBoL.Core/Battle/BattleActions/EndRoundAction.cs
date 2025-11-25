using System;
namespace LBoL.Core.Battle.BattleActions
{
	public class EndRoundAction : SimpleEventBattleAction<GameEventArgs>
	{
		internal EndRoundAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.RoundEnding);
		}
		protected override void MainPhase()
		{
			base.Battle.EndRound();
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.RoundEnded);
		}
	}
}
