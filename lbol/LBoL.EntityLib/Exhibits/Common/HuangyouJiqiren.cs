using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class HuangyouJiqiren : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.DrinkTeaCardRewardFlag + 1;
			gameRun.DrinkTeaCardRewardFlag = num;
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
			GameRunController gameRun = base.GameRun;
			int num = gameRun.DrinkTeaCardRewardFlag - 1;
			gameRun.DrinkTeaCardRewardFlag = num;
		}
	}
}
