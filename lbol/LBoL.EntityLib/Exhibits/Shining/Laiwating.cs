using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000131 RID: 305
	[UsedImplicitly]
	public sealed class Laiwating : ShiningExhibit
	{
		// Token: 0x0600042F RID: 1071 RVA: 0x0000B4BE File Offset: 0x000096BE
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.EnemyVulnerableExtraPercentage += base.Value1;
			base.GameRun.PlayerVulnerableExtraPercentage += base.Value2;
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x0000B4F0 File Offset: 0x000096F0
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.EnemyVulnerableExtraPercentage -= base.Value1;
			base.GameRun.PlayerVulnerableExtraPercentage -= base.Value2;
		}
	}
}
