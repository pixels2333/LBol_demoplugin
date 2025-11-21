using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x020000FE RID: 254
	[UsedImplicitly]
	public sealed class CountDownIntention : Intention
	{
		// Token: 0x17000306 RID: 774
		// (get) Token: 0x060009A1 RID: 2465 RVA: 0x0001C0A2 File Offset: 0x0001A2A2
		public override IntentionType Type
		{
			get
			{
				return IntentionType.CountDown;
			}
		}

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x060009A2 RID: 2466 RVA: 0x0001C0A6 File Offset: 0x0001A2A6
		// (set) Token: 0x060009A3 RID: 2467 RVA: 0x0001C0AE File Offset: 0x0001A2AE
		public int Counter { get; internal set; }
	}
}
