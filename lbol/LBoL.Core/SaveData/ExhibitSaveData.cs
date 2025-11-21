using System;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000DA RID: 218
	public sealed class ExhibitSaveData
	{
		// Token: 0x0400041B RID: 1051
		public string Name;

		// Token: 0x0400041C RID: 1052
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? Counter;

		// Token: 0x0400041D RID: 1053
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? CardInstanceId;
	}
}
