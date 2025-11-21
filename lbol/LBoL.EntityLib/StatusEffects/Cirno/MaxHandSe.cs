using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000E4 RID: 228
	[UsedImplicitly]
	public sealed class MaxHandSe : StatusEffect
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000332 RID: 818 RVA: 0x000088EB File Offset: 0x00006AEB
		[UsedImplicitly]
		public int MaxHand
		{
			get
			{
				return 12;
			}
		}

		// Token: 0x06000333 RID: 819 RVA: 0x000088EF File Offset: 0x00006AEF
		protected override void OnAdded(Unit unit)
		{
			base.Battle.MaxHand = this.MaxHand;
		}
	}
}
