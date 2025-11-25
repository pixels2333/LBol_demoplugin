using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class GainManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public GainManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaGaining);
		}
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.GainMana(base.Args.Value);
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaGained);
		}
	}
}
