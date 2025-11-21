using System;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x02000118 RID: 280
	[UsedImplicitly]
	public sealed class GetMoney : GapOption
	{
		// Token: 0x1700033A RID: 826
		// (get) Token: 0x06000A10 RID: 2576 RVA: 0x0001CA49 File Offset: 0x0001AC49
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.GetMoney;
			}
		}

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06000A11 RID: 2577 RVA: 0x0001CA4C File Offset: 0x0001AC4C
		// (set) Token: 0x06000A12 RID: 2578 RVA: 0x0001CA54 File Offset: 0x0001AC54
		[UsedImplicitly]
		public int Value { get; set; }
	}
}
