using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class KongbaiKapai : ShiningExhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RewardAndShopCardColorLimitFlag + 1;
			gameRun.RewardAndShopCardColorLimitFlag = num;
			base.GameRun.AdditionalRewardCardCount += base.Value1;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RewardAndShopCardColorLimitFlag - 1;
			gameRun.RewardAndShopCardColorLimitFlag = num;
			base.GameRun.AdditionalRewardCardCount -= base.Value1;
		}
	}
}
