using System;
using LBoL.Base.Extensions;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200018E RID: 398
	public class LoseBlockShieldAction : SimpleEventBattleAction<BlockShieldEventArgs>
	{
		// Token: 0x06000EC2 RID: 3778 RVA: 0x00027F48 File Offset: 0x00026148
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

		// Token: 0x06000EC3 RID: 3779 RVA: 0x00027F7D File Offset: 0x0002617D
		protected override void PreEventPhase()
		{
			base.Trigger(base.Args.Target.BlockShieldLosing);
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x00027F98 File Offset: 0x00026198
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

		// Token: 0x06000EC5 RID: 3781 RVA: 0x0002803A File Offset: 0x0002623A
		protected override void PostEventPhase()
		{
			base.Trigger(base.Args.Target.BlockShieldLost);
		}
	}
}
