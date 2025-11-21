using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000278 RID: 632
	[UsedImplicitly]
	public sealed class NiuqiDefense : Card
	{
		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x00015295 File Offset: 0x00013495
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.Battle.DiscardZone.Count;
			}
		}
	}
}
