using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class SfxConfig
	{
		private SfxConfig(string Name, string Folder, string Path, double Rep, float Volume)
		{
			this.Name = Name;
			this.Folder = Folder;
			this.Path = Path;
			this.Rep = Rep;
			this.Volume = Volume;
		}
		public string Name { get; private set; }
		public string Folder { get; private set; }
		public string Path { get; private set; }
		public double Rep { get; private set; }
		public float Volume { get; private set; }
		public static IReadOnlyList<SfxConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SfxConfig>(SfxConfig._data);
		}
		public static SfxConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			SfxConfig sfxConfig;
			return (!SfxConfig._NameTable.TryGetValue(Name, out sfxConfig)) ? null : sfxConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{SfxConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Folder=",
				ConfigDataManager.System_String.ToString(this.Folder),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Rep=",
				ConfigDataManager.System_Double.ToString(this.Rep),
				", Volume=",
				ConfigDataManager.System_Single.ToString(this.Volume),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SfxConfig[] array = new SfxConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SfxConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Double.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				SfxConfig._data = array;
				SfxConfig._NameTable = Enumerable.ToDictionary<SfxConfig, string>(SfxConfig._data, (SfxConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SfxConfig");
			if (textAsset != null)
			{
				try
				{
					SfxConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SfxConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SfxConfig', please reimport config data");
			}
		}
		private static SfxConfig[] _data = Array.Empty<SfxConfig>();
		private static Dictionary<string, SfxConfig> _NameTable = Enumerable.ToDictionary<SfxConfig, string>(SfxConfig._data, (SfxConfig elem) => elem.Name);
	}
}
