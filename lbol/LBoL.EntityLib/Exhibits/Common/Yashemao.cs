using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001AB RID: 427
	[UsedImplicitly]
	public sealed class Yashemao : Exhibit
	{
		// Token: 0x06000622 RID: 1570 RVA: 0x0000E478 File Offset: 0x0000C678
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.DrawCardCount += base.Value1;
			if (base.Battle != null)
			{
				base.Battle.DrawCardCount += base.Value1;
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0000E4B2 File Offset: 0x0000C6B2
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.DrawCardCount -= base.Value1;
			if (base.Battle != null)
			{
				base.Battle.DrawCardCount -= base.Value1;
			}
		}
	}
}
