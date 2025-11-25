using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class StartAllEnemyTurnAction : SimpleEventBattleAction<GameEventArgs>
	{
		internal StartAllEnemyTurnAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnStarting);
		}
		protected override void MainPhase()
		{
			base.Battle.StartAllEnemyTurn();
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.AllEnemyTurnStarted);
		}
	}
}
