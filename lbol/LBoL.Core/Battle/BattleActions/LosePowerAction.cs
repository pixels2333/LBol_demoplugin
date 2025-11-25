using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class LosePowerAction : SimpleEventBattleAction<PowerEventArgs>
	{
		public LosePowerAction(int power)
		{
			base.Args = new PowerEventArgs
			{
				Power = power
			};
		}
		protected override void PreEventPhase()
		{
		}
		protected override void MainPhase()
		{
			int num = base.Battle.GameRun.InternalLosePower(base.Args.Power);
			if (num != base.Args.Power)
			{
				base.Args.Power = num;
				base.Args.IsModified = true;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.Player.PowerLost);
		}
	}
}
