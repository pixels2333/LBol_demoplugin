using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class PuzzleConfig
	{
		private PuzzleConfig(string Id, int UnlockLevel)
		{
			this.Id = Id;
			this.UnlockLevel = UnlockLevel;
		}
		public string Id { get; private set; }
		public int UnlockLevel { get; private set; }
		public static IReadOnlyList<PuzzleConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<PuzzleConfig>(PuzzleConfig._data);
		}
		public static PuzzleConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			PuzzleConfig puzzleConfig;
			return (!PuzzleConfig._IdTable.TryGetValue(Id, out puzzleConfig)) ? null : puzzleConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{PuzzleConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", UnlockLevel=",
				ConfigDataManager.System_Int32.ToString(this.UnlockLevel),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				PuzzleConfig[] array = new PuzzleConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new PuzzleConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader));
				}
				PuzzleConfig._data = array;
				PuzzleConfig._IdTable = Enumerable.ToDictionary<PuzzleConfig, string>(PuzzleConfig._data, (PuzzleConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/PuzzleConfig");
			if (textAsset != null)
			{
				try
				{
					PuzzleConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load PuzzleConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'PuzzleConfig', please reimport config data");
			}
		}
		private static PuzzleConfig[] _data = Array.Empty<PuzzleConfig>();
		private static Dictionary<string, PuzzleConfig> _IdTable = Enumerable.ToDictionary<PuzzleConfig, string>(PuzzleConfig._data, (PuzzleConfig elem) => elem.Id);
	}
}
