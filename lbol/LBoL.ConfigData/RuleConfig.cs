using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class RuleConfig
	{
		private RuleConfig(int Order, string Id, IReadOnlyList<string> CardIds, IReadOnlyList<string> ExhibitIds)
		{
			this.Order = Order;
			this.Id = Id;
			this.CardIds = CardIds;
			this.ExhibitIds = ExhibitIds;
		}
		public int Order { get; private set; }
		public string Id { get; private set; }
		public IReadOnlyList<string> CardIds { get; private set; }
		public IReadOnlyList<string> ExhibitIds { get; private set; }
		public static IReadOnlyList<RuleConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<RuleConfig>(RuleConfig._data);
		}
		public static RuleConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			RuleConfig ruleConfig;
			return (!RuleConfig._IdTable.TryGetValue(Id, out ruleConfig)) ? null : ruleConfig;
		}
		public override string ToString()
		{
			string[] array = new string[9];
			array[0] = "{RuleConfig Order=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", CardIds=[";
			array[5] = string.Join(", ", Enumerable.Select<string, string>(this.CardIds, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[6] = "], ExhibitIds=[";
			array[7] = string.Join(", ", Enumerable.Select<string, string>(this.ExhibitIds, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[8] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				RuleConfig[] array = new RuleConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new RuleConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				RuleConfig._data = array;
				RuleConfig._IdTable = Enumerable.ToDictionary<RuleConfig, string>(RuleConfig._data, (RuleConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/RuleConfig");
			if (textAsset != null)
			{
				try
				{
					RuleConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load RuleConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'RuleConfig', please reimport config data");
			}
		}
		private static RuleConfig[] _data = Array.Empty<RuleConfig>();
		private static Dictionary<string, RuleConfig> _IdTable = Enumerable.ToDictionary<RuleConfig, string>(RuleConfig._data, (RuleConfig elem) => elem.Id);
	}
}
