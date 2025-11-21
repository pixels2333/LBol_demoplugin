using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000174 RID: 372
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class JiangyouPing : Exhibit
	{
		// Token: 0x0600052D RID: 1325 RVA: 0x0000CDEF File Offset: 0x0000AFEF
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate(StationEventArgs args)
			{
				if (args.Station.Type == StationType.Shop)
				{
					base.NotifyActivating();
					base.GameRun.Heal(base.Value1, true, "JiangyouPing");
					base.GameRun.GainPower(base.Value2, false);
				}
			});
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0000CE0E File Offset: 0x0000B00E
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x0000CE17 File Offset: 0x0000B017
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
