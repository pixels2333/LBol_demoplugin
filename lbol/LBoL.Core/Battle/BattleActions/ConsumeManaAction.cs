using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000166 RID: 358
	public class ConsumeManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000DE7 RID: 3559 RVA: 0x00026485 File Offset: 0x00024685
		public ConsumeManaAction(ManaGroup group)
		{
			base.Args = new ManaEventArgs
			{
				Value = group
			};
		}

		// Token: 0x06000DE8 RID: 3560 RVA: 0x0002649F File Offset: 0x0002469F
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaConsuming);
		}

		// Token: 0x06000DE9 RID: 3561 RVA: 0x000264B2 File Offset: 0x000246B2
		protected override void MainPhase()
		{
			base.Battle.ConsumeMana(base.Args.Value);
			base.Args.CanCancel = false;
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x000264D6 File Offset: 0x000246D6
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaConsumed);
		}
	}
}
