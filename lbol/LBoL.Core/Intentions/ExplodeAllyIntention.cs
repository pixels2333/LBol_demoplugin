using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000102 RID: 258
	[UsedImplicitly]
	public sealed class ExplodeAllyIntention : Intention
	{
		// Token: 0x1700030B RID: 779
		// (get) Token: 0x060009AB RID: 2475 RVA: 0x0001C0E2 File Offset: 0x0001A2E2
		public override IntentionType Type
		{
			get
			{
				return IntentionType.ExplodeAlly;
			}
		}
	}
}
