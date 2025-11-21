using System;

namespace LBoL.Core
{
	// Token: 0x0200000A RID: 10
	public class EntityName : IFormattable
	{
		// Token: 0x06000038 RID: 56 RVA: 0x0000291E File Offset: 0x00000B1E
		public EntityName(string qualifiedId, string fallback)
		{
			this._qualifiedId = qualifiedId;
			this._fallback = fallback;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002934 File Offset: 0x00000B34
		public string ToString(string format)
		{
			return EntityNameTable.TryGet(this._qualifiedId, format) ?? this._fallback;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x0000294C File Offset: 0x00000B4C
		public override string ToString()
		{
			return this.ToString(string.Empty, null);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000295A File Offset: 0x00000B5A
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format);
		}

		// Token: 0x04000059 RID: 89
		private readonly string _qualifiedId;

		// Token: 0x0400005A RID: 90
		private readonly string _fallback;
	}
}
