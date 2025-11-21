using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000019 RID: 25
	public sealed class ExpConfig
	{
		// Token: 0x06000450 RID: 1104 RVA: 0x0000D627 File Offset: 0x0000B827
		private ExpConfig(int Level, int Exp, IReadOnlyList<string> UnlockExhibits, IReadOnlyList<string> UnlockCards)
		{
			this.Level = Level;
			this.Exp = Exp;
			this.UnlockExhibits = UnlockExhibits;
			this.UnlockCards = UnlockCards;
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x0000D64C File Offset: 0x0000B84C
		// (set) Token: 0x06000452 RID: 1106 RVA: 0x0000D654 File Offset: 0x0000B854
		public int Level { get; private set; }

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000453 RID: 1107 RVA: 0x0000D65D File Offset: 0x0000B85D
		// (set) Token: 0x06000454 RID: 1108 RVA: 0x0000D665 File Offset: 0x0000B865
		public int Exp { get; private set; }

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000455 RID: 1109 RVA: 0x0000D66E File Offset: 0x0000B86E
		// (set) Token: 0x06000456 RID: 1110 RVA: 0x0000D676 File Offset: 0x0000B876
		public IReadOnlyList<string> UnlockExhibits { get; private set; }

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x0000D67F File Offset: 0x0000B87F
		// (set) Token: 0x06000458 RID: 1112 RVA: 0x0000D687 File Offset: 0x0000B887
		public IReadOnlyList<string> UnlockCards { get; private set; }

		// Token: 0x06000459 RID: 1113 RVA: 0x0000D690 File Offset: 0x0000B890
		public static IReadOnlyList<ExpConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<ExpConfig>(ExpConfig._data);
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x0000D6A4 File Offset: 0x0000B8A4
		public static ExpConfig FromLevel(int Level)
		{
			ConfigDataManager.Initialize();
			ExpConfig expConfig;
			return (!ExpConfig._LevelTable.TryGetValue(Level, out expConfig)) ? null : expConfig;
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x0000D6D0 File Offset: 0x0000B8D0
		public override string ToString()
		{
			string[] array = new string[9];
			array[0] = "{ExpConfig Level=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Level);
			array[2] = ", Exp=";
			array[3] = ConfigDataManager.System_Int32.ToString(this.Exp);
			array[4] = ", UnlockExhibits=[";
			array[5] = string.Join(", ", Enumerable.Select<string, string>(this.UnlockExhibits, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[6] = "], UnlockCards=[";
			array[7] = string.Join(", ", Enumerable.Select<string, string>(this.UnlockCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[8] = "]}";
			return string.Concat(array);
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x0000D7A4 File Offset: 0x0000B9A4
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				ExpConfig[] array = new ExpConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new ExpConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				ExpConfig._data = array;
				ExpConfig._LevelTable = Enumerable.ToDictionary<ExpConfig, int>(ExpConfig._data, (ExpConfig elem) => elem.Level);
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0000D898 File Offset: 0x0000BA98
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/ExpConfig");
			if (textAsset != null)
			{
				try
				{
					ExpConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load ExpConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'ExpConfig', please reimport config data");
			}
		}

		// Token: 0x0400024F RID: 591
		private static ExpConfig[] _data = Array.Empty<ExpConfig>();

		// Token: 0x04000250 RID: 592
		private static Dictionary<int, ExpConfig> _LevelTable = Enumerable.ToDictionary<ExpConfig, int>(ExpConfig._data, (ExpConfig elem) => elem.Level);
	}
}
