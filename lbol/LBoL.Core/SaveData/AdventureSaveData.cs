using System;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000CE RID: 206
	public sealed class AdventureSaveData
	{
		// Token: 0x040003CE RID: 974
		public string AdventureId;

		// Token: 0x040003CF RID: 975
		public string NodeName;

		// Token: 0x040003D0 RID: 976
		public string StorageYaml;

		// Token: 0x040003D1 RID: 977
		public AdvSlotSaveData[] Slots;
	}
}
