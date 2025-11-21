using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000183 RID: 387
	public class GainManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000E99 RID: 3737 RVA: 0x00027B82 File Offset: 0x00025D82
		public GainManaAction(ManaGroup value)
		{
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}

		// Token: 0x06000E9A RID: 3738 RVA: 0x00027B9C File Offset: 0x00025D9C
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaGaining);
		}

		// Token: 0x06000E9B RID: 3739 RVA: 0x00027BAF File Offset: 0x00025DAF
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.GainMana(base.Args.Value);
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x00027BD2 File Offset: 0x00025DD2
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaGained);
		}
	}
}
