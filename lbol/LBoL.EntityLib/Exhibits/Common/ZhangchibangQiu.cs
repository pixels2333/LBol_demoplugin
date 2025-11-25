using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class ZhangchibangQiu : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMoney(base.Value1, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Entity,
				Source = this
			});
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
