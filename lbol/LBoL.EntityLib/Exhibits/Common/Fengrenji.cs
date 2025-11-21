using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000166 RID: 358
	[UsedImplicitly]
	public sealed class Fengrenji : Exhibit
	{
		// Token: 0x060004EE RID: 1262 RVA: 0x0000C818 File Offset: 0x0000AA18
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckDefenseCardFlag + 1;
			gameRun.UpgradeNewDeckDefenseCardFlag = num;
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0000C83C File Offset: 0x0000AA3C
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckDefenseCardFlag - 1;
			gameRun.UpgradeNewDeckDefenseCardFlag = num;
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0000C85E File Offset: 0x0000AA5E
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0000C867 File Offset: 0x0000AA67
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
