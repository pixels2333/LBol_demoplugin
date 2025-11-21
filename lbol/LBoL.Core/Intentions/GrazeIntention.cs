using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000104 RID: 260
	[UsedImplicitly]
	public sealed class GrazeIntention : Intention
	{
		// Token: 0x1700030F RID: 783
		// (get) Token: 0x060009B2 RID: 2482 RVA: 0x0001C12D File Offset: 0x0001A32D
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Graze;
			}
		}
	}
}
