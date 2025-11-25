using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class EnemyUnitConfig
	{
		private EnemyUnitConfig(string Id, bool RealName, bool OnlyLore, IReadOnlyList<ManaColor> BaseManaColor, int Order, string ModleName, string NarrativeColor, EnemyType Type, bool IsPreludeOpponent, float? HpLength, int? MaxHpAdd, int MaxHp, int? Damage1, int? Damage2, int? Damage3, int? Damage4, int? Power, int? Defend, int? Count1, int? Count2, int? MaxHpHard, int? Damage1Hard, int? Damage2Hard, int? Damage3Hard, int? Damage4Hard, int? PowerHard, int? DefendHard, int? Count1Hard, int? Count2Hard, int? MaxHpLunatic, int? Damage1Lunatic, int? Damage2Lunatic, int? Damage3Lunatic, int? Damage4Lunatic, int? PowerLunatic, int? DefendLunatic, int? Count1Lunatic, int? Count2Lunatic, MinMax PowerLoot, MinMax BluePointLoot, IReadOnlyList<string> Gun1, IReadOnlyList<string> Gun2, IReadOnlyList<string> Gun3, IReadOnlyList<string> Gun4)
		{
			this.Id = Id;
			this.RealName = RealName;
			this.OnlyLore = OnlyLore;
			this.BaseManaColor = BaseManaColor;
			this.Order = Order;
			this.ModleName = ModleName;
			this.NarrativeColor = NarrativeColor;
			this.Type = Type;
			this.IsPreludeOpponent = IsPreludeOpponent;
			this.HpLength = HpLength;
			this.MaxHpAdd = MaxHpAdd;
			this.MaxHp = MaxHp;
			this.Damage1 = Damage1;
			this.Damage2 = Damage2;
			this.Damage3 = Damage3;
			this.Damage4 = Damage4;
			this.Power = Power;
			this.Defend = Defend;
			this.Count1 = Count1;
			this.Count2 = Count2;
			this.MaxHpHard = MaxHpHard;
			this.Damage1Hard = Damage1Hard;
			this.Damage2Hard = Damage2Hard;
			this.Damage3Hard = Damage3Hard;
			this.Damage4Hard = Damage4Hard;
			this.PowerHard = PowerHard;
			this.DefendHard = DefendHard;
			this.Count1Hard = Count1Hard;
			this.Count2Hard = Count2Hard;
			this.MaxHpLunatic = MaxHpLunatic;
			this.Damage1Lunatic = Damage1Lunatic;
			this.Damage2Lunatic = Damage2Lunatic;
			this.Damage3Lunatic = Damage3Lunatic;
			this.Damage4Lunatic = Damage4Lunatic;
			this.PowerLunatic = PowerLunatic;
			this.DefendLunatic = DefendLunatic;
			this.Count1Lunatic = Count1Lunatic;
			this.Count2Lunatic = Count2Lunatic;
			this.PowerLoot = PowerLoot;
			this.BluePointLoot = BluePointLoot;
			this.Gun1 = Gun1;
			this.Gun2 = Gun2;
			this.Gun3 = Gun3;
			this.Gun4 = Gun4;
		}
		public string Id { get; private set; }
		public bool RealName { get; private set; }
		public bool OnlyLore { get; private set; }
		public IReadOnlyList<ManaColor> BaseManaColor { get; private set; }
		public int Order { get; private set; }
		public string ModleName { get; private set; }
		public string NarrativeColor { get; private set; }
		public EnemyType Type { get; private set; }
		public bool IsPreludeOpponent { get; private set; }
		public float? HpLength { get; private set; }
		public int? MaxHpAdd { get; private set; }
		public int MaxHp { get; private set; }
		public int? Damage1 { get; private set; }
		public int? Damage2 { get; private set; }
		public int? Damage3 { get; private set; }
		public int? Damage4 { get; private set; }
		public int? Power { get; private set; }
		public int? Defend { get; private set; }
		public int? Count1 { get; private set; }
		public int? Count2 { get; private set; }
		public int? MaxHpHard { get; private set; }
		public int? Damage1Hard { get; private set; }
		public int? Damage2Hard { get; private set; }
		public int? Damage3Hard { get; private set; }
		public int? Damage4Hard { get; private set; }
		public int? PowerHard { get; private set; }
		public int? DefendHard { get; private set; }
		public int? Count1Hard { get; private set; }
		public int? Count2Hard { get; private set; }
		public int? MaxHpLunatic { get; private set; }
		public int? Damage1Lunatic { get; private set; }
		public int? Damage2Lunatic { get; private set; }
		public int? Damage3Lunatic { get; private set; }
		public int? Damage4Lunatic { get; private set; }
		public int? PowerLunatic { get; private set; }
		public int? DefendLunatic { get; private set; }
		public int? Count1Lunatic { get; private set; }
		public int? Count2Lunatic { get; private set; }
		public MinMax PowerLoot { get; private set; }
		public MinMax BluePointLoot { get; private set; }
		public IReadOnlyList<string> Gun1 { get; private set; }
		public IReadOnlyList<string> Gun2 { get; private set; }
		public IReadOnlyList<string> Gun3 { get; private set; }
		public IReadOnlyList<string> Gun4 { get; private set; }
		public static IReadOnlyList<EnemyUnitConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<EnemyUnitConfig>(EnemyUnitConfig._data);
		}
		public static EnemyUnitConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			EnemyUnitConfig enemyUnitConfig;
			return (!EnemyUnitConfig._IdTable.TryGetValue(Id, out enemyUnitConfig)) ? null : enemyUnitConfig;
		}
		public override string ToString()
		{
			string[] array = new string[89];
			array[0] = "{EnemyUnitConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", RealName=";
			array[3] = ConfigDataManager.System_Boolean.ToString(this.RealName);
			array[4] = ", OnlyLore=";
			array[5] = ConfigDataManager.System_Boolean.ToString(this.OnlyLore);
			array[6] = ", BaseManaColor=[";
			array[7] = string.Join(", ", Enumerable.Select<ManaColor, string>(this.BaseManaColor, (ManaColor v1) => ConfigDataManager.LBoL_Base_ManaColor.ToString(v1)));
			array[8] = "], Order=";
			array[9] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[10] = ", ModleName=";
			array[11] = ConfigDataManager.System_String.ToString(this.ModleName);
			array[12] = ", NarrativeColor=";
			array[13] = ConfigDataManager.System_String.ToString(this.NarrativeColor);
			array[14] = ", Type=";
			array[15] = this.Type.ToString();
			array[16] = ", IsPreludeOpponent=";
			array[17] = ConfigDataManager.System_Boolean.ToString(this.IsPreludeOpponent);
			array[18] = ", HpLength=";
			array[19] = ((this.HpLength == null) ? "null" : ConfigDataManager.System_Single.ToString(this.HpLength.Value));
			array[20] = ", MaxHpAdd=";
			array[21] = ((this.MaxHpAdd == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.MaxHpAdd.Value));
			array[22] = ", MaxHp=";
			array[23] = ConfigDataManager.System_Int32.ToString(this.MaxHp);
			array[24] = ", Damage1=";
			array[25] = ((this.Damage1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage1.Value));
			array[26] = ", Damage2=";
			array[27] = ((this.Damage2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage2.Value));
			array[28] = ", Damage3=";
			array[29] = ((this.Damage3 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage3.Value));
			array[30] = ", Damage4=";
			array[31] = ((this.Damage4 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage4.Value));
			array[32] = ", Power=";
			array[33] = ((this.Power == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Power.Value));
			array[34] = ", Defend=";
			array[35] = ((this.Defend == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Defend.Value));
			array[36] = ", Count1=";
			array[37] = ((this.Count1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Count1.Value));
			array[38] = ", Count2=";
			array[39] = ((this.Count2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Count2.Value));
			array[40] = ", MaxHpHard=";
			array[41] = ((this.MaxHpHard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.MaxHpHard.Value));
			array[42] = ", Damage1Hard=";
			array[43] = ((this.Damage1Hard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage1Hard.Value));
			array[44] = ", Damage2Hard=";
			array[45] = ((this.Damage2Hard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage2Hard.Value));
			array[46] = ", Damage3Hard=";
			array[47] = ((this.Damage3Hard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage3Hard.Value));
			array[48] = ", Damage4Hard=";
			array[49] = ((this.Damage4Hard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage4Hard.Value));
			array[50] = ", PowerHard=";
			array[51] = ((this.PowerHard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.PowerHard.Value));
			array[52] = ", DefendHard=";
			array[53] = ((this.DefendHard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.DefendHard.Value));
			array[54] = ", Count1Hard=";
			array[55] = ((this.Count1Hard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Count1Hard.Value));
			array[56] = ", Count2Hard=";
			array[57] = ((this.Count2Hard == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Count2Hard.Value));
			array[58] = ", MaxHpLunatic=";
			array[59] = ((this.MaxHpLunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.MaxHpLunatic.Value));
			array[60] = ", Damage1Lunatic=";
			array[61] = ((this.Damage1Lunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage1Lunatic.Value));
			array[62] = ", Damage2Lunatic=";
			array[63] = ((this.Damage2Lunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage2Lunatic.Value));
			array[64] = ", Damage3Lunatic=";
			array[65] = ((this.Damage3Lunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage3Lunatic.Value));
			array[66] = ", Damage4Lunatic=";
			array[67] = ((this.Damage4Lunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage4Lunatic.Value));
			array[68] = ", PowerLunatic=";
			array[69] = ((this.PowerLunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.PowerLunatic.Value));
			array[70] = ", DefendLunatic=";
			array[71] = ((this.DefendLunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.DefendLunatic.Value));
			array[72] = ", Count1Lunatic=";
			array[73] = ((this.Count1Lunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Count1Lunatic.Value));
			array[74] = ", Count2Lunatic=";
			array[75] = ((this.Count2Lunatic == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Count2Lunatic.Value));
			array[76] = ", PowerLoot=";
			array[77] = ConfigDataManager.LBoL_Base_MinMax.ToString(this.PowerLoot);
			array[78] = ", BluePointLoot=";
			array[79] = ConfigDataManager.LBoL_Base_MinMax.ToString(this.BluePointLoot);
			array[80] = ", Gun1=[";
			array[81] = string.Join(", ", Enumerable.Select<string, string>(this.Gun1, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[82] = "], Gun2=[";
			array[83] = string.Join(", ", Enumerable.Select<string, string>(this.Gun2, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[84] = "], Gun3=[";
			array[85] = string.Join(", ", Enumerable.Select<string, string>(this.Gun3, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[86] = "], Gun4=[";
			array[87] = string.Join(", ", Enumerable.Select<string, string>(this.Gun4, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[88] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				EnemyUnitConfig[] array = new EnemyUnitConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new EnemyUnitConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.ReadList<ManaColor>(binaryReader, (BinaryReader r1) => ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(r1)), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), (EnemyType)binaryReader.ReadInt32(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), ConfigDataManager.System_Int32.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), ConfigDataManager.LBoL_Base_MinMax.ReadFrom(binaryReader), ConfigDataManager.LBoL_Base_MinMax.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				EnemyUnitConfig._data = array;
				EnemyUnitConfig._IdTable = Enumerable.ToDictionary<EnemyUnitConfig, string>(EnemyUnitConfig._data, (EnemyUnitConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/EnemyUnitConfig");
			if (textAsset != null)
			{
				try
				{
					EnemyUnitConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load EnemyUnitConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'EnemyUnitConfig', please reimport config data");
			}
		}
		private static EnemyUnitConfig[] _data = Array.Empty<EnemyUnitConfig>();
		private static Dictionary<string, EnemyUnitConfig> _IdTable = Enumerable.ToDictionary<EnemyUnitConfig, string>(EnemyUnitConfig._data, (EnemyUnitConfig elem) => elem.Id);
	}
}
