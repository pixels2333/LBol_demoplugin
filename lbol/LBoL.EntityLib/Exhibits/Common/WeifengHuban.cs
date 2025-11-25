using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class WeifengHuban : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ExtraPowerLowerbound += base.Value1;
			base.GameRun.ExtraPowerUpperbound += base.Value2;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ExtraPowerLowerbound -= base.Value1;
			base.GameRun.ExtraPowerUpperbound -= base.Value2;
		}
	}
}
