using System;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x0200011B RID: 283
	[UsedImplicitly]
	public sealed class UpgradeBaota : GapOption
	{
		// Token: 0x1700033F RID: 831
		// (get) Token: 0x06000A1A RID: 2586 RVA: 0x0001CA8C File Offset: 0x0001AC8C
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.UpgradeBaota;
			}
		}
	}
}
