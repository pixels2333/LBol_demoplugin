using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class Laiwating : ShiningExhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.EnemyVulnerableExtraPercentage += base.Value1;
			base.GameRun.PlayerVulnerableExtraPercentage += base.Value2;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.EnemyVulnerableExtraPercentage -= base.Value1;
			base.GameRun.PlayerVulnerableExtraPercentage -= base.Value2;
		}
	}
}
