using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000017 RID: 23
	public sealed class StageConfig
	{
		// Token: 0x0600041C RID: 1052 RVA: 0x0000CE90 File Offset: 0x0000B090
		private StageConfig(string Id, string Obj0, string Obj1, int Level1, string Obj2, int Level2, string Obj3, int Level3, string Obj4, int Level4)
		{
			this.Id = Id;
			this.Obj0 = Obj0;
			this.Obj1 = Obj1;
			this.Level1 = Level1;
			this.Obj2 = Obj2;
			this.Level2 = Level2;
			this.Obj3 = Obj3;
			this.Level3 = Level3;
			this.Obj4 = Obj4;
			this.Level4 = Level4;
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x0600041D RID: 1053 RVA: 0x0000CEF0 File Offset: 0x0000B0F0
		// (set) Token: 0x0600041E RID: 1054 RVA: 0x0000CEF8 File Offset: 0x0000B0F8
		public string Id { get; private set; }

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x0600041F RID: 1055 RVA: 0x0000CF01 File Offset: 0x0000B101
		// (set) Token: 0x06000420 RID: 1056 RVA: 0x0000CF09 File Offset: 0x0000B109
		public string Obj0 { get; private set; }

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000421 RID: 1057 RVA: 0x0000CF12 File Offset: 0x0000B112
		// (set) Token: 0x06000422 RID: 1058 RVA: 0x0000CF1A File Offset: 0x0000B11A
		public string Obj1 { get; private set; }

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000423 RID: 1059 RVA: 0x0000CF23 File Offset: 0x0000B123
		// (set) Token: 0x06000424 RID: 1060 RVA: 0x0000CF2B File Offset: 0x0000B12B
		public int Level1 { get; private set; }

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000425 RID: 1061 RVA: 0x0000CF34 File Offset: 0x0000B134
		// (set) Token: 0x06000426 RID: 1062 RVA: 0x0000CF3C File Offset: 0x0000B13C
		public string Obj2 { get; private set; }

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x0000CF45 File Offset: 0x0000B145
		// (set) Token: 0x06000428 RID: 1064 RVA: 0x0000CF4D File Offset: 0x0000B14D
		public int Level2 { get; private set; }

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000429 RID: 1065 RVA: 0x0000CF56 File Offset: 0x0000B156
		// (set) Token: 0x0600042A RID: 1066 RVA: 0x0000CF5E File Offset: 0x0000B15E
		public string Obj3 { get; private set; }

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x0000CF67 File Offset: 0x0000B167
		// (set) Token: 0x0600042C RID: 1068 RVA: 0x0000CF6F File Offset: 0x0000B16F
		public int Level3 { get; private set; }

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x0600042D RID: 1069 RVA: 0x0000CF78 File Offset: 0x0000B178
		// (set) Token: 0x0600042E RID: 1070 RVA: 0x0000CF80 File Offset: 0x0000B180
		public string Obj4 { get; private set; }

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x0600042F RID: 1071 RVA: 0x0000CF89 File Offset: 0x0000B189
		// (set) Token: 0x06000430 RID: 1072 RVA: 0x0000CF91 File Offset: 0x0000B191
		public int Level4 { get; private set; }

		// Token: 0x06000431 RID: 1073 RVA: 0x0000CF9A File Offset: 0x0000B19A
		public static IReadOnlyList<StageConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<StageConfig>(StageConfig._data);
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x0000CFAC File Offset: 0x0000B1AC
		public static StageConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			StageConfig stageConfig;
			return (!StageConfig._IdTable.TryGetValue(Id, out stageConfig)) ? null : stageConfig;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x0000CFD8 File Offset: 0x0000B1D8
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{StageConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", Obj0=",
				ConfigDataManager.System_String.ToString(this.Obj0),
				", Obj1=",
				ConfigDataManager.System_String.ToString(this.Obj1),
				", Level1=",
				ConfigDataManager.System_Int32.ToString(this.Level1),
				", Obj2=",
				ConfigDataManager.System_String.ToString(this.Obj2),
				", Level2=",
				ConfigDataManager.System_Int32.ToString(this.Level2),
				", Obj3=",
				ConfigDataManager.System_String.ToString(this.Obj3),
				", Level3=",
				ConfigDataManager.System_Int32.ToString(this.Level3),
				", Obj4=",
				ConfigDataManager.System_String.ToString(this.Obj4),
				", Level4=",
				ConfigDataManager.System_Int32.ToString(this.Level4),
				"}"
			});
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x0000D114 File Offset: 0x0000B314
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				StageConfig[] array = new StageConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new StageConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader));
				}
				StageConfig._data = array;
				StageConfig._IdTable = Enumerable.ToDictionary<StageConfig, string>(StageConfig._data, (StageConfig elem) => elem.Id);
			}
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0000D218 File Offset: 0x0000B418
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/StageConfig");
			if (textAsset != null)
			{
				try
				{
					StageConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load StageConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'StageConfig', please reimport config data");
			}
		}

		// Token: 0x0400023E RID: 574
		private static StageConfig[] _data = Array.Empty<StageConfig>();

		// Token: 0x0400023F RID: 575
		private static Dictionary<string, StageConfig> _IdTable = Enumerable.ToDictionary<StageConfig, string>(StageConfig._data, (StageConfig elem) => elem.Id);
	}
}
