using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000193 RID: 403
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class ShoushiYubi : Exhibit
	{
		// Token: 0x060005B1 RID: 1457 RVA: 0x0000DA58 File Offset: 0x0000BC58
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				base.NotifyActivating();
				((GapStation)args.Station).GapOptions.Add(Library.CreateGapOption<RemoveCard>());
			});
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x0000DA77 File Offset: 0x0000BC77
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x0000DA80 File Offset: 0x0000BC80
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
