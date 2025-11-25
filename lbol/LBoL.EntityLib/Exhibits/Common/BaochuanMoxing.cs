using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 4)]
	public sealed class BaochuanMoxing : Exhibit
	{
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
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
