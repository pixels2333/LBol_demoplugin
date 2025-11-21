using System;
using JetBrains.Annotations;
using LBoL.Core;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000168 RID: 360
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9, WeighterType = typeof(GaodangShoubao.GaodangShoubaoWeighter))]
	public sealed class GaodangShoubao : Exhibit
	{
		// Token: 0x060004FC RID: 1276 RVA: 0x0000C9EE File Offset: 0x0000ABEE
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0000C9F7 File Offset: 0x0000ABF7
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x0200063D RID: 1597
		private class GaodangShoubaoWeighter : IExhibitWeighter
		{
			// Token: 0x0600194C RID: 6476 RVA: 0x00033963 File Offset: 0x00031B63
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.CurrentStation.Level == 10) ? 0 : 1);
			}
		}
	}
}
