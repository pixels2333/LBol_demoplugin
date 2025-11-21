using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000191 RID: 401
	[UsedImplicitly]
	public sealed class Shoubiao : Exhibit
	{
		// Token: 0x060005A8 RID: 1448 RVA: 0x0000D9B8 File Offset: 0x0000BBB8
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckSkillCardFlag + 1;
			gameRun.UpgradeNewDeckSkillCardFlag = num;
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x0000D9DC File Offset: 0x0000BBDC
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckSkillCardFlag - 1;
			gameRun.UpgradeNewDeckSkillCardFlag = num;
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x0000D9FE File Offset: 0x0000BBFE
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x0000DA07 File Offset: 0x0000BC07
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
