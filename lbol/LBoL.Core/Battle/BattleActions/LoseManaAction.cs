using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200018F RID: 399
	public class LoseManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000EC6 RID: 3782 RVA: 0x00028052 File Offset: 0x00026252
		public LoseManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x0002806C File Offset: 0x0002626C
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaLosing);
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x00028080 File Offset: 0x00026280
		protected override void MainPhase()
		{
			ManaGroup manaGroup = base.Battle.LoseMana(base.Args.Value);
			if (manaGroup != base.Args.Value)
			{
				base.Args.Value = manaGroup;
				base.Args.IsModified = true;
			}
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x000280CF File Offset: 0x000262CF
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaLost);
		}
	}
}
