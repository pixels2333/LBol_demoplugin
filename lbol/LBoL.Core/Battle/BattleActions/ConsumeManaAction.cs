using System;
using LBoL.Base;
namespace LBoL.Core.Battle.BattleActions
{
	public class ConsumeManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		public ConsumeManaAction(ManaGroup group)
		{
			base.Args = new ManaEventArgs
			{
				Value = group
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaConsuming);
		}
		protected override void MainPhase()
		{
			base.Battle.ConsumeMana(base.Args.Value);
			base.Args.CanCancel = false;
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaConsumed);
		}
	}
}
