using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public static class SaveDataHelper
	{
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
		private static T Deserialize<T>(byte[] bytes)
		{
			return new DeserializerBuilder().IgnoreUnmatchedProperties().Build().Deserialize<T>(SaveDataHelper.DecodeYaml(bytes));
		}
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
		public static SysSaveData DeserializeSys([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<SysSaveData>(bytes);
		}
		public static byte[] SerializeSys([NotNull] SysSaveData data, bool compress)
		{
			return SaveDataHelper.Serialize<SysSaveData>(data, compress);
		}
		public static ProfileSaveData DeserializeProfile([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<ProfileSaveData>(bytes);
		}
		public static byte[] SerializeProfile([NotNull] ProfileSaveData data, bool compress)
		{
			return SaveDataHelper.Serialize<ProfileSaveData>(data, compress);
		}
		public static GameRunSaveData DeserializeGameRun([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<GameRunSaveData>(bytes);
		}
		public static byte[] SerializeGameRun([NotNull] GameRunSaveData gameRun, bool compress)
		{
			return SaveDataHelper.Serialize<GameRunSaveData>(gameRun, compress);
		}
		public static List<GameRunRecordSaveData> DeserializeGameRunHistory([NotNull] byte[] bytes)
		{
			return SaveDataHelper.Deserialize<List<GameRunRecordSaveData>>(bytes);
		}
		public static byte[] SerializeGameRunHistory(IEnumerable<GameRunRecordSaveData> records, bool compress)
		{
			return SaveDataHelper.Serialize<IEnumerable<GameRunRecordSaveData>>(records, compress);
		}
		private static readonly byte[] Flag = new byte[] { 76, 66, 79, 76, 83, 65, 86, 69 };
		private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
	}
}
