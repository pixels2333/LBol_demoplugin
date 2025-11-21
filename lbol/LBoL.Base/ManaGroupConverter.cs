using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000015 RID: 21
	public sealed class ManaGroupConverter : IConfigValueConverter<ManaGroup>
	{
		// Token: 0x06000094 RID: 148 RVA: 0x0000476E File Offset: 0x0000296E
		public ManaGroup Parse(string value)
		{
			return ManaGroup.Parse(value);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00004776 File Offset: 0x00002976
		public void WriteTo(BinaryWriter writer, ManaGroup value)
		{
			value.WriteTo(writer);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00004780 File Offset: 0x00002980
		public ManaGroup ReadFrom(BinaryReader reader)
		{
			return ManaGroup.FromBinary(reader);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00004788 File Offset: 0x00002988
		public string ToString(ManaGroup value)
		{
			return value.ToString();
		}
	}
}
