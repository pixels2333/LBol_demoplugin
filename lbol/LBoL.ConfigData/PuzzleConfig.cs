using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200001D RID: 29
	public sealed class PuzzleConfig
	{
		// Token: 0x06000496 RID: 1174 RVA: 0x0000E193 File Offset: 0x0000C393
		private PuzzleConfig(string Id, int UnlockLevel)
		{
			this.Id = Id;
			this.UnlockLevel = UnlockLevel;
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000497 RID: 1175 RVA: 0x0000E1A9 File Offset: 0x0000C3A9
		// (set) Token: 0x06000498 RID: 1176 RVA: 0x0000E1B1 File Offset: 0x0000C3B1
		public string Id { get; private set; }

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000499 RID: 1177 RVA: 0x0000E1BA File Offset: 0x0000C3BA
		// (set) Token: 0x0600049A RID: 1178 RVA: 0x0000E1C2 File Offset: 0x0000C3C2
		public int UnlockLevel { get; private set; }

		// Token: 0x0600049B RID: 1179 RVA: 0x0000E1CB File Offset: 0x0000C3CB
		public static IReadOnlyList<PuzzleConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<PuzzleConfig>(PuzzleConfig._data);
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x0000E1DC File Offset: 0x0000C3DC
		public static PuzzleConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			PuzzleConfig puzzleConfig;
			return (!PuzzleConfig._IdTable.TryGetValue(Id, out puzzleConfig)) ? null : puzzleConfig;
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x0000E208 File Offset: 0x0000C408
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{PuzzleConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", UnlockLevel=",
				ConfigDataManager.System_Int32.ToString(this.UnlockLevel),
				"}"
			});
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0000E260 File Offset: 0x0000C460
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				PuzzleConfig[] array = new PuzzleConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new PuzzleConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader));
				}
				PuzzleConfig._data = array;
				PuzzleConfig._IdTable = Enumerable.ToDictionary<PuzzleConfig, string>(PuzzleConfig._data, (PuzzleConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0000E30C File Offset: 0x0000C50C
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/PuzzleConfig");
			if (textAsset != null)
			{
				try
				{
					PuzzleConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load PuzzleConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'PuzzleConfig', please reimport config data");
			}
		}

		// Token: 0x0400026E RID: 622
		private static PuzzleConfig[] _data = Array.Empty<PuzzleConfig>();

		// Token: 0x0400026F RID: 623
		private static Dictionary<string, PuzzleConfig> _IdTable = Enumerable.ToDictionary<PuzzleConfig, string>(PuzzleConfig._data, (PuzzleConfig elem) => elem.Id);
	}
}
