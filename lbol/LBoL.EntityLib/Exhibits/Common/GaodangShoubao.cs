using System;
using JetBrains.Annotations;
using LBoL.Core;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9, WeighterType = typeof(GaodangShoubao.GaodangShoubaoWeighter))]
	public sealed class GaodangShoubao : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		private class GaodangShoubaoWeighter : IExhibitWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.CurrentStation.Level == 10) ? 0 : 1);
			}
		}
	}
}
