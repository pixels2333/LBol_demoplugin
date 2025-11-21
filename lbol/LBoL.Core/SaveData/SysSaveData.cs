using System;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E8 RID: 232
	public sealed class SysSaveData
	{
		// Token: 0x040004AC RID: 1196
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? SaveIndex;

		// Token: 0x040004AD RID: 1197
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string Locale;
	}
}
