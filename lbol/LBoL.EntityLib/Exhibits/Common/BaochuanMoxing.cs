using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000155 RID: 341
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 4)]
	public sealed class BaochuanMoxing : Exhibit
	{
		// Token: 0x060004A7 RID: 1191 RVA: 0x0000C1A0 File Offset: 0x0000A3A0
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate(StationEventArgs _)
			{
				base.GameRun.GainMoney(base.Value1, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Entity,
					Source = this
				});
				base.NotifyActivating();
			});
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x0000C1BF File Offset: 0x0000A3BF
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x0000C1C8 File Offset: 0x0000A3C8
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
