using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000110 RID: 272
	[UsedImplicitly]
	public sealed class UnknownIntention : Intention
	{
		// Token: 0x17000327 RID: 807
		// (get) Token: 0x060009DD RID: 2525 RVA: 0x0001C385 File Offset: 0x0001A585
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Unknown;
			}
		}
	}
}
