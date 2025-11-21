using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000160 RID: 352
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class DiannaoPeijian : Exhibit
	{
		// Token: 0x060004D5 RID: 1237 RVA: 0x0000C5FB File Offset: 0x0000A7FB
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.DrinkTeaAdditionalEnergy += base.Value1;
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0000C615 File Offset: 0x0000A815
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0000C61E File Offset: 0x0000A81E
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0000C627 File Offset: 0x0000A827
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.DrinkTeaAdditionalEnergy -= base.Value1;
		}
	}
}
