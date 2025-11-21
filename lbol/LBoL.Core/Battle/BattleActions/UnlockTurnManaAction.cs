using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001AF RID: 431
	public class UnlockTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000F5D RID: 3933 RVA: 0x000294F7 File Offset: 0x000276F7
		public UnlockTurnManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x00029511 File Offset: 0x00027711
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaUnlocking);
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x00029524 File Offset: 0x00027724
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.UnlockTurnMana(base.Args.Value);
		}

		// Token: 0x06000F60 RID: 3936 RVA: 0x00029547 File Offset: 0x00027747
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaUnlocked);
		}
	}
}
