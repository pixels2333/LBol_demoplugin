using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x020000FF RID: 255
	[UsedImplicitly]
	public sealed class DefendIntention : Intention
	{
		// Token: 0x17000308 RID: 776
		// (get) Token: 0x060009A5 RID: 2469 RVA: 0x0001C0BF File Offset: 0x0001A2BF
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Defend;
			}
		}
	}
}
