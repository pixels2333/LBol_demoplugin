using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	public sealed class MinMaxConverter : IMultiSegConfigValueConverter<MinMax>
	{
		public MinMax Parse(string[] segs)
		{
			return new MinMax(int.Parse(segs[0]), int.Parse(segs[1]));
		}
		public void WriteTo(BinaryWriter writer, MinMax value)
		{
			writer.Write(value.Min);
			writer.Write(value.Max);
		}
		public MinMax ReadFrom(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			return new MinMax(num, num2);
		}
		public string ToString(MinMax value)
		{
			return string.Format("{0}, {1}", value.Min, value.Max);
		}
	}
}
