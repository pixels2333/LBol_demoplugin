using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200018B RID: 395
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class ShanliangDengpao : Exhibit
	{
		// Token: 0x06000591 RID: 1425 RVA: 0x0000D7D5 File Offset: 0x0000B9D5
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				base.NotifyActivating();
				GapStation gapStation = (GapStation)args.Station;
				GetRareCard getRareCard = Library.CreateGapOption<GetRareCard>();
				getRareCard.Value = base.Value1;
				gapStation.GapOptions.Add(getRareCard);
			});
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0000D7F4 File Offset: 0x0000B9F4
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0000D7FD File Offset: 0x0000B9FD
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
