using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A9 RID: 425
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class Xunlongchi : Exhibit
	{
		// Token: 0x06000619 RID: 1561 RVA: 0x0000E3C3 File Offset: 0x0000C5C3
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				base.NotifyActivating();
				((GapStation)args.Station).GapOptions.Add(Library.CreateGapOption<FindExhibit>());
			});
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x0000E3E2 File Offset: 0x0000C5E2
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x0000E3EB File Offset: 0x0000C5EB
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
