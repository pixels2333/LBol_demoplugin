using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D8 RID: 216
	public sealed class StageSaveData
	{
		// Token: 0x04000407 RID: 1031
		public string Name;

		// Token: 0x04000408 RID: 1032
		public int Index;

		// Token: 0x04000409 RID: 1033
		public ulong MapSeed;

		// Token: 0x0400040A RID: 1034
		public int Level;

		// Token: 0x0400040B RID: 1035
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public string SelectedBoss;

		// Token: 0x0400040C RID: 1036
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public string DebutAdventure;

		// Token: 0x0400040D RID: 1037
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool IsNormalFinalStage;

		// Token: 0x0400040E RID: 1038
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool IsTrueEndFinalStage;

		// Token: 0x0400040F RID: 1039
		public UniqueRandomPoolSaveData<string> AdventurePool;

		// Token: 0x04000410 RID: 1040
		public UniqueRandomPoolSaveData<string> EnemyPoolAct1;

		// Token: 0x04000411 RID: 1041
		public UniqueRandomPoolSaveData<string> EnemyPoolAct2;

		// Token: 0x04000412 RID: 1042
		public UniqueRandomPoolSaveData<string> EnemyPoolAct3;

		// Token: 0x04000413 RID: 1043
		public UniqueRandomPoolSaveData<string> EliteEnemyPool;

		// Token: 0x04000414 RID: 1044
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> AdventureHistory = new List<string>();

		// Token: 0x04000415 RID: 1045
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> ExtraFlags = new List<string>();
	}
}
