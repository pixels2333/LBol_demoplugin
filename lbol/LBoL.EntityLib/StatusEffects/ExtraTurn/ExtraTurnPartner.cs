using System;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn
{
	// Token: 0x02000084 RID: 132
	public abstract class ExtraTurnPartner : StatusEffect
	{
		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x00005A23 File Offset: 0x00003C23
		// (set) Token: 0x060001D7 RID: 471 RVA: 0x00005A2B File Offset: 0x00003C2B
		protected bool ThisTurnActivating { get; set; }

		// Token: 0x060001D8 RID: 472 RVA: 0x00005A34 File Offset: 0x00003C34
		protected override string GetBaseDescription()
		{
			if (!this.ThisTurnActivating)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}
	}
}
