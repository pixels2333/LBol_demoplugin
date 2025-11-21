using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000194 RID: 404
	[UsedImplicitly]
	public sealed class Shouyinji : Exhibit
	{
		// Token: 0x060005B6 RID: 1462 RVA: 0x0000DAB4 File Offset: 0x0000BCB4
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckAbilityCardFlag + 1;
			gameRun.UpgradeNewDeckAbilityCardFlag = num;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x0000DAD6 File Offset: 0x0000BCD6
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x0000DAE0 File Offset: 0x0000BCE0
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckAbilityCardFlag - 1;
			gameRun.UpgradeNewDeckAbilityCardFlag = num;
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x0000DB02 File Offset: 0x0000BD02
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
