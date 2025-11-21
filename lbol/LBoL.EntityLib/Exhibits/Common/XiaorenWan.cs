using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A6 RID: 422
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 4)]
	public sealed class XiaorenWan : Exhibit
	{
		// Token: 0x0600060B RID: 1547 RVA: 0x0000E296 File Offset: 0x0000C496
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.RewardAbandoned, delegate(GameEventArgs _)
			{
				base.NotifyActivating();
				base.GameRun.GainMaxHp(base.Value1, true, true);
			});
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x0000E2B5 File Offset: 0x0000C4B5
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x0000E2BE File Offset: 0x0000C4BE
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
