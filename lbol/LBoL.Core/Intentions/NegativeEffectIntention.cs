using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000108 RID: 264
	[UsedImplicitly]
	public sealed class NegativeEffectIntention : Intention
	{
		// Token: 0x17000316 RID: 790
		// (get) Token: 0x060009BF RID: 2495 RVA: 0x0001C1A1 File Offset: 0x0001A3A1
		public override IntentionType Type
		{
			get
			{
				return IntentionType.NegativeEffect;
			}
		}
	}
}
