using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E4 RID: 228
	public static class SaveDataHelper
	{
		// Token: 0x060008F1 RID: 2289 RVA: 0x0001A254 File Offset: 0x00018454
		internal static string DecodeYaml(byte[] bytes)
		{
			if (Enumerable.SequenceEqual<byte>(Enumerable.Take<byte>(bytes, SaveDataHelper.Flag.Length), SaveDataHelper.Flag))
			{
				byte[] array = Enumerable.ToArray<byte>(Enumerable.Take<byte>(Enumerable.Skip<byte>(bytes, SaveDataHelper.Flag.Length), 4));
				if (!BitConverter.IsLittleEndian)
				{
					Array.Reverse<byte>(array);
				}
				int num = BitConverter.ToInt32(array, 0);
				bytes = Enumerable.ToArray<byte>(Lzss.Decode(Enumerable.Skip<byte>(bytes, SaveDataHelper.Flag.Length + 4)));
				if (bytes.Length != num)
				{
					throw new ArgumentException("Decode bytes failed: mismatched size.");
				}
			}
			return SaveDataHelper.Utf8.GetString(bytes);
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0001A2E0 File Offset: 0x000184E0
		internal static byte[] EncodeYaml(string text, bool compress)
		{
			byte[] array = SaveDataHelper.Utf8.GetBytes(text);
			if (compress)
			{
				byte[] bytes = BitConverter.GetBytes(array.Length);
				if (!BitConverter.IsLittleEndian)
				{
					Array.Reverse<byte>(bytes);
				}
				array = Enumerable.ToArray<byte>(Enumerable.Concat<byte>(Enumerable.Concat<byte>(SaveDataHelper.Flag, bytes), Lzss.Encode(array)));
			}
			return array;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x0001A32F File Offset: 0x0001852F
		private static T Deserialize<T>(byte[] bytes)
		{
			return new DeserializerBuilder().IgnoreUnmatchedProperties().Build().Deserialize<T>(SaveDataHelper.DecodeYaml(bytes));
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x0001A34C File Offset: 0x0001854C
		private static byte[] Serialize<T>(T saveData, bool compress)
		{
			byte[] array;
			using (StringWriter stringWriter = new StringWriter
			{
				NewLine = "\n"
			})
			{
				new SerializerBuilder().DisableAliases().Build().Serialize(stringWriter, saveData);
				array = SaveDataHelper.EncodeYaml(stringWriter.ToString(), compress);
			}
			return array;
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x0001A3B0 File Offset: 0x000185B0
		public static SysSaveData DeserializeSys([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<SysSaveData>(bytes);
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0001A3B8 File Offset: 0x000185B8
		public static byte[] SerializeSys([NotNull] SysSaveData data, bool compress)
		{
			return SaveDataHelper.Serialize<SysSaveData>(data, compress);
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x0001A3C1 File Offset: 0x000185C1
		public static ProfileSaveData DeserializeProfile([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<ProfileSaveData>(bytes);
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x0001A3C9 File Offset: 0x000185C9
		public static byte[] SerializeProfile([NotNull] ProfileSaveData data, bool compress)
		{
			return SaveDataHelper.Serialize<ProfileSaveData>(data, compress);
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x0001A3D2 File Offset: 0x000185D2
		public static GameRunSaveData DeserializeGameRun([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<GameRunSaveData>(bytes);
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x0001A3DA File Offset: 0x000185DA
		public static byte[] SerializeGameRun([NotNull] GameRunSaveData gameRun, bool compress)
		{
			return SaveDataHelper.Serialize<GameRunSaveData>(gameRun, compress);
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x0001A3E3 File Offset: 0x000185E3
		public static List<GameRunRecordSaveData> DeserializeGameRunHistory([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<List<GameRunRecordSaveData>>(bytes);
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0001A3EB File Offset: 0x000185EB
		public static byte[] SerializeGameRunHistory(IEnumerable<GameRunRecordSaveData> records, bool compress)
		{
			return SaveDataHelper.Serialize<IEnumerable<GameRunRecordSaveData>>(records, compress);
		}

		// Token: 0x04000493 RID: 1171
		private static readonly byte[] Flag = new byte[] { 76, 66, 79, 76, 83, 65, 86, 69 };

		// Token: 0x04000494 RID: 1172
		private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
	}
}
