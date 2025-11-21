using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x0200010D RID: 269
	[UsedImplicitly]
	public sealed class SpawnIntention : Intention
	{
		// Token: 0x1700031B RID: 795
		// (get) Token: 0x060009C9 RID: 2505 RVA: 0x0001C1DB File Offset: 0x0001A3DB
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Spawn;
			}
		}
	}
}
