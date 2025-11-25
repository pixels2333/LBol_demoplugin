using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(ChaojiMogu.ChaojiMoguWeighter))]
	public sealed class ChaojiMogu : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainPower(base.Value1, false);
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		private class ChaojiMoguWeighter : IExhibitWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.Power <= gameRun.Player.MaxPower / 2) ? 1 : 0);
			}
		}
	}
}
