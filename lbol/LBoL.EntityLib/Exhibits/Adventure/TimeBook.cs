using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class TimeBook : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.CanViewDrawZoneActualOrder + 1;
			gameRun.CanViewDrawZoneActualOrder = num;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.CanViewDrawZoneActualOrder - 1;
			gameRun.CanViewDrawZoneActualOrder = num;
		}
	}
}
