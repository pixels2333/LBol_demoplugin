using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B8 RID: 440
	[UsedImplicitly]
	public sealed class Zixingche : Exhibit
	{
		// Token: 0x06000653 RID: 1619 RVA: 0x0000EA14 File Offset: 0x0000CC14
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckAttackCardFlag + 1;
			gameRun.UpgradeNewDeckAttackCardFlag = num;
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0000EA36 File Offset: 0x0000CC36
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0000EA40 File Offset: 0x0000CC40
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckAttackCardFlag - 1;
			gameRun.UpgradeNewDeckAttackCardFlag = num;
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0000EA62 File Offset: 0x0000CC62
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
