using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class PlayerUnitConfig
	{
		private PlayerUnitConfig(string Id, bool HasHomeName, string ModleName, int Order, int ShowOrder, int? UnlockLevel, bool IsSelectable, int? BasicRingOrder, ManaColor LeftColor, ManaColor RightColor, ManaGroup InitialMana, string NarrativeColor, int MaxHp, int InitialMoney, int InitialPower, string UltimateSkillA, string UltimateSkillB, string ExhibitA, string ExhibitB, IReadOnlyList<string> DeckA, IReadOnlyList<string> DeckB, int DifficultyA, int DifficultyB)
		{
			this.Id = Id;
			this.HasHomeName = HasHomeName;
			this.ModleName = ModleName;
			this.Order = Order;
			this.ShowOrder = ShowOrder;
			this.UnlockLevel = UnlockLevel;
			this.IsSelectable = IsSelectable;
			this.BasicRingOrder = BasicRingOrder;
			this.LeftColor = LeftColor;
			this.RightColor = RightColor;
			this.InitialMana = InitialMana;
			this.NarrativeColor = NarrativeColor;
			this.MaxHp = MaxHp;
			this.InitialMoney = InitialMoney;
			this.InitialPower = InitialPower;
			this.UltimateSkillA = UltimateSkillA;
			this.UltimateSkillB = UltimateSkillB;
			this.ExhibitA = ExhibitA;
			this.ExhibitB = ExhibitB;
			this.DeckA = DeckA;
			this.DeckB = DeckB;
			this.DifficultyA = DifficultyA;
			this.DifficultyB = DifficultyB;
		}
		public string Id { get; private set; }
		public bool HasHomeName { get; private set; }
		public string ModleName { get; private set; }
		public int Order { get; private set; }
		public int ShowOrder { get; private set; }
		public int? UnlockLevel { get; private set; }
		public bool IsSelectable { get; private set; }
		public int? BasicRingOrder { get; private set; }
		public ManaColor LeftColor { get; private set; }
		public ManaColor RightColor { get; private set; }
		public ManaGroup InitialMana { get; private set; }
		public string NarrativeColor { get; private set; }
		public int MaxHp { get; private set; }
		public int InitialMoney { get; private set; }
		public int InitialPower { get; private set; }
		public string UltimateSkillA { get; private set; }
		public string UltimateSkillB { get; private set; }
		public string ExhibitA { get; private set; }
		public string ExhibitB { get; private set; }
		public IReadOnlyList<string> DeckA { get; private set; }
		public IReadOnlyList<string> DeckB { get; private set; }
		public int DifficultyA { get; private set; }
		public int DifficultyB { get; private set; }
		public static IReadOnlyList<PlayerUnitConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<PlayerUnitConfig>(PlayerUnitConfig._data);
		}
		public static PlayerUnitConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			PlayerUnitConfig playerUnitConfig;
			return (!PlayerUnitConfig._IdTable.TryGetValue(Id, out playerUnitConfig)) ? null : playerUnitConfig;
		}
		public override string ToString()
		{
			string[] array = new string[47];
			array[0] = "{PlayerUnitConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", HasHomeName=";
			array[3] = ConfigDataManager.System_Boolean.ToString(this.HasHomeName);
			array[4] = ", ModleName=";
			array[5] = ConfigDataManager.System_String.ToString(this.ModleName);
			array[6] = ", Order=";
			array[7] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[8] = ", ShowOrder=";
			array[9] = ConfigDataManager.System_Int32.ToString(this.ShowOrder);
			array[10] = ", UnlockLevel=";
			array[11] = ((this.UnlockLevel == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UnlockLevel.Value));
			array[12] = ", IsSelectable=";
			array[13] = ConfigDataManager.System_Boolean.ToString(this.IsSelectable);
			array[14] = ", BasicRingOrder=";
			array[15] = ((this.BasicRingOrder == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.BasicRingOrder.Value));
			array[16] = ", LeftColor=";
			array[17] = ConfigDataManager.LBoL_Base_ManaColor.ToString(this.LeftColor);
			array[18] = ", RightColor=";
			array[19] = ConfigDataManager.LBoL_Base_ManaColor.ToString(this.RightColor);
			array[20] = ", InitialMana=";
			array[21] = ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.InitialMana);
			array[22] = ", NarrativeColor=";
			array[23] = ConfigDataManager.System_String.ToString(this.NarrativeColor);
			array[24] = ", MaxHp=";
			array[25] = ConfigDataManager.System_Int32.ToString(this.MaxHp);
			array[26] = ", InitialMoney=";
			array[27] = ConfigDataManager.System_Int32.ToString(this.InitialMoney);
			array[28] = ", InitialPower=";
			array[29] = ConfigDataManager.System_Int32.ToString(this.InitialPower);
			array[30] = ", UltimateSkillA=";
			array[31] = ConfigDataManager.System_String.ToString(this.UltimateSkillA);
			array[32] = ", UltimateSkillB=";
			array[33] = ConfigDataManager.System_String.ToString(this.UltimateSkillB);
			array[34] = ", ExhibitA=";
			array[35] = ConfigDataManager.System_String.ToString(this.ExhibitA);
			array[36] = ", ExhibitB=";
			array[37] = ConfigDataManager.System_String.ToString(this.ExhibitB);
			array[38] = ", DeckA=[";
			array[39] = string.Join(", ", Enumerable.Select<string, string>(this.DeckA, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[40] = "], DeckB=[";
			array[41] = string.Join(", ", Enumerable.Select<string, string>(this.DeckB, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[42] = "], DifficultyA=";
			array[43] = ConfigDataManager.System_Int32.ToString(this.DifficultyA);
			array[44] = ", DifficultyB=";
			array[45] = ConfigDataManager.System_Int32.ToString(this.DifficultyB);
			array[46] = "}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				PlayerUnitConfig[] array = new PlayerUnitConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new PlayerUnitConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(binaryReader), ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(binaryReader), ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader));
				}
				PlayerUnitConfig._data = array;
				PlayerUnitConfig._IdTable = Enumerable.ToDictionary<PlayerUnitConfig, string>(PlayerUnitConfig._data, (PlayerUnitConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/PlayerUnitConfig");
			if (textAsset != null)
			{
				try
				{
					PlayerUnitConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load PlayerUnitConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'PlayerUnitConfig', please reimport config data");
			}
		}
		private static PlayerUnitConfig[] _data = Array.Empty<PlayerUnitConfig>();
		private static Dictionary<string, PlayerUnitConfig> _IdTable = Enumerable.ToDictionary<PlayerUnitConfig, string>(PlayerUnitConfig._data, (PlayerUnitConfig elem) => elem.Id);
	}
}
