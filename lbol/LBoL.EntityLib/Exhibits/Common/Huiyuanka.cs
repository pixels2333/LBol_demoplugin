using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000171 RID: 369
	[UsedImplicitly]
	public sealed class Huiyuanka : Exhibit
	{
		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600051D RID: 1309 RVA: 0x0000CC78 File Offset: 0x0000AE78
		private float Multiplier
		{
			get
			{
				return (float)(100 - base.Value1) / 100f;
			}
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x0000CC8A File Offset: 0x0000AE8A
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ShopPriceMultiplier *= this.Multiplier;
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x0000CCA4 File Offset: 0x0000AEA4
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x0000CCAD File Offset: 0x0000AEAD
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x0000CCB6 File Offset: 0x0000AEB6
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ShopPriceMultiplier /= this.Multiplier;
		}
	}
}
