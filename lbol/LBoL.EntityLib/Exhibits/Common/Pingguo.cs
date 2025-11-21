using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000186 RID: 390
	[UsedImplicitly]
	public sealed class Pingguo : Exhibit
	{
		// Token: 0x0600057A RID: 1402 RVA: 0x0000D5A9 File Offset: 0x0000B7A9
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0000D5BE File Offset: 0x0000B7BE
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
