using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class UnlockTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public UnlockTurnManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaUnlocking);
		}
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.UnlockTurnMana(base.Args.Value);
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaUnlocked);
		}
	}
}
