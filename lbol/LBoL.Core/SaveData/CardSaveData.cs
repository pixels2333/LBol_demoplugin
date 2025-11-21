using System;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D9 RID: 217
	public sealed class CardSaveData
	{
		// Token: 0x04000416 RID: 1046
		public string Name;

		// Token: 0x04000417 RID: 1047
		public int InstanceId;

		// Token: 0x04000418 RID: 1048
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool IsUpgraded;

		// Token: 0x04000419 RID: 1049
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? DeckCounter;

		// Token: 0x0400041A RID: 1050
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? UpgradeCounter;
	}
}
