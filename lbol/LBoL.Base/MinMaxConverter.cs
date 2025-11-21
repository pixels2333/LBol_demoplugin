using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000017 RID: 23
	public sealed class MinMaxConverter : IMultiSegConfigValueConverter<MinMax>
	{
		// Token: 0x0600009C RID: 156 RVA: 0x000047CF File Offset: 0x000029CF
		public MinMax Parse(string[] segs)
		{
			return new MinMax(int.Parse(segs[0]), int.Parse(segs[1]));
		}

		// Token: 0x0600009D RID: 157 RVA: 0x000047E6 File Offset: 0x000029E6
		public void WriteTo(BinaryWriter writer, MinMax value)
		{
			writer.Write(value.Min);
			writer.Write(value.Max);
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00004804 File Offset: 0x00002A04
		public MinMax ReadFrom(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			return new MinMax(num, num2);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00004824 File Offset: 0x00002A24
		public string ToString(MinMax value)
		{
			return string.Format("{0}, {1}", value.Min, value.Max);
		}
	}
}
