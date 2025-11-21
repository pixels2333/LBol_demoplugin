using System;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000016 RID: 22
	[ConfigValueConverter(typeof(MinMaxConverter), new string[] { "min-max" })]
	public struct MinMax
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000099 RID: 153 RVA: 0x0000479F File Offset: 0x0000299F
		public readonly int Min { get; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600009A RID: 154 RVA: 0x000047A7 File Offset: 0x000029A7
		public readonly int Max { get; }

		// Token: 0x0600009B RID: 155 RVA: 0x000047B0 File Offset: 0x000029B0
		public MinMax(int min = 0, int max = 0)
		{
			this.Min = min;
			this.Max = max;
		}
	}
}
