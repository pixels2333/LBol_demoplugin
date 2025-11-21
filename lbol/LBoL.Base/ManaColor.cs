using System;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x0200000F RID: 15
	[ConfigValueConverter(typeof(ManaColorConverter), new string[] { })]
	public enum ManaColor
	{
		// Token: 0x04000072 RID: 114
		Any,
		// Token: 0x04000073 RID: 115
		White,
		// Token: 0x04000074 RID: 116
		Blue,
		// Token: 0x04000075 RID: 117
		Black,
		// Token: 0x04000076 RID: 118
		Red,
		// Token: 0x04000077 RID: 119
		Green,
		// Token: 0x04000078 RID: 120
		Colorless,
		// Token: 0x04000079 RID: 121
		Philosophy,
		// Token: 0x0400007A RID: 122
		Hybrid
	}
}
