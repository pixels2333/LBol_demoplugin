using System;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D0 RID: 208
	public sealed class CardRecordSaveData
	{
		// Token: 0x040003D5 RID: 981
		public string Id;

		// Token: 0x040003D6 RID: 982
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool Upgraded;

		// Token: 0x040003D7 RID: 983
		public int? UpgradeCounter;
	}
}
