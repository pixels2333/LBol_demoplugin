using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x020000FD RID: 253
	[UsedImplicitly]
	public sealed class ClearIntention : Intention
	{
		// Token: 0x17000305 RID: 773
		// (get) Token: 0x0600099F RID: 2463 RVA: 0x0001C096 File Offset: 0x0001A296
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Clear;
			}
		}
	}
}
