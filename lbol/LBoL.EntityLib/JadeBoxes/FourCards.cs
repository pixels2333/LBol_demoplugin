using System;
using JetBrains.Annotations;
using LBoL.Core;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000113 RID: 275
	[UsedImplicitly]
	public sealed class FourCards : JadeBox
	{
		// Token: 0x060003C9 RID: 969 RVA: 0x0000A8F8 File Offset: 0x00008AF8
		protected override void OnAdded()
		{
			base.GameRun.LootCardCommonDupeCount += base.Value1;
			base.GameRun.LootCardUncommonDupeCount += base.Value2;
		}
	}
}
