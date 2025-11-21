using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200018C RID: 396
	public class LockTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000EBC RID: 3772 RVA: 0x00027EB3 File Offset: 0x000260B3
		public LockTurnManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x00027ECD File Offset: 0x000260CD
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLocking);
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x00027EE0 File Offset: 0x000260E0
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.LockTurnMana(base.Args.Value);
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x00027F03 File Offset: 0x00026103
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLocked);
		}
	}
}
