using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x020000FA RID: 250
	[UsedImplicitly]
	public sealed class AddCardIntention : Intention
	{
		// Token: 0x170002FA RID: 762
		// (get) Token: 0x0600098D RID: 2445 RVA: 0x0001BEE0 File Offset: 0x0001A0E0
		public override IntentionType Type
		{
			get
			{
				return IntentionType.AddCard;
			}
		}
	}
}
