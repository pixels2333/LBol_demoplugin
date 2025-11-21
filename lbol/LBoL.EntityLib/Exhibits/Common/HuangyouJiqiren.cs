using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200016F RID: 367
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class HuangyouJiqiren : Exhibit
	{
		// Token: 0x06000515 RID: 1301 RVA: 0x0000CBDC File Offset: 0x0000ADDC
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.DrinkTeaCardRewardFlag + 1;
			gameRun.DrinkTeaCardRewardFlag = num;
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0000CBFE File Offset: 0x0000ADFE
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0000CC07 File Offset: 0x0000AE07
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0000CC10 File Offset: 0x0000AE10
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.DrinkTeaCardRewardFlag - 1;
			gameRun.DrinkTeaCardRewardFlag = num;
		}
	}
}
