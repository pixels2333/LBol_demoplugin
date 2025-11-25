using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class WaijieYanjing : Exhibit
	{
		private float GetMultiplier()
		{
			return (float)(100 + base.Value1) / 100f;
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.RewardMoneyMultiplier *= this.GetMultiplier();
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.RewardMoneyMultiplier /= this.GetMultiplier();
		}
	}
}
