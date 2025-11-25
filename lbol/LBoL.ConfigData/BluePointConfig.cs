using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class BluePointConfig
	{
		private BluePointConfig(string Id, int? BluePoint)
		{
			this.Id = Id;
			this.BluePoint = BluePoint;
		}
		public string Id { get; private set; }
		public int? BluePoint { get; private set; }
		public static IReadOnlyList<BluePointConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<BluePointConfig>(BluePointConfig._data);
		}
		public static BluePointConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			BluePointConfig bluePointConfig;
			return (!BluePointConfig._IdTable.TryGetValue(Id, out bluePointConfig)) ? null : bluePointConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{BluePointConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", BluePoint=",
				(this.BluePoint == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.BluePoint.Value),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				BluePointConfig[] array = new BluePointConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new BluePointConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)));
				}
				BluePointConfig._data = array;
				BluePointConfig._IdTable = Enumerable.ToDictionary<BluePointConfig, string>(BluePointConfig._data, (BluePointConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/BluePointConfig");
			if (textAsset != null)
			{
				try
				{
					BluePointConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load BluePointConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'BluePointConfig', please reimport config data");
			}
		}
		private static BluePointConfig[] _data = Array.Empty<BluePointConfig>();
		private static Dictionary<string, BluePointConfig> _IdTable = Enumerable.ToDictionary<BluePointConfig, string>(BluePointConfig._data, (BluePointConfig elem) => elem.Id);
	}
}
