using System;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	[ConfigValueConverter(typeof(MinMaxConverter), new string[] { "min-max" })]
	public struct MinMax
	{
		public readonly int Min { get; }
		public readonly int Max { get; }
		public MinMax(int min = 0, int max = 0)
		{
			this.Min = min;
			this.Max = max;
		}
	}
}
