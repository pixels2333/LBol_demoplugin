using System;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E1 RID: 225
	[Serializable]
	public sealed class HintStatusSaveData
	{
		// Token: 0x0400047C RID: 1148
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[DefaultValue(false)]
		public bool BattleHintShown;

		// Token: 0x0400047D RID: 1149
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public HashSet<string> ShownHints = new HashSet<string>();
	}
}
