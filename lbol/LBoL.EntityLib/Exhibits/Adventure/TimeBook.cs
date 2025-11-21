using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C7 RID: 455
	[UsedImplicitly]
	public sealed class TimeBook : Exhibit
	{
		// Token: 0x06000696 RID: 1686 RVA: 0x0000F020 File Offset: 0x0000D220
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.CanViewDrawZoneActualOrder + 1;
			gameRun.CanViewDrawZoneActualOrder = num;
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x0000F044 File Offset: 0x0000D244
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.CanViewDrawZoneActualOrder - 1;
			gameRun.CanViewDrawZoneActualOrder = num;
		}
	}
}
