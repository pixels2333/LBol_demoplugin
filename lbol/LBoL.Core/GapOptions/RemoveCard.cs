using System;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x02000119 RID: 281
	[UsedImplicitly]
	public sealed class RemoveCard : GapOption
	{
		// Token: 0x1700033C RID: 828
		// (get) Token: 0x06000A14 RID: 2580 RVA: 0x0001CA65 File Offset: 0x0001AC65
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.RemoveCard;
			}
		}
	}
}
