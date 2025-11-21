using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000185 RID: 389
	[UsedImplicitly]
	public sealed class PingfanDao : Exhibit
	{
		// Token: 0x06000577 RID: 1399 RVA: 0x0000D53D File Offset: 0x0000B73D
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.BasicAttackCardExtraDamage1 += base.Value1;
			base.GameRun.BasicAttackCardExtraDamage2 += base.Value2;
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0000D56F File Offset: 0x0000B76F
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.BasicAttackCardExtraDamage1 -= base.Value1;
			base.GameRun.BasicAttackCardExtraDamage2 -= base.Value2;
		}
	}
}
