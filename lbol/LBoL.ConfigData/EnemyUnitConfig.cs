using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000007 RID: 7
	public sealed class EnemyUnitConfig
	{
		// Token: 0x06000066 RID: 102 RVA: 0x00003074 File Offset: 0x00001274
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

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000067 RID: 103 RVA: 0x000031E4 File Offset: 0x000013E4
		// (set) Token: 0x06000068 RID: 104 RVA: 0x000031EC File Offset: 0x000013EC
		public string Id { get; private set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000069 RID: 105 RVA: 0x000031F5 File Offset: 0x000013F5
		// (set) Token: 0x0600006A RID: 106 RVA: 0x000031FD File Offset: 0x000013FD
		public bool RealName { get; private set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00003206 File Offset: 0x00001406
		// (set) Token: 0x0600006C RID: 108 RVA: 0x0000320E File Offset: 0x0000140E
		public bool OnlyLore { get; private set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00003217 File Offset: 0x00001417
		// (set) Token: 0x0600006E RID: 110 RVA: 0x0000321F File Offset: 0x0000141F
		public IReadOnlyList<ManaColor> BaseManaColor { get; private set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003228 File Offset: 0x00001428
		// (set) Token: 0x06000070 RID: 112 RVA: 0x00003230 File Offset: 0x00001430
		public int Order { get; private set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000071 RID: 113 RVA: 0x00003239 File Offset: 0x00001439
		// (set) Token: 0x06000072 RID: 114 RVA: 0x00003241 File Offset: 0x00001441
		public string ModleName { get; private set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000073 RID: 115 RVA: 0x0000324A File Offset: 0x0000144A
		// (set) Token: 0x06000074 RID: 116 RVA: 0x00003252 File Offset: 0x00001452
		public string NarrativeColor { get; private set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000075 RID: 117 RVA: 0x0000325B File Offset: 0x0000145B
		// (set) Token: 0x06000076 RID: 118 RVA: 0x00003263 File Offset: 0x00001463
		public EnemyType Type { get; private set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000077 RID: 119 RVA: 0x0000326C File Offset: 0x0000146C
		// (set) Token: 0x06000078 RID: 120 RVA: 0x00003274 File Offset: 0x00001474
		public bool IsPreludeOpponent { get; private set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000079 RID: 121 RVA: 0x0000327D File Offset: 0x0000147D
		// (set) Token: 0x0600007A RID: 122 RVA: 0x00003285 File Offset: 0x00001485
		public float? HpLength { get; private set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x0600007B RID: 123 RVA: 0x0000328E File Offset: 0x0000148E
		// (set) Token: 0x0600007C RID: 124 RVA: 0x00003296 File Offset: 0x00001496
		public int? MaxHpAdd { get; private set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x0600007D RID: 125 RVA: 0x0000329F File Offset: 0x0000149F
		// (set) Token: 0x0600007E RID: 126 RVA: 0x000032A7 File Offset: 0x000014A7
		public int MaxHp { get; private set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600007F RID: 127 RVA: 0x000032B0 File Offset: 0x000014B0
		// (set) Token: 0x06000080 RID: 128 RVA: 0x000032B8 File Offset: 0x000014B8
		public int? Damage1 { get; private set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000081 RID: 129 RVA: 0x000032C1 File Offset: 0x000014C1
		// (set) Token: 0x06000082 RID: 130 RVA: 0x000032C9 File Offset: 0x000014C9
		public int? Damage2 { get; private set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000083 RID: 131 RVA: 0x000032D2 File Offset: 0x000014D2
		// (set) Token: 0x06000084 RID: 132 RVA: 0x000032DA File Offset: 0x000014DA
		public int? Damage3 { get; private set; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000085 RID: 133 RVA: 0x000032E3 File Offset: 0x000014E3
		// (set) Token: 0x06000086 RID: 134 RVA: 0x000032EB File Offset: 0x000014EB
		public int? Damage4 { get; private set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000087 RID: 135 RVA: 0x000032F4 File Offset: 0x000014F4
		// (set) Token: 0x06000088 RID: 136 RVA: 0x000032FC File Offset: 0x000014FC
		public int? Power { get; private set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000089 RID: 137 RVA: 0x00003305 File Offset: 0x00001505
		// (set) Token: 0x0600008A RID: 138 RVA: 0x0000330D File Offset: 0x0000150D
		public int? Defend { get; private set; }

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00003316 File Offset: 0x00001516
		// (set) Token: 0x0600008C RID: 140 RVA: 0x0000331E File Offset: 0x0000151E
		public int? Count1 { get; private set; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600008D RID: 141 RVA: 0x00003327 File Offset: 0x00001527
		// (set) Token: 0x0600008E RID: 142 RVA: 0x0000332F File Offset: 0x0000152F
		public int? Count2 { get; private set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00003338 File Offset: 0x00001538
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00003340 File Offset: 0x00001540
		public int? MaxHpHard { get; private set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00003349 File Offset: 0x00001549
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00003351 File Offset: 0x00001551
		public int? Damage1Hard { get; private set; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000093 RID: 147 RVA: 0x0000335A File Offset: 0x0000155A
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00003362 File Offset: 0x00001562
		public int? Damage2Hard { get; private set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000095 RID: 149 RVA: 0x0000336B File Offset: 0x0000156B
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00003373 File Offset: 0x00001573
		public int? Damage3Hard { get; private set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000097 RID: 151 RVA: 0x0000337C File Offset: 0x0000157C
		// (set) Token: 0x06000098 RID: 152 RVA: 0x00003384 File Offset: 0x00001584
		public int? Damage4Hard { get; private set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000099 RID: 153 RVA: 0x0000338D File Offset: 0x0000158D
		// (set) Token: 0x0600009A RID: 154 RVA: 0x00003395 File Offset: 0x00001595
		public int? PowerHard { get; private set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600009B RID: 155 RVA: 0x0000339E File Offset: 0x0000159E
		// (set) Token: 0x0600009C RID: 156 RVA: 0x000033A6 File Offset: 0x000015A6
		public int? DefendHard { get; private set; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600009D RID: 157 RVA: 0x000033AF File Offset: 0x000015AF
		// (set) Token: 0x0600009E RID: 158 RVA: 0x000033B7 File Offset: 0x000015B7
		public int? Count1Hard { get; private set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600009F RID: 159 RVA: 0x000033C0 File Offset: 0x000015C0
		// (set) Token: 0x060000A0 RID: 160 RVA: 0x000033C8 File Offset: 0x000015C8
		public int? Count2Hard { get; private set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x000033D1 File Offset: 0x000015D1
		// (set) Token: 0x060000A2 RID: 162 RVA: 0x000033D9 File Offset: 0x000015D9
		public int? MaxHpLunatic { get; private set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x000033E2 File Offset: 0x000015E2
		// (set) Token: 0x060000A4 RID: 164 RVA: 0x000033EA File Offset: 0x000015EA
		public int? Damage1Lunatic { get; private set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x000033F3 File Offset: 0x000015F3
		// (set) Token: 0x060000A6 RID: 166 RVA: 0x000033FB File Offset: 0x000015FB
		public int? Damage2Lunatic { get; private set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x00003404 File Offset: 0x00001604
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x0000340C File Offset: 0x0000160C
		public int? Damage3Lunatic { get; private set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00003415 File Offset: 0x00001615
		// (set) Token: 0x060000AA RID: 170 RVA: 0x0000341D File Offset: 0x0000161D
		public int? Damage4Lunatic { get; private set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00003426 File Offset: 0x00001626
		// (set) Token: 0x060000AC RID: 172 RVA: 0x0000342E File Offset: 0x0000162E
		public int? PowerLunatic { get; private set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00003437 File Offset: 0x00001637
		// (set) Token: 0x060000AE RID: 174 RVA: 0x0000343F File Offset: 0x0000163F
		public int? DefendLunatic { get; private set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00003448 File Offset: 0x00001648
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00003450 File Offset: 0x00001650
		public int? Count1Lunatic { get; private set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00003459 File Offset: 0x00001659
		// (set) Token: 0x060000B2 RID: 178 RVA: 0x00003461 File Offset: 0x00001661
		public int? Count2Lunatic { get; private set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x0000346A File Offset: 0x0000166A
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x00003472 File Offset: 0x00001672
		public MinMax PowerLoot { get; private set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x0000347B File Offset: 0x0000167B
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x00003483 File Offset: 0x00001683
		public MinMax BluePointLoot { get; private set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x0000348C File Offset: 0x0000168C
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x00003494 File Offset: 0x00001694
		public IReadOnlyList<string> Gun1 { get; private set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x0000349D File Offset: 0x0000169D
		// (set) Token: 0x060000BA RID: 186 RVA: 0x000034A5 File Offset: 0x000016A5
		public IReadOnlyList<string> Gun2 { get; private set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000BB RID: 187 RVA: 0x000034AE File Offset: 0x000016AE
		// (set) Token: 0x060000BC RID: 188 RVA: 0x000034B6 File Offset: 0x000016B6
		public IReadOnlyList<string> Gun3 { get; private set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000BD RID: 189 RVA: 0x000034BF File Offset: 0x000016BF
		// (set) Token: 0x060000BE RID: 190 RVA: 0x000034C7 File Offset: 0x000016C7
		public IReadOnlyList<string> Gun4 { get; private set; }

		// Token: 0x060000BF RID: 191 RVA: 0x000034D0 File Offset: 0x000016D0
		public static IReadOnlyList<EnemyUnitConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<EnemyUnitConfig>(EnemyUnitConfig._data);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000034E4 File Offset: 0x000016E4
		public static EnemyUnitConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			EnemyUnitConfig enemyUnitConfig;
			return (!EnemyUnitConfig._IdTable.TryGetValue(Id, out enemyUnitConfig)) ? null : enemyUnitConfig;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00003510 File Offset: 0x00001710
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

		// Token: 0x060000C2 RID: 194 RVA: 0x00003F14 File Offset: 0x00002114
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

		// Token: 0x060000C3 RID: 195 RVA: 0x00004570 File Offset: 0x00002770
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

		// Token: 0x04000058 RID: 88
		private static EnemyUnitConfig[] _data = Array.Empty<EnemyUnitConfig>();

		// Token: 0x04000059 RID: 89
		private static Dictionary<string, EnemyUnitConfig> _IdTable = Enumerable.ToDictionary<EnemyUnitConfig, string>(EnemyUnitConfig._data, (EnemyUnitConfig elem) => elem.Id);
	}
}
