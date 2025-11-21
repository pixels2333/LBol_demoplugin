using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000101 RID: 257
	[UsedImplicitly]
	public sealed class EscapeIntention : Intention
	{
		// Token: 0x1700030A RID: 778
		// (get) Token: 0x060009A9 RID: 2473 RVA: 0x0001C0D6 File Offset: 0x0001A2D6
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Escape;
			}
		}
	}
}
