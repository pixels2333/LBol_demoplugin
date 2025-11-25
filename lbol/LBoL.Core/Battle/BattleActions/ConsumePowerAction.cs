using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class ConsumePowerAction : EventBattleAction<PowerEventArgs>
	{
		public ConsumePowerAction(int power)
		{
			base.Args = new PowerEventArgs
			{
				Power = power,
				CanCancel = false
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.GameRun.InternalConsumePower(base.Args.Power);
			}, true);
			yield break;
		}
	}
}
