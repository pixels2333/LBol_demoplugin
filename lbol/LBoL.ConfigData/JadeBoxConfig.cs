using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class JadeBoxConfig
	{
		private JadeBoxConfig(int Index, string Id, int Order, IReadOnlyList<string> Group, int? Value1, int? Value2, int? Value3, ManaGroup? Mana, Keyword Keywords, IReadOnlyList<string> RelativeEffects, IReadOnlyList<string> RelativeCards)
		{
			this.Index = Index;
			this.Id = Id;
			this.Order = Order;
			this.Group = Group;
			this.Value1 = Value1;
			this.Value2 = Value2;
			this.Value3 = Value3;
			this.Mana = Mana;
			this.Keywords = Keywords;
			this.RelativeEffects = RelativeEffects;
			this.RelativeCards = RelativeCards;
		}
		public int Index { get; private set; }
		public string Id { get; private set; }
		public int Order { get; private set; }
		public IReadOnlyList<string> Group { get; private set; }
		public int? Value1 { get; private set; }
		public int? Value2 { get; private set; }
		public int? Value3 { get; private set; }
		public ManaGroup? Mana { get; private set; }
		public Keyword Keywords { get; private set; }
		public IReadOnlyList<string> RelativeEffects { get; private set; }
		public IReadOnlyList<string> RelativeCards { get; private set; }
		public static IReadOnlyList<JadeBoxConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<JadeBoxConfig>(JadeBoxConfig._data);
		}
		public static JadeBoxConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			JadeBoxConfig jadeBoxConfig;
			return (!JadeBoxConfig._IdTable.TryGetValue(Id, out jadeBoxConfig)) ? null : jadeBoxConfig;
		}
		public override string ToString()
		{
			string[] array = new string[23];
			array[0] = "{JadeBoxConfig Index=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Index);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", Order=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[6] = ", Group=[";
			array[7] = string.Join(", ", Enumerable.Select<string, string>(this.Group, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[8] = "], Value1=";
			array[9] = ((this.Value1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value1.Value));
			array[10] = ", Value2=";
			array[11] = ((this.Value2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value2.Value));
			array[12] = ", Value3=";
			array[13] = ((this.Value3 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value3.Value));
			array[14] = ", Mana=";
			array[15] = ((this.Mana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Mana.Value));
			array[16] = ", Keywords=";
			array[17] = this.Keywords.ToString();
			array[18] = ", RelativeEffects=[";
			array[19] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[20] = "], RelativeCards=[";
			array[21] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[22] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				JadeBoxConfig[] array = new JadeBoxConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new JadeBoxConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				JadeBoxConfig._data = array;
				JadeBoxConfig._IdTable = Enumerable.ToDictionary<JadeBoxConfig, string>(JadeBoxConfig._data, (JadeBoxConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/JadeBoxConfig");
			if (textAsset != null)
			{
				try
				{
					JadeBoxConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load JadeBoxConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'JadeBoxConfig', please reimport config data");
			}
		}
		private static JadeBoxConfig[] _data = Array.Empty<JadeBoxConfig>();
		private static Dictionary<string, JadeBoxConfig> _IdTable = Enumerable.ToDictionary<JadeBoxConfig, string>(JadeBoxConfig._data, (JadeBoxConfig elem) => elem.Id);
	}
}
