using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class JuanzengZhengming : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainPower(base.Value2, false);
		}
		protected override void OnAdded(PlayerUnit player)
		{
			player.Us.MaxPowerLevel += base.Value1;
			player.Us.UsRepeatableType = UsRepeatableType.FreeToUse;
		}
	}
}
