using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000009 RID: 9
	public sealed class PlayerUnitConfig
	{
		// Token: 0x060000FA RID: 250 RVA: 0x00004C8C File Offset: 0x00002E8C
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

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060000FB RID: 251 RVA: 0x00004D54 File Offset: 0x00002F54
		// (set) Token: 0x060000FC RID: 252 RVA: 0x00004D5C File Offset: 0x00002F5C
		public string Id { get; private set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060000FD RID: 253 RVA: 0x00004D65 File Offset: 0x00002F65
		// (set) Token: 0x060000FE RID: 254 RVA: 0x00004D6D File Offset: 0x00002F6D
		public bool HasHomeName { get; private set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060000FF RID: 255 RVA: 0x00004D76 File Offset: 0x00002F76
		// (set) Token: 0x06000100 RID: 256 RVA: 0x00004D7E File Offset: 0x00002F7E
		public string ModleName { get; private set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000101 RID: 257 RVA: 0x00004D87 File Offset: 0x00002F87
		// (set) Token: 0x06000102 RID: 258 RVA: 0x00004D8F File Offset: 0x00002F8F
		public int Order { get; private set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000103 RID: 259 RVA: 0x00004D98 File Offset: 0x00002F98
		// (set) Token: 0x06000104 RID: 260 RVA: 0x00004DA0 File Offset: 0x00002FA0
		public int ShowOrder { get; private set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000105 RID: 261 RVA: 0x00004DA9 File Offset: 0x00002FA9
		// (set) Token: 0x06000106 RID: 262 RVA: 0x00004DB1 File Offset: 0x00002FB1
		public int? UnlockLevel { get; private set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000107 RID: 263 RVA: 0x00004DBA File Offset: 0x00002FBA
		// (set) Token: 0x06000108 RID: 264 RVA: 0x00004DC2 File Offset: 0x00002FC2
		public bool IsSelectable { get; private set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000109 RID: 265 RVA: 0x00004DCB File Offset: 0x00002FCB
		// (set) Token: 0x0600010A RID: 266 RVA: 0x00004DD3 File Offset: 0x00002FD3
		public int? BasicRingOrder { get; private set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600010B RID: 267 RVA: 0x00004DDC File Offset: 0x00002FDC
		// (set) Token: 0x0600010C RID: 268 RVA: 0x00004DE4 File Offset: 0x00002FE4
		public ManaColor LeftColor { get; private set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600010D RID: 269 RVA: 0x00004DED File Offset: 0x00002FED
		// (set) Token: 0x0600010E RID: 270 RVA: 0x00004DF5 File Offset: 0x00002FF5
		public ManaColor RightColor { get; private set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600010F RID: 271 RVA: 0x00004DFE File Offset: 0x00002FFE
		// (set) Token: 0x06000110 RID: 272 RVA: 0x00004E06 File Offset: 0x00003006
		public ManaGroup InitialMana { get; private set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000111 RID: 273 RVA: 0x00004E0F File Offset: 0x0000300F
		// (set) Token: 0x06000112 RID: 274 RVA: 0x00004E17 File Offset: 0x00003017
		public string NarrativeColor { get; private set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000113 RID: 275 RVA: 0x00004E20 File Offset: 0x00003020
		// (set) Token: 0x06000114 RID: 276 RVA: 0x00004E28 File Offset: 0x00003028
		public int MaxHp { get; private set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000115 RID: 277 RVA: 0x00004E31 File Offset: 0x00003031
		// (set) Token: 0x06000116 RID: 278 RVA: 0x00004E39 File Offset: 0x00003039
		public int InitialMoney { get; private set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000117 RID: 279 RVA: 0x00004E42 File Offset: 0x00003042
		// (set) Token: 0x06000118 RID: 280 RVA: 0x00004E4A File Offset: 0x0000304A
		public int InitialPower { get; private set; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000119 RID: 281 RVA: 0x00004E53 File Offset: 0x00003053
		// (set) Token: 0x0600011A RID: 282 RVA: 0x00004E5B File Offset: 0x0000305B
		public string UltimateSkillA { get; private set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x0600011B RID: 283 RVA: 0x00004E64 File Offset: 0x00003064
		// (set) Token: 0x0600011C RID: 284 RVA: 0x00004E6C File Offset: 0x0000306C
		public string UltimateSkillB { get; private set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x0600011D RID: 285 RVA: 0x00004E75 File Offset: 0x00003075
		// (set) Token: 0x0600011E RID: 286 RVA: 0x00004E7D File Offset: 0x0000307D
		public string ExhibitA { get; private set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00004E86 File Offset: 0x00003086
		// (set) Token: 0x06000120 RID: 288 RVA: 0x00004E8E File Offset: 0x0000308E
		public string ExhibitB { get; private set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00004E97 File Offset: 0x00003097
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00004E9F File Offset: 0x0000309F
		public IReadOnlyList<string> DeckA { get; private set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000123 RID: 291 RVA: 0x00004EA8 File Offset: 0x000030A8
		// (set) Token: 0x06000124 RID: 292 RVA: 0x00004EB0 File Offset: 0x000030B0
		public IReadOnlyList<string> DeckB { get; private set; }

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000125 RID: 293 RVA: 0x00004EB9 File Offset: 0x000030B9
		// (set) Token: 0x06000126 RID: 294 RVA: 0x00004EC1 File Offset: 0x000030C1
		public int DifficultyA { get; private set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000127 RID: 295 RVA: 0x00004ECA File Offset: 0x000030CA
		// (set) Token: 0x06000128 RID: 296 RVA: 0x00004ED2 File Offset: 0x000030D2
		public int DifficultyB { get; private set; }

		// Token: 0x06000129 RID: 297 RVA: 0x00004EDB File Offset: 0x000030DB
		public static IReadOnlyList<PlayerUnitConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<PlayerUnitConfig>(PlayerUnitConfig._data);
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00004EEC File Offset: 0x000030EC
		public static PlayerUnitConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			PlayerUnitConfig playerUnitConfig;
			return (!PlayerUnitConfig._IdTable.TryGetValue(Id, out playerUnitConfig)) ? null : playerUnitConfig;
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00004F18 File Offset: 0x00003118
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

		// Token: 0x0600012C RID: 300 RVA: 0x0000525C File Offset: 0x0000345C
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

		// Token: 0x0600012D RID: 301 RVA: 0x00005468 File Offset: 0x00003668
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

		// Token: 0x04000091 RID: 145
		private static PlayerUnitConfig[] _data = Array.Empty<PlayerUnitConfig>();

		// Token: 0x04000092 RID: 146
		private static Dictionary<string, PlayerUnitConfig> _IdTable = Enumerable.ToDictionary<PlayerUnitConfig, string>(PlayerUnitConfig._data, (PlayerUnitConfig elem) => elem.Id);
	}
}
