using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000179 RID: 377
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class JingzhiChaju : Exhibit
	{
		// Token: 0x06000541 RID: 1345 RVA: 0x0000CFA1 File Offset: 0x0000B1A1
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.DrinkTeaAdditionalHeal += base.Value1;
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x0000CFBB File Offset: 0x0000B1BB
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x0000CFC4 File Offset: 0x0000B1C4
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x0000CFCD File Offset: 0x0000B1CD
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.DrinkTeaAdditionalHeal -= base.Value1;
		}
	}
}
