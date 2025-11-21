using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001CA RID: 458
	[UsedImplicitly]
	public sealed class WaijieYouxiji : Exhibit
	{
		// Token: 0x060006A2 RID: 1698 RVA: 0x0000F198 File Offset: 0x0000D398
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				base.GameRun.GainPower(base.Value1, true);
			}
		}
	}
}
