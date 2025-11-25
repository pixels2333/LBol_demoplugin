using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class UltimateSkillConfig
	{
		private UltimateSkillConfig(string Id, int Order, int PowerCost, int PowerPerLevel, int MaxPowerLevel, UsRepeatableType RepeatableType, int Damage, int Value1, int Value2, Keyword Keywords, IReadOnlyList<string> RelativeEffects, IReadOnlyList<string> RelativeCards)
		{
			this.Id = Id;
			this.Order = Order;
			this.PowerCost = PowerCost;
			this.PowerPerLevel = PowerPerLevel;
			this.MaxPowerLevel = MaxPowerLevel;
			this.RepeatableType = RepeatableType;
			this.Damage = Damage;
			this.Value1 = Value1;
			this.Value2 = Value2;
			this.Keywords = Keywords;
			this.RelativeEffects = RelativeEffects;
			this.RelativeCards = RelativeCards;
		}
		public string Id { get; private set; }
		public int Order { get; private set; }
		public int PowerCost { get; private set; }
		public int PowerPerLevel { get; private set; }
		public int MaxPowerLevel { get; private set; }
		public UsRepeatableType RepeatableType { get; private set; }
		public int Damage { get; private set; }
		public int Value1 { get; private set; }
		public int Value2 { get; private set; }
		public Keyword Keywords { get; private set; }
		public IReadOnlyList<string> RelativeEffects { get; private set; }
		public IReadOnlyList<string> RelativeCards { get; private set; }
		public static IReadOnlyList<UltimateSkillConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<UltimateSkillConfig>(UltimateSkillConfig._data);
		}
		public static UltimateSkillConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			UltimateSkillConfig ultimateSkillConfig;
			return (!UltimateSkillConfig._IdTable.TryGetValue(Id, out ultimateSkillConfig)) ? null : ultimateSkillConfig;
		}
		public override string ToString()
		{
			string[] array = new string[25];
			array[0] = "{UltimateSkillConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", Order=";
			array[3] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[4] = ", PowerCost=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.PowerCost);
			array[6] = ", PowerPerLevel=";
			array[7] = ConfigDataManager.System_Int32.ToString(this.PowerPerLevel);
			array[8] = ", MaxPowerLevel=";
			array[9] = ConfigDataManager.System_Int32.ToString(this.MaxPowerLevel);
			array[10] = ", RepeatableType=";
			array[11] = this.RepeatableType.ToString();
			array[12] = ", Damage=";
			array[13] = ConfigDataManager.System_Int32.ToString(this.Damage);
			array[14] = ", Value1=";
			array[15] = ConfigDataManager.System_Int32.ToString(this.Value1);
			array[16] = ", Value2=";
			array[17] = ConfigDataManager.System_Int32.ToString(this.Value2);
			array[18] = ", Keywords=";
			array[19] = this.Keywords.ToString();
			array[20] = ", RelativeEffects=[";
			array[21] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[22] = "], RelativeCards=[";
			array[23] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[24] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				UltimateSkillConfig[] array = new UltimateSkillConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new UltimateSkillConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), (UsRepeatableType)binaryReader.ReadInt32(), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				UltimateSkillConfig._data = array;
				UltimateSkillConfig._IdTable = Enumerable.ToDictionary<UltimateSkillConfig, string>(UltimateSkillConfig._data, (UltimateSkillConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/UltimateSkillConfig");
			if (textAsset != null)
			{
				try
				{
					UltimateSkillConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load UltimateSkillConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'UltimateSkillConfig', please reimport config data");
			}
		}
		private static UltimateSkillConfig[] _data = Array.Empty<UltimateSkillConfig>();
		private static Dictionary<string, UltimateSkillConfig> _IdTable = Enumerable.ToDictionary<UltimateSkillConfig, string>(UltimateSkillConfig._data, (UltimateSkillConfig elem) => elem.Id);
	}
}
