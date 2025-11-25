using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class DiannaoPeijian : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.DrinkTeaAdditionalEnergy += base.Value1;
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.DrinkTeaAdditionalEnergy -= base.Value1;
		}
	}
}
