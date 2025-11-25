using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Headphone : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.SynergyAdditionalCount += base.Value1;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.SynergyAdditionalCount -= base.Value1;
		}
	}
}
