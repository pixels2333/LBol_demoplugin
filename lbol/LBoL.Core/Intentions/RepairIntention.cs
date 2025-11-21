using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x0200010A RID: 266
	[UsedImplicitly]
	public sealed class RepairIntention : Intention
	{
		// Token: 0x17000318 RID: 792
		// (get) Token: 0x060009C3 RID: 2499 RVA: 0x0001C1B7 File Offset: 0x0001A3B7
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Repair;
			}
		}
	}
}
