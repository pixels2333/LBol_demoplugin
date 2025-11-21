using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200015F RID: 351
	[UsedImplicitly]
	public sealed class Cookie : Exhibit
	{
		// Token: 0x060004D2 RID: 1234 RVA: 0x0000C5C4 File Offset: 0x0000A7C4
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
			base.GameRun.HealToMaxHp(true, "Cookie");
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0000C5EA File Offset: 0x0000A7EA
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
