using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x0200010B RID: 267
	[UsedImplicitly]
	public sealed class SleepIntention : Intention
	{
		// Token: 0x17000319 RID: 793
		// (get) Token: 0x060009C5 RID: 2501 RVA: 0x0001C1C3 File Offset: 0x0001A3C3
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Sleep;
			}
		}
	}
}
