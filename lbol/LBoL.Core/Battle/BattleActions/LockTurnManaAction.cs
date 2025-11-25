using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class LockTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public LockTurnManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLocking);
		}
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.LockTurnMana(base.Args.Value);
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLocked);
		}
	}
}
