using System;
namespace LBoL.Core.Battle.BattleActions
{
	public class EndAllEnemyTurnAction : SimpleEventBattleAction<GameEventArgs>
	{
		internal EndAllEnemyTurnAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnEnding);
		}
		protected override void MainPhase()
		{
			base.Battle.EndAllEnemyTurn();
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnEnded);
		}
	}
}
