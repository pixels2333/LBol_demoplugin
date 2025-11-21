using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000188 RID: 392
	[UsedImplicitly]
	public sealed class Putao : Exhibit
	{
		// Token: 0x06000583 RID: 1411 RVA: 0x0000D672 File Offset: 0x0000B872
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0000D687 File Offset: 0x0000B887
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
