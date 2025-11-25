using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class GainPowerAction : SimpleEventBattleAction<PowerEventArgs>
	{
		public GainPowerAction(int power)
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
			int num = base.Battle.GameRun.InternalGainPower(base.Args.Power);
			if (num != base.Args.Power)
			{
				base.Args.Power = num;
				base.Args.IsModified = true;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.Player.PowerGained);
		}
	}
}
