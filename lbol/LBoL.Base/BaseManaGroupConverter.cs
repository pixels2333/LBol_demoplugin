using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000006 RID: 6
	public sealed class BaseManaGroupConverter : IConfigValueConverter<BaseManaGroup>
	{
		// Token: 0x0600001D RID: 29 RVA: 0x0000255E File Offset: 0x0000075E
		public BaseManaGroup Parse(string value)
		{
			return ManaGroup.Parse(value);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x0000256C File Offset: 0x0000076C
		public void WriteTo(BinaryWriter writer, BaseManaGroup value)
		{
			value.Value.WriteTo(writer);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002589 File Offset: 0x00000789
		public BaseManaGroup ReadFrom(BinaryReader reader)
		{
			return ManaGroup.FromBinary(reader);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002596 File Offset: 0x00000796
		public string ToString(BaseManaGroup value)
		{
			return value.ToString();
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000021 RID: 33 RVA: 0x000025A5 File Offset: 0x000007A5
		public bool IsScalar
		{
			get
			{
				return true;
			}
		}
	}
}
