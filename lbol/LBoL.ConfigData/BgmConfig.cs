using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class BgmConfig
	{
		private BgmConfig(string ID, int No, string Name, string Folder, string Path, float Volume, float? LoopStart, float? LoopEnd, float? ExtraDelay, string TrackName, string Artist, string Original, string Comment)
		{
			this.ID = ID;
			this.No = No;
			this.Name = Name;
			this.Folder = Folder;
			this.Path = Path;
			this.Volume = Volume;
			this.LoopStart = LoopStart;
			this.LoopEnd = LoopEnd;
			this.ExtraDelay = ExtraDelay;
			this.TrackName = TrackName;
			this.Artist = Artist;
			this.Original = Original;
			this.Comment = Comment;
		}
		public string ID { get; private set; }
		public int No { get; private set; }
		public string Name { get; private set; }
		public string Folder { get; private set; }
		public string Path { get; private set; }
		public float Volume { get; private set; }
		public float? LoopStart { get; private set; }
		public float? LoopEnd { get; private set; }
		public float? ExtraDelay { get; private set; }
		public string TrackName { get; private set; }
		public string Artist { get; private set; }
		public string Original { get; private set; }
		public string Comment { get; private set; }
		public static IReadOnlyList<BgmConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<BgmConfig>(BgmConfig._data);
		}
		public static BgmConfig FromID(string ID)
		{
			ConfigDataManager.Initialize();
			BgmConfig bgmConfig;
			return (!BgmConfig._IDTable.TryGetValue(ID, out bgmConfig)) ? null : bgmConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{BgmConfig ID=",
				ConfigDataManager.System_String.ToString(this.ID),
				", No=",
				ConfigDataManager.System_Int32.ToString(this.No),
				", Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Folder=",
				ConfigDataManager.System_String.ToString(this.Folder),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Volume=",
				ConfigDataManager.System_Single.ToString(this.Volume),
				", LoopStart=",
				(this.LoopStart == null) ? "null" : ConfigDataManager.System_Single.ToString(this.LoopStart.Value),
				", LoopEnd=",
				(this.LoopEnd == null) ? "null" : ConfigDataManager.System_Single.ToString(this.LoopEnd.Value),
				", ExtraDelay=",
				(this.ExtraDelay == null) ? "null" : ConfigDataManager.System_Single.ToString(this.ExtraDelay.Value),
				", TrackName=",
				ConfigDataManager.System_String.ToString(this.TrackName),
				", Artist=",
				ConfigDataManager.System_String.ToString(this.Artist),
				", Original=",
				ConfigDataManager.System_String.ToString(this.Original),
				", Comment=",
				ConfigDataManager.System_String.ToString(this.Comment),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				BgmConfig[] array = new BgmConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new BgmConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				BgmConfig._data = array;
				BgmConfig._IDTable = Enumerable.ToDictionary<BgmConfig, string>(BgmConfig._data, (BgmConfig elem) => elem.ID);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/BgmConfig");
			if (textAsset != null)
			{
				try
				{
					BgmConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load BgmConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'BgmConfig', please reimport config data");
			}
		}
		private static BgmConfig[] _data = Array.Empty<BgmConfig>();
		private static Dictionary<string, BgmConfig> _IDTable = Enumerable.ToDictionary<BgmConfig, string>(BgmConfig._data, (BgmConfig elem) => elem.ID);
	}
}
