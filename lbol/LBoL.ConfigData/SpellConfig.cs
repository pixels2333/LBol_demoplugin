using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class SpellConfig
	{
		private SpellConfig(string Id, string Resource)
		{
			this.Id = Id;
			this.Resource = Resource;
		}
		public string Id { get; private set; }
		public string Resource { get; private set; }
		public static IReadOnlyList<SpellConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SpellConfig>(SpellConfig._data);
		}
		public static SpellConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			SpellConfig spellConfig;
			return (!SpellConfig._IdTable.TryGetValue(Id, out spellConfig)) ? null : spellConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{SpellConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", Resource=",
				ConfigDataManager.System_String.ToString(this.Resource),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SpellConfig[] array = new SpellConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SpellConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				SpellConfig._data = array;
				SpellConfig._IdTable = Enumerable.ToDictionary<SpellConfig, string>(SpellConfig._data, (SpellConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SpellConfig");
			if (textAsset != null)
			{
				try
				{
					SpellConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SpellConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SpellConfig', please reimport config data");
			}
		}
		private static SpellConfig[] _data = Array.Empty<SpellConfig>();
		private static Dictionary<string, SpellConfig> _IdTable = Enumerable.ToDictionary<SpellConfig, string>(SpellConfig._data, (SpellConfig elem) => elem.Id);
	}
}
