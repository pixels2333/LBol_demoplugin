using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B2 RID: 434
	[UsedImplicitly]
	public sealed class ZhangchibangQiu : Exhibit
	{
		// Token: 0x0600063C RID: 1596 RVA: 0x0000E710 File Offset: 0x0000C910
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMoney(base.Value1, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Entity,
				Source = this
			});
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0000E737 File Offset: 0x0000C937
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
