using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000457 RID: 1111
	[UsedImplicitly]
	public sealed class BaseFollower : Card
	{
		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000F0E RID: 3854 RVA: 0x0001B399 File Offset: 0x00019599
		public override bool ShuffleToBottom
		{
			get
			{
				return true;
			}
		}
	}
}
