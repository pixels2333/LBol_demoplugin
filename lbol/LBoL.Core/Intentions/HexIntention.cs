using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000106 RID: 262
	[UsedImplicitly]
	public sealed class HexIntention : Intention
	{
		// Token: 0x17000311 RID: 785
		// (get) Token: 0x060009B6 RID: 2486 RVA: 0x0001C143 File Offset: 0x0001A343
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Hex;
			}
		}
	}
}
