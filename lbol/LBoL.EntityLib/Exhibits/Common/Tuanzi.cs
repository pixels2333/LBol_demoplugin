using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A2 RID: 418
	[UsedImplicitly]
	public sealed class Tuanzi : Exhibit
	{
		// Token: 0x060005FB RID: 1531 RVA: 0x0000E09C File Offset: 0x0000C29C
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				base.GameRun.Heal(base.Value1, true, null);
				base.GameRun.GainPower(base.Value2, false);
			}
		}
	}
}
