using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class SequenceConfig
	{
		private SequenceConfig(string Id, string Animation, IReadOnlyList<float> KeyTime, IReadOnlyList<float> Speed, float StartTime)
		{
			this.Id = Id;
			this.Animation = Animation;
			this.KeyTime = KeyTime;
			this.Speed = Speed;
			this.StartTime = StartTime;
		}
		public string Id { get; private set; }
		public string Animation { get; private set; }
		public IReadOnlyList<float> KeyTime { get; private set; }
		public IReadOnlyList<float> Speed { get; private set; }
		public float StartTime { get; private set; }
		public static IReadOnlyList<SequenceConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SequenceConfig>(SequenceConfig._data);
		}
		public static SequenceConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			SequenceConfig sequenceConfig;
			return (!SequenceConfig._IdTable.TryGetValue(Id, out sequenceConfig)) ? null : sequenceConfig;
		}
		public override string ToString()
		{
			string[] array = new string[11];
			array[0] = "{SequenceConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", Animation=";
			array[3] = ConfigDataManager.System_String.ToString(this.Animation);
			array[4] = ", KeyTime=[";
			array[5] = string.Join(", ", Enumerable.Select<float, string>(this.KeyTime, (float v1) => ConfigDataManager.System_Single.ToString(v1)));
			array[6] = "], Speed=[";
			array[7] = string.Join(", ", Enumerable.Select<float, string>(this.Speed, (float v1) => ConfigDataManager.System_Single.ToString(v1)));
			array[8] = "], StartTime=";
			array[9] = ConfigDataManager.System_Single.ToString(this.StartTime);
			array[10] = "}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SequenceConfig[] array = new SequenceConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SequenceConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<float>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1)), ConfigDataManager.ReadList<float>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1)), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				SequenceConfig._data = array;
				SequenceConfig._IdTable = Enumerable.ToDictionary<SequenceConfig, string>(SequenceConfig._data, (SequenceConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SequenceConfig");
			if (textAsset != null)
			{
				try
				{
					SequenceConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SequenceConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SequenceConfig', please reimport config data");
			}
		}
		private static SequenceConfig[] _data = Array.Empty<SequenceConfig>();
		private static Dictionary<string, SequenceConfig> _IdTable = Enumerable.ToDictionary<SequenceConfig, string>(SequenceConfig._data, (SequenceConfig elem) => elem.Id);
	}
}
