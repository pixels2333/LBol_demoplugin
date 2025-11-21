using System;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x0200011A RID: 282
	[UsedImplicitly]
	public sealed class GetRareCard : GapOption
	{
		// Token: 0x1700033D RID: 829
		// (get) Token: 0x06000A16 RID: 2582 RVA: 0x0001CA70 File Offset: 0x0001AC70
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.GetRareCard;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x06000A17 RID: 2583 RVA: 0x0001CA73 File Offset: 0x0001AC73
		// (set) Token: 0x06000A18 RID: 2584 RVA: 0x0001CA7B File Offset: 0x0001AC7B
		[UsedImplicitly]
		public int Value { get; set; }
	}
}
