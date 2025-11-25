using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class StageConfig
	{
		private StageConfig(string Id, string Obj0, string Obj1, int Level1, string Obj2, int Level2, string Obj3, int Level3, string Obj4, int Level4)
		{
			this.Id = Id;
			this.Obj0 = Obj0;
			this.Obj1 = Obj1;
			this.Level1 = Level1;
			this.Obj2 = Obj2;
			this.Level2 = Level2;
			this.Obj3 = Obj3;
			this.Level3 = Level3;
			this.Obj4 = Obj4;
			this.Level4 = Level4;
		}
		public string Id { get; private set; }
		public string Obj0 { get; private set; }
		public string Obj1 { get; private set; }
		public int Level1 { get; private set; }
		public string Obj2 { get; private set; }
		public int Level2 { get; private set; }
		public string Obj3 { get; private set; }
		public int Level3 { get; private set; }
		public string Obj4 { get; private set; }
		public int Level4 { get; private set; }
		public static IReadOnlyList<StageConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<StageConfig>(StageConfig._data);
		}
		public static StageConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			StageConfig stageConfig;
			return (!StageConfig._IdTable.TryGetValue(Id, out stageConfig)) ? null : stageConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{StageConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", Obj0=",
				ConfigDataManager.System_String.ToString(this.Obj0),
				", Obj1=",
				ConfigDataManager.System_String.ToString(this.Obj1),
				", Level1=",
				ConfigDataManager.System_Int32.ToString(this.Level1),
				", Obj2=",
				ConfigDataManager.System_String.ToString(this.Obj2),
				", Level2=",
				ConfigDataManager.System_Int32.ToString(this.Level2),
				", Obj3=",
				ConfigDataManager.System_String.ToString(this.Obj3),
				", Level3=",
				ConfigDataManager.System_Int32.ToString(this.Level3),
				", Obj4=",
				ConfigDataManager.System_String.ToString(this.Obj4),
				", Level4=",
				ConfigDataManager.System_Int32.ToString(this.Level4),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				StageConfig[] array = new StageConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new StageConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader));
				}
				StageConfig._data = array;
				StageConfig._IdTable = Enumerable.ToDictionary<StageConfig, string>(StageConfig._data, (StageConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/StageConfig");
			if (textAsset != null)
			{
				try
				{
					StageConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load StageConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'StageConfig', please reimport config data");
			}
		}
		private static StageConfig[] _data = Array.Empty<StageConfig>();
		private static Dictionary<string, StageConfig> _IdTable = Enumerable.ToDictionary<StageConfig, string>(StageConfig._data, (StageConfig elem) => elem.Id);
	}
}
