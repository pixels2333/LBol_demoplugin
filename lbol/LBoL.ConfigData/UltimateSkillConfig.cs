using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200000A RID: 10
	public sealed class UltimateSkillConfig
	{
		// Token: 0x06000135 RID: 309 RVA: 0x00005548 File Offset: 0x00003748
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

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000136 RID: 310 RVA: 0x000055B8 File Offset: 0x000037B8
		// (set) Token: 0x06000137 RID: 311 RVA: 0x000055C0 File Offset: 0x000037C0
		public string Id { get; private set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000138 RID: 312 RVA: 0x000055C9 File Offset: 0x000037C9
		// (set) Token: 0x06000139 RID: 313 RVA: 0x000055D1 File Offset: 0x000037D1
		public int Order { get; private set; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600013A RID: 314 RVA: 0x000055DA File Offset: 0x000037DA
		// (set) Token: 0x0600013B RID: 315 RVA: 0x000055E2 File Offset: 0x000037E2
		public int PowerCost { get; private set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600013C RID: 316 RVA: 0x000055EB File Offset: 0x000037EB
		// (set) Token: 0x0600013D RID: 317 RVA: 0x000055F3 File Offset: 0x000037F3
		public int PowerPerLevel { get; private set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600013E RID: 318 RVA: 0x000055FC File Offset: 0x000037FC
		// (set) Token: 0x0600013F RID: 319 RVA: 0x00005604 File Offset: 0x00003804
		public int MaxPowerLevel { get; private set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000140 RID: 320 RVA: 0x0000560D File Offset: 0x0000380D
		// (set) Token: 0x06000141 RID: 321 RVA: 0x00005615 File Offset: 0x00003815
		public UsRepeatableType RepeatableType { get; private set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000142 RID: 322 RVA: 0x0000561E File Offset: 0x0000381E
		// (set) Token: 0x06000143 RID: 323 RVA: 0x00005626 File Offset: 0x00003826
		public int Damage { get; private set; }

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000562F File Offset: 0x0000382F
		// (set) Token: 0x06000145 RID: 325 RVA: 0x00005637 File Offset: 0x00003837
		public int Value1 { get; private set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000146 RID: 326 RVA: 0x00005640 File Offset: 0x00003840
		// (set) Token: 0x06000147 RID: 327 RVA: 0x00005648 File Offset: 0x00003848
		public int Value2 { get; private set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000148 RID: 328 RVA: 0x00005651 File Offset: 0x00003851
		// (set) Token: 0x06000149 RID: 329 RVA: 0x00005659 File Offset: 0x00003859
		public Keyword Keywords { get; private set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600014A RID: 330 RVA: 0x00005662 File Offset: 0x00003862
		// (set) Token: 0x0600014B RID: 331 RVA: 0x0000566A File Offset: 0x0000386A
		public IReadOnlyList<string> RelativeEffects { get; private set; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600014C RID: 332 RVA: 0x00005673 File Offset: 0x00003873
		// (set) Token: 0x0600014D RID: 333 RVA: 0x0000567B File Offset: 0x0000387B
		public IReadOnlyList<string> RelativeCards { get; private set; }

		// Token: 0x0600014E RID: 334 RVA: 0x00005684 File Offset: 0x00003884
		public static IReadOnlyList<UltimateSkillConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<UltimateSkillConfig>(UltimateSkillConfig._data);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00005698 File Offset: 0x00003898
		public static UltimateSkillConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			UltimateSkillConfig ultimateSkillConfig;
			return (!UltimateSkillConfig._IdTable.TryGetValue(Id, out ultimateSkillConfig)) ? null : ultimateSkillConfig;
		}

		// Token: 0x06000150 RID: 336 RVA: 0x000056C4 File Offset: 0x000038C4
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

		// Token: 0x06000151 RID: 337 RVA: 0x00005888 File Offset: 0x00003A88
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

		// Token: 0x06000152 RID: 338 RVA: 0x000059D4 File Offset: 0x00003BD4
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

		// Token: 0x040000A4 RID: 164
		private static UltimateSkillConfig[] _data = Array.Empty<UltimateSkillConfig>();

		// Token: 0x040000A5 RID: 165
		private static Dictionary<string, UltimateSkillConfig> _IdTable = Enumerable.ToDictionary<UltimateSkillConfig, string>(UltimateSkillConfig._data, (UltimateSkillConfig elem) => elem.Id);
	}
}
