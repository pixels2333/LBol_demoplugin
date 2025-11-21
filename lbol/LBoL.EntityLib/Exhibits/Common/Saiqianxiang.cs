using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000189 RID: 393
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class Saiqianxiang : Exhibit
	{
		// Token: 0x06000586 RID: 1414 RVA: 0x0000D698 File Offset: 0x0000B898
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				base.NotifyActivating();
				GapStation gapStation = (GapStation)args.Station;
				GetMoney getMoney = Library.CreateGapOption<GetMoney>();
				getMoney.Value = base.Value1;
				gapStation.GapOptions.Add(getMoney);
			});
			base.GameRun.RewardCardAbandonMoney += base.Value2;
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0000D6CF File Offset: 0x0000B8CF
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.RewardCardAbandonMoney -= base.Value2;
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x0000D6E9 File Offset: 0x0000B8E9
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x0000D6F2 File Offset: 0x0000B8F2
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
