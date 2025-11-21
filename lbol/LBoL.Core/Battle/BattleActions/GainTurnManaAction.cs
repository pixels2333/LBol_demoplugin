using System;
using LBoL.Base;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000187 RID: 391
	public class GainTurnManaAction : SimpleEventBattleAction<ManaEventArgs>
	{
		// Token: 0x06000EA5 RID: 3749 RVA: 0x00027CAB File Offset: 0x00025EAB
		public GainTurnManaAction(ManaGroup value)
		{
			if (value.Any > 0)
			{
				throw new InvalidOperationException("Cant gain extra turn mana with color: Any.");
			}
			base.Args = new ManaEventArgs
			{
				Value = value
			};
		}

		// Token: 0x06000EA6 RID: 3750 RVA: 0x00027CDA File Offset: 0x00025EDA
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.TurnManaGaining);
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x00027CED File Offset: 0x00025EED
		protected override void MainPhase()
		{
			base.Args.Value = base.Battle.GainTurnMana(base.Args.Value);
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x00027D10 File Offset: 0x00025F10
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.TurnManaGained);
		}
	}
}
