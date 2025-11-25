using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Shouyinji : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckAbilityCardFlag + 1;
			gameRun.UpgradeNewDeckAbilityCardFlag = num;
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.UpgradeNewDeckAbilityCardFlag - 1;
			gameRun.UpgradeNewDeckAbilityCardFlag = num;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
