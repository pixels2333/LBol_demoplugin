using System;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x02000117 RID: 279
	[UsedImplicitly]
	public sealed class FindExhibit : GapOption
	{
		// Token: 0x17000339 RID: 825
		// (get) Token: 0x06000A0E RID: 2574 RVA: 0x0001CA3E File Offset: 0x0001AC3E
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.FindExhibit;
			}
		}
	}
}
