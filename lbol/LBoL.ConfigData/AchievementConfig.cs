using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class AchievementConfig
	{
		private AchievementConfig(int Order, string Id, bool Hidden)
		{
			this.Order = Order;
			this.Id = Id;
			this.Hidden = Hidden;
		}
		public int Order { get; private set; }
		public string Id { get; private set; }
		public bool Hidden { get; private set; }
		public static IReadOnlyList<AchievementConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<AchievementConfig>(AchievementConfig._data);
		}
		public static AchievementConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			AchievementConfig achievementConfig;
			return (!AchievementConfig._IdTable.TryGetValue(Id, out achievementConfig)) ? null : achievementConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{AchievementConfig Order=",
				ConfigDataManager.System_Int32.ToString(this.Order),
				", Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", Hidden=",
				ConfigDataManager.System_Boolean.ToString(this.Hidden),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				AchievementConfig[] array = new AchievementConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new AchievementConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader));
				}
				AchievementConfig._data = array;
				AchievementConfig._IdTable = Enumerable.ToDictionary<AchievementConfig, string>(AchievementConfig._data, (AchievementConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/AchievementConfig");
			if (textAsset != null)
			{
				try
				{
					AchievementConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load AchievementConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'AchievementConfig', please reimport config data");
			}
		}
		private static AchievementConfig[] _data = Array.Empty<AchievementConfig>();
		private static Dictionary<string, AchievementConfig> _IdTable = Enumerable.ToDictionary<AchievementConfig, string>(AchievementConfig._data, (AchievementConfig elem) => elem.Id);
	}
}
