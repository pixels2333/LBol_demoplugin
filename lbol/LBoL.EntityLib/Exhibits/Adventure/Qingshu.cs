using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C5 RID: 453
	[UsedImplicitly]
	public sealed class Qingshu : Exhibit
	{
		// Token: 0x0600068F RID: 1679 RVA: 0x0000EFB5 File Offset: 0x0000D1B5
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x0000EFCA File Offset: 0x0000D1CA
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x0000EFD3 File Offset: 0x0000D1D3
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
