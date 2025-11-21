using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x0200010C RID: 268
	[UsedImplicitly]
	public sealed class SpawnDroneIntention : Intention
	{
		// Token: 0x1700031A RID: 794
		// (get) Token: 0x060009C7 RID: 2503 RVA: 0x0001C1CF File Offset: 0x0001A3CF
		public override IntentionType Type
		{
			get
			{
				return IntentionType.SpawnDrone;
			}
		}
	}
}
