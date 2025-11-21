using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000100 RID: 256
	[UsedImplicitly]
	public sealed class DoNothingIntention : Intention
	{
		// Token: 0x17000309 RID: 777
		// (get) Token: 0x060009A7 RID: 2471 RVA: 0x0001C0CA File Offset: 0x0001A2CA
		public override IntentionType Type
		{
			get
			{
				return IntentionType.DoNothing;
			}
		}
	}
}
