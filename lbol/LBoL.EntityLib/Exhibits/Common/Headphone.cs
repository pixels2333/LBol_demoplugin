using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200016B RID: 363
	[UsedImplicitly]
	public sealed class Headphone : Exhibit
	{
		// Token: 0x06000506 RID: 1286 RVA: 0x0000CAA7 File Offset: 0x0000ACA7
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.SynergyAdditionalCount += base.Value1;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0000CAC1 File Offset: 0x0000ACC1
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.SynergyAdditionalCount -= base.Value1;
		}
	}
}
