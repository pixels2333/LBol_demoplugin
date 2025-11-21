using System;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits
{
	// Token: 0x0200011C RID: 284
	public sealed class KongZhanpinhe : Exhibit
	{
		// Token: 0x060003EB RID: 1003 RVA: 0x0000AE07 File Offset: 0x00009007
		protected override void OnGain(PlayerUnit player)
		{
			base.Blackout = true;
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0000AE10 File Offset: 0x00009010
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
