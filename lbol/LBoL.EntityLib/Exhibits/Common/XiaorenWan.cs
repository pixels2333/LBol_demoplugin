using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 4)]
	public sealed class XiaorenWan : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.RewardAbandoned, delegate(GameEventArgs _)
			{
				base.NotifyActivating();
				base.GameRun.GainMaxHp(base.Value1, true, true);
			});
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
