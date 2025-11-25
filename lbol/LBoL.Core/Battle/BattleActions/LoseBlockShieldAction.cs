using System;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class LoseBlockShieldAction : SimpleEventBattleAction<BlockShieldEventArgs>
	{
		public LoseBlockShieldAction(Unit target, int block, int shield, bool forced = false)
		{
			base.Args = new BlockShieldEventArgs
			{
				Target = target,
				Block = (float)block,
				Shield = (float)shield,
				CanCancel = !forced
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Args.Target.BlockShieldLosing);
		}
		protected override void MainPhase()
		{
			ValueTuple<int, int> valueTuple = base.Battle.LoseBlockShield(base.Args.Target, base.Args.Block, base.Args.Shield);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			if (!base.Args.Block.Approximately((float)item) || !base.Args.Shield.Approximately((float)item2))
			{
				BlockShieldEventArgs args = base.Args;
				BlockShieldEventArgs args2 = base.Args;
				float num = (float)item;
				float num2 = (float)item2;
				args.Block = num;
				args2.Shield = num2;
				base.Args.IsModified = true;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Args.Target.BlockShieldLost);
		}
	}
}
