using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class LoseTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public LoseTurnManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLosing);
		}
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.LoseTurnMana(base.Args.Value);
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLost);
		}
	}
}
