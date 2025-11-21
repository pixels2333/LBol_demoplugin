using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C8 RID: 456
	[UsedImplicitly]
	public sealed class WaijieYanjing : Exhibit
	{
		// Token: 0x06000699 RID: 1689 RVA: 0x0000F06E File Offset: 0x0000D26E
		private float GetMultiplier()
		{
			return (float)(100 + base.Value1) / 100f;
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x0000F080 File Offset: 0x0000D280
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.RewardMoneyMultiplier *= this.GetMultiplier();
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x0000F09A File Offset: 0x0000D29A
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.RewardMoneyMultiplier /= this.GetMultiplier();
		}
	}
}
