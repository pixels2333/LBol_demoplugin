using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A4 RID: 420
	[UsedImplicitly]
	public sealed class WeifengHuban : Exhibit
	{
		// Token: 0x06000604 RID: 1540 RVA: 0x0000E1E5 File Offset: 0x0000C3E5
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ExtraPowerLowerbound += base.Value1;
			base.GameRun.ExtraPowerUpperbound += base.Value2;
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x0000E217 File Offset: 0x0000C417
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ExtraPowerLowerbound -= base.Value1;
			base.GameRun.ExtraPowerUpperbound -= base.Value2;
		}
	}
}
