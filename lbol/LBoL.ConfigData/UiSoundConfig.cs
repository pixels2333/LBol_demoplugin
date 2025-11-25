using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class UiSoundConfig
	{
		private UiSoundConfig(string Name, string Folder, string Path, float Volume)
		{
			this.Name = Name;
			this.Folder = Folder;
			this.Path = Path;
			this.Volume = Volume;
		}
		public string Name { get; private set; }
		public string Folder { get; private set; }
		public string Path { get; private set; }
		public float Volume { get; private set; }
		public static IReadOnlyList<UiSoundConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<UiSoundConfig>(UiSoundConfig._data);
		}
		public static UiSoundConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			UiSoundConfig uiSoundConfig;
			return (!UiSoundConfig._NameTable.TryGetValue(Name, out uiSoundConfig)) ? null : uiSoundConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{UiSoundConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Folder=",
				ConfigDataManager.System_String.ToString(this.Folder),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Volume=",
				ConfigDataManager.System_Single.ToString(this.Volume),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				UiSoundConfig[] array = new UiSoundConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new UiSoundConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				UiSoundConfig._data = array;
				UiSoundConfig._NameTable = Enumerable.ToDictionary<UiSoundConfig, string>(UiSoundConfig._data, (UiSoundConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/UiSoundConfig");
			if (textAsset != null)
			{
				try
				{
					UiSoundConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load UiSoundConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'UiSoundConfig', please reimport config data");
			}
		}
		private static UiSoundConfig[] _data = Array.Empty<UiSoundConfig>();
		private static Dictionary<string, UiSoundConfig> _NameTable = Enumerable.ToDictionary<UiSoundConfig, string>(UiSoundConfig._data, (UiSoundConfig elem) => elem.Name);
	}
}
