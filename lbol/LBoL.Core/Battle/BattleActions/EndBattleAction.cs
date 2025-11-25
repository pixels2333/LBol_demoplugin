using System;
namespace LBoL.Core.Battle.BattleActions
{
	public class EndBattleAction : SimpleEventBattleAction<GameEventArgs>
	{
		internal EndBattleAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.BattleEnding);
		}
		protected override void MainPhase()
		{
			base.Battle.EndBattle();
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.BattleEnded);
		}
	}
}
