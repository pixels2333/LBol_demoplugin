using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000130 RID: 304
	[UsedImplicitly]
	public sealed class KongbaiKapai : ShiningExhibit
	{
		// Token: 0x0600042C RID: 1068 RVA: 0x0000B440 File Offset: 0x00009640
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RewardAndShopCardColorLimitFlag + 1;
			gameRun.RewardAndShopCardColorLimitFlag = num;
			base.GameRun.AdditionalRewardCardCount += base.Value1;
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0000B47C File Offset: 0x0000967C
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RewardAndShopCardColorLimitFlag - 1;
			gameRun.RewardAndShopCardColorLimitFlag = num;
			base.GameRun.AdditionalRewardCardCount -= base.Value1;
		}
	}
}
