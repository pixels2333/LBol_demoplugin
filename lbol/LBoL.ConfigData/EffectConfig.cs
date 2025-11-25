using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class EffectConfig
	{
		private EffectConfig(string Name, string Path, float Life)
		{
			this.Name = Name;
			this.Path = Path;
			this.Life = Life;
		}
		public string Name { get; private set; }
		public string Path { get; private set; }
		public float Life { get; private set; }
		public static IReadOnlyList<EffectConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<EffectConfig>(EffectConfig._data);
		}
		public static EffectConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			EffectConfig effectConfig;
			return (!EffectConfig._NameTable.TryGetValue(Name, out effectConfig)) ? null : effectConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{EffectConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Life=",
				ConfigDataManager.System_Single.ToString(this.Life),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				EffectConfig[] array = new EffectConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new EffectConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				EffectConfig._data = array;
				EffectConfig._NameTable = Enumerable.ToDictionary<EffectConfig, string>(EffectConfig._data, (EffectConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/EffectConfig");
			if (textAsset != null)
			{
				try
				{
					EffectConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load EffectConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'EffectConfig', please reimport config data");
			}
		}
		private static EffectConfig[] _data = Array.Empty<EffectConfig>();
		private static Dictionary<string, EffectConfig> _NameTable = Enumerable.ToDictionary<EffectConfig, string>(EffectConfig._data, (EffectConfig elem) => elem.Name);
	}
}
