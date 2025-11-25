using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class AdventureConfig
	{
		private AdventureConfig(int No, string Id, string HostId, string HostId2, int Music, bool HideUlt, bool TempArt)
		{
			this.No = No;
			this.Id = Id;
			this.HostId = HostId;
			this.HostId2 = HostId2;
			this.Music = Music;
			this.HideUlt = HideUlt;
			this.TempArt = TempArt;
		}
		public int No { get; private set; }
		public string Id { get; private set; }
		public string HostId { get; private set; }
		public string HostId2 { get; private set; }
		public int Music { get; private set; }
		public bool HideUlt { get; private set; }
		public bool TempArt { get; private set; }
		public static IReadOnlyList<AdventureConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<AdventureConfig>(AdventureConfig._data);
		}
		public static AdventureConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			AdventureConfig adventureConfig;
			return (!AdventureConfig._IdTable.TryGetValue(Id, out adventureConfig)) ? null : adventureConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{AdventureConfig No=",
				ConfigDataManager.System_Int32.ToString(this.No),
				", Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", HostId=",
				ConfigDataManager.System_String.ToString(this.HostId),
				", HostId2=",
				ConfigDataManager.System_String.ToString(this.HostId2),
				", Music=",
				ConfigDataManager.System_Int32.ToString(this.Music),
				", HideUlt=",
				ConfigDataManager.System_Boolean.ToString(this.HideUlt),
				", TempArt=",
				ConfigDataManager.System_Boolean.ToString(this.TempArt),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				AdventureConfig[] array = new AdventureConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new AdventureConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader));
				}
				AdventureConfig._data = array;
				AdventureConfig._IdTable = Enumerable.ToDictionary<AdventureConfig, string>(AdventureConfig._data, (AdventureConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/AdventureConfig");
			if (textAsset != null)
			{
				try
				{
					AdventureConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load AdventureConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'AdventureConfig', please reimport config data");
			}
		}
		private static AdventureConfig[] _data = Array.Empty<AdventureConfig>();
		private static Dictionary<string, AdventureConfig> _IdTable = Enumerable.ToDictionary<AdventureConfig, string>(AdventureConfig._data, (AdventureConfig elem) => elem.Id);
	}
}
