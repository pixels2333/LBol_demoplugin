using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class ExpConfig
	{
		private ExpConfig(int Level, int Exp, IReadOnlyList<string> UnlockExhibits, IReadOnlyList<string> UnlockCards)
		{
			this.Level = Level;
			this.Exp = Exp;
			this.UnlockExhibits = UnlockExhibits;
			this.UnlockCards = UnlockCards;
		}
		public int Level { get; private set; }
		public int Exp { get; private set; }
		public IReadOnlyList<string> UnlockExhibits { get; private set; }
		public IReadOnlyList<string> UnlockCards { get; private set; }
		public static IReadOnlyList<ExpConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<ExpConfig>(ExpConfig._data);
		}
		public static ExpConfig FromLevel(int Level)
		{
			ConfigDataManager.Initialize();
			ExpConfig expConfig;
			return (!ExpConfig._LevelTable.TryGetValue(Level, out expConfig)) ? null : expConfig;
		}
		public override string ToString()
		{
			string[] array = new string[9];
			array[0] = "{ExpConfig Level=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Level);
			array[2] = ", Exp=";
			array[3] = ConfigDataManager.System_Int32.ToString(this.Exp);
			array[4] = ", UnlockExhibits=[";
			array[5] = string.Join(", ", Enumerable.Select<string, string>(this.UnlockExhibits, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[6] = "], UnlockCards=[";
			array[7] = string.Join(", ", Enumerable.Select<string, string>(this.UnlockCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[8] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				ExpConfig[] array = new ExpConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new ExpConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				ExpConfig._data = array;
				ExpConfig._LevelTable = Enumerable.ToDictionary<ExpConfig, int>(ExpConfig._data, (ExpConfig elem) => elem.Level);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/ExpConfig");
			if (textAsset != null)
			{
				try
				{
					ExpConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load ExpConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'ExpConfig', please reimport config data");
			}
		}
		private static ExpConfig[] _data = Array.Empty<ExpConfig>();
		private static Dictionary<int, ExpConfig> _LevelTable = Enumerable.ToDictionary<ExpConfig, int>(ExpConfig._data, (ExpConfig elem) => elem.Level);
	}
}
