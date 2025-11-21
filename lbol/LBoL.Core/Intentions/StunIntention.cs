using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x0200010F RID: 271
	[UsedImplicitly]
	public sealed class StunIntention : Intention
	{
		// Token: 0x17000326 RID: 806
		// (get) Token: 0x060009DB RID: 2523 RVA: 0x0001C379 File Offset: 0x0001A579
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Stun;
			}
		}
	}
}
