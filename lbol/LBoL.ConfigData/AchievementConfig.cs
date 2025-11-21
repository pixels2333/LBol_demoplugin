using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200001A RID: 26
	public sealed class AchievementConfig
	{
		// Token: 0x06000465 RID: 1125 RVA: 0x0000D977 File Offset: 0x0000BB77
		private AchievementConfig(int Order, string Id, bool Hidden)
		{
			this.Order = Order;
			this.Id = Id;
			this.Hidden = Hidden;
		}

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000466 RID: 1126 RVA: 0x0000D994 File Offset: 0x0000BB94
		// (set) Token: 0x06000467 RID: 1127 RVA: 0x0000D99C File Offset: 0x0000BB9C
		public int Order { get; private set; }

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000468 RID: 1128 RVA: 0x0000D9A5 File Offset: 0x0000BBA5
		// (set) Token: 0x06000469 RID: 1129 RVA: 0x0000D9AD File Offset: 0x0000BBAD
		public string Id { get; private set; }

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x0600046A RID: 1130 RVA: 0x0000D9B6 File Offset: 0x0000BBB6
		// (set) Token: 0x0600046B RID: 1131 RVA: 0x0000D9BE File Offset: 0x0000BBBE
		public bool Hidden { get; private set; }

		// Token: 0x0600046C RID: 1132 RVA: 0x0000D9C7 File Offset: 0x0000BBC7
		public static IReadOnlyList<AchievementConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<AchievementConfig>(AchievementConfig._data);
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0000D9D8 File Offset: 0x0000BBD8
		public static AchievementConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			AchievementConfig achievementConfig;
			return (!AchievementConfig._IdTable.TryGetValue(Id, out achievementConfig)) ? null : achievementConfig;
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x0000DA04 File Offset: 0x0000BC04
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{AchievementConfig Order=",
				ConfigDataManager.System_Int32.ToString(this.Order),
				", Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", Hidden=",
				ConfigDataManager.System_Boolean.ToString(this.Hidden),
				"}"
			});
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x0000DA78 File Offset: 0x0000BC78
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				AchievementConfig[] array = new AchievementConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new AchievementConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader));
				}
				AchievementConfig._data = array;
				AchievementConfig._IdTable = Enumerable.ToDictionary<AchievementConfig, string>(AchievementConfig._data, (AchievementConfig elem) => elem.Id);
			}
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x0000DB30 File Offset: 0x0000BD30
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/AchievementConfig");
			if (textAsset != null)
			{
				try
				{
					AchievementConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load AchievementConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'AchievementConfig', please reimport config data");
			}
		}

		// Token: 0x04000259 RID: 601
		private static AchievementConfig[] _data = Array.Empty<AchievementConfig>();

		// Token: 0x0400025A RID: 602
		private static Dictionary<string, AchievementConfig> _IdTable = Enumerable.ToDictionary<AchievementConfig, string>(AchievementConfig._data, (AchievementConfig elem) => elem.Id);
	}
}
