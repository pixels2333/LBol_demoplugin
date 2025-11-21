using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000105 RID: 261
	[UsedImplicitly]
	public sealed class HealIntention : Intention
	{
		// Token: 0x17000310 RID: 784
		// (get) Token: 0x060009B4 RID: 2484 RVA: 0x0001C138 File Offset: 0x0001A338
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Heal;
			}
		}
	}
}
