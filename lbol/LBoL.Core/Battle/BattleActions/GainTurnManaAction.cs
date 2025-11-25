using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class GainTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public GainTurnManaAction(ManaGroup value)
		{
			if (value.Any > 0)
			{
				throw new InvalidOperationException("Cant gain extra turn mana with color: Any.");
			}
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaGaining);
		}
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.GainTurnMana(base.Args.Value);
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaGained);
		}
	}
}
