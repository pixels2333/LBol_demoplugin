using System;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000DB RID: 219
	public sealed class JadeBoxSaveData
	{
		// Token: 0x0400041E RID: 1054
		public string Name;

		// Token: 0x0400041F RID: 1055
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? Counter;
	}
}
