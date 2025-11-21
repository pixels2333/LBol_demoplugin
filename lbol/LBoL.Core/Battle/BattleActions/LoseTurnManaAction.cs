using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000192 RID: 402
	public class LoseTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000ED1 RID: 3793 RVA: 0x00028197 File Offset: 0x00026397
		public LoseTurnManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}

		// Token: 0x06000ED2 RID: 3794 RVA: 0x000281B1 File Offset: 0x000263B1
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLosing);
		}

		// Token: 0x06000ED3 RID: 3795 RVA: 0x000281C4 File Offset: 0x000263C4
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.LoseTurnMana(base.Args.Value);
		}

		// Token: 0x06000ED4 RID: 3796 RVA: 0x000281E7 File Offset: 0x000263E7
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaLost);
		}
	}
}
