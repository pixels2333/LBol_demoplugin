using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000109 RID: 265
	[UsedImplicitly]
	public sealed class PositiveEffectIntention : Intention
	{
		// Token: 0x17000317 RID: 791
		// (get) Token: 0x060009C1 RID: 2497 RVA: 0x0001C1AC File Offset: 0x0001A3AC
		public override IntentionType Type
		{
			get
			{
				return IntentionType.PositiveEffect;
			}
		}
	}
}
