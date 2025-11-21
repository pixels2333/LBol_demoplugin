using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x020000FC RID: 252
	[UsedImplicitly]
	public sealed class ChargeIntention : Intention
	{
		// Token: 0x17000304 RID: 772
		// (get) Token: 0x0600099D RID: 2461 RVA: 0x0001C08A File Offset: 0x0001A28A
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Charge;
			}
		}
	}
}
