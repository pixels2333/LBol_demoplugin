using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000199 RID: 409
	[UsedImplicitly]
	public sealed class Taozi : Exhibit
	{
		// Token: 0x060005D1 RID: 1489 RVA: 0x0000DD23 File Offset: 0x0000BF23
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x0000DD38 File Offset: 0x0000BF38
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
