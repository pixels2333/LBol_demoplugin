using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000187 RID: 391
	[UsedImplicitly]
	public sealed class PortalGun : Exhibit
	{
		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600057D RID: 1405 RVA: 0x0000D5CF File Offset: 0x0000B7CF
		private float Multiplier
		{
			get
			{
				return (float)(100 - base.Value1) / 100f;
			}
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0000D5E4 File Offset: 0x0000B7E4
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ShopPriceMultiplier *= this.Multiplier;
			GameRunController gameRun = base.GameRun;
			int num = gameRun.ShopResupplyFlag + 1;
			gameRun.ShopResupplyFlag = num;
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0000D61E File Offset: 0x0000B81E
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0000D627 File Offset: 0x0000B827
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0000D630 File Offset: 0x0000B830
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ShopPriceMultiplier /= this.Multiplier;
			GameRunController gameRun = base.GameRun;
			int num = gameRun.ShopResupplyFlag - 1;
			gameRun.ShopResupplyFlag = num;
		}
	}
}
