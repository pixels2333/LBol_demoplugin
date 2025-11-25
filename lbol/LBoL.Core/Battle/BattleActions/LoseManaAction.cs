using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class LoseManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public LoseManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaLosing);
		}
		protected override void MainPhase()
		{
			ManaGroup manaGroup = base.Battle.LoseMana(base.Args.Value);
			if (manaGroup != base.Args.Value)
			{
				base.Args.Value = manaGroup;
				base.Args.IsModified = true;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaLost);
		}
	}
}
