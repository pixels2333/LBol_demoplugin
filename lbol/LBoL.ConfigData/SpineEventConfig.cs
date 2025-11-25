using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class SpineEventConfig
	{
		private SpineEventConfig(string Name, string Sfx, string Effect)
		{
			this.Name = Name;
			this.Sfx = Sfx;
			this.Effect = Effect;
		}
		public string Name { get; private set; }
		public string Sfx { get; private set; }
		public string Effect { get; private set; }
		public static IReadOnlyList<SpineEventConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SpineEventConfig>(SpineEventConfig._data);
		}
		public static SpineEventConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			SpineEventConfig spineEventConfig;
			return (!SpineEventConfig._NameTable.TryGetValue(Name, out spineEventConfig)) ? null : spineEventConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{SpineEventConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Sfx=",
				ConfigDataManager.System_String.ToString(this.Sfx),
				", Effect=",
				ConfigDataManager.System_String.ToString(this.Effect),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SpineEventConfig[] array = new SpineEventConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SpineEventConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				SpineEventConfig._data = array;
				SpineEventConfig._NameTable = Enumerable.ToDictionary<SpineEventConfig, string>(SpineEventConfig._data, (SpineEventConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SpineEventConfig");
			if (textAsset != null)
			{
				try
				{
					SpineEventConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SpineEventConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SpineEventConfig', please reimport config data");
			}
		}
		private static SpineEventConfig[] _data = Array.Empty<SpineEventConfig>();
		private static Dictionary<string, SpineEventConfig> _NameTable = Enumerable.ToDictionary<SpineEventConfig, string>(SpineEventConfig._data, (SpineEventConfig elem) => elem.Name);
	}
}
