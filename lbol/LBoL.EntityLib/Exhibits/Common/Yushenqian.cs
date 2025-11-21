using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B1 RID: 433
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class Yushenqian : Exhibit
	{
		// Token: 0x06000637 RID: 1591 RVA: 0x0000E6C2 File Offset: 0x0000C8C2
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.AdditionalRewardCardCount += base.Value1;
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0000E6DC File Offset: 0x0000C8DC
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0000E6E5 File Offset: 0x0000C8E5
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0000E6EE File Offset: 0x0000C8EE
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.AdditionalRewardCardCount -= base.Value1;
		}
	}
}
