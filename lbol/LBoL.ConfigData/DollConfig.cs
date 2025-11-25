using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class DollConfig
	{
		private DollConfig(string Id, bool Flip, int? Damage, int? Value1, int? Value2, ManaGroup? Mana, bool Usable, bool HasMagic, int InitialMagic, int MagicCost, int MaxMagic, Keyword Keywords, IReadOnlyList<string> RelativeEffects)
		{
			this.Id = Id;
			this.Flip = Flip;
			this.Damage = Damage;
			this.Value1 = Value1;
			this.Value2 = Value2;
			this.Mana = Mana;
			this.Usable = Usable;
			this.HasMagic = HasMagic;
			this.InitialMagic = InitialMagic;
			this.MagicCost = MagicCost;
			this.MaxMagic = MaxMagic;
			this.Keywords = Keywords;
			this.RelativeEffects = RelativeEffects;
		}
		public string Id { get; private set; }
		public bool Flip { get; private set; }
		public int? Damage { get; private set; }
		public int? Value1 { get; private set; }
		public int? Value2 { get; private set; }
		public ManaGroup? Mana { get; private set; }
		public bool Usable { get; private set; }
		public bool HasMagic { get; private set; }
		public int InitialMagic { get; private set; }
		public int MagicCost { get; private set; }
		public int MaxMagic { get; private set; }
		public Keyword Keywords { get; private set; }
		public IReadOnlyList<string> RelativeEffects { get; private set; }
		public static IReadOnlyList<DollConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<DollConfig>(DollConfig._data);
		}
		public static DollConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			DollConfig dollConfig;
			return (!DollConfig._IdTable.TryGetValue(Id, out dollConfig)) ? null : dollConfig;
		}
		public override string ToString()
		{
			string[] array = new string[27];
			array[0] = "{DollConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", Flip=";
			array[3] = ConfigDataManager.System_Boolean.ToString(this.Flip);
			array[4] = ", Damage=";
			array[5] = ((this.Damage == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage.Value));
			array[6] = ", Value1=";
			array[7] = ((this.Value1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value1.Value));
			array[8] = ", Value2=";
			array[9] = ((this.Value2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value2.Value));
			array[10] = ", Mana=";
			array[11] = ((this.Mana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Mana.Value));
			array[12] = ", Usable=";
			array[13] = ConfigDataManager.System_Boolean.ToString(this.Usable);
			array[14] = ", HasMagic=";
			array[15] = ConfigDataManager.System_Boolean.ToString(this.HasMagic);
			array[16] = ", InitialMagic=";
			array[17] = ConfigDataManager.System_Int32.ToString(this.InitialMagic);
			array[18] = ", MagicCost=";
			array[19] = ConfigDataManager.System_Int32.ToString(this.MagicCost);
			array[20] = ", MaxMagic=";
			array[21] = ConfigDataManager.System_Int32.ToString(this.MaxMagic);
			array[22] = ", Keywords=";
			array[23] = this.Keywords.ToString();
			array[24] = ", RelativeEffects=[";
			array[25] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[26] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				DollConfig[] array = new DollConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new DollConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				DollConfig._data = array;
				DollConfig._IdTable = Enumerable.ToDictionary<DollConfig, string>(DollConfig._data, (DollConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/DollConfig");
			if (textAsset != null)
			{
				try
				{
					DollConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load DollConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'DollConfig', please reimport config data");
			}
		}
		private static DollConfig[] _data = Array.Empty<DollConfig>();
		private static Dictionary<string, DollConfig> _IdTable = Enumerable.ToDictionary<DollConfig, string>(DollConfig._data, (DollConfig elem) => elem.Id);
	}
}
