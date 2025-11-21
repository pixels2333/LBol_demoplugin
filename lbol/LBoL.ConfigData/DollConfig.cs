using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200000B RID: 11
	public sealed class DollConfig
	{
		// Token: 0x0600015A RID: 346 RVA: 0x00005AB4 File Offset: 0x00003CB4
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

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600015B RID: 347 RVA: 0x00005B2C File Offset: 0x00003D2C
		// (set) Token: 0x0600015C RID: 348 RVA: 0x00005B34 File Offset: 0x00003D34
		public string Id { get; private set; }

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x0600015D RID: 349 RVA: 0x00005B3D File Offset: 0x00003D3D
		// (set) Token: 0x0600015E RID: 350 RVA: 0x00005B45 File Offset: 0x00003D45
		public bool Flip { get; private set; }

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x0600015F RID: 351 RVA: 0x00005B4E File Offset: 0x00003D4E
		// (set) Token: 0x06000160 RID: 352 RVA: 0x00005B56 File Offset: 0x00003D56
		public int? Damage { get; private set; }

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000161 RID: 353 RVA: 0x00005B5F File Offset: 0x00003D5F
		// (set) Token: 0x06000162 RID: 354 RVA: 0x00005B67 File Offset: 0x00003D67
		public int? Value1 { get; private set; }

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000163 RID: 355 RVA: 0x00005B70 File Offset: 0x00003D70
		// (set) Token: 0x06000164 RID: 356 RVA: 0x00005B78 File Offset: 0x00003D78
		public int? Value2 { get; private set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000165 RID: 357 RVA: 0x00005B81 File Offset: 0x00003D81
		// (set) Token: 0x06000166 RID: 358 RVA: 0x00005B89 File Offset: 0x00003D89
		public ManaGroup? Mana { get; private set; }

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000167 RID: 359 RVA: 0x00005B92 File Offset: 0x00003D92
		// (set) Token: 0x06000168 RID: 360 RVA: 0x00005B9A File Offset: 0x00003D9A
		public bool Usable { get; private set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000169 RID: 361 RVA: 0x00005BA3 File Offset: 0x00003DA3
		// (set) Token: 0x0600016A RID: 362 RVA: 0x00005BAB File Offset: 0x00003DAB
		public bool HasMagic { get; private set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x0600016B RID: 363 RVA: 0x00005BB4 File Offset: 0x00003DB4
		// (set) Token: 0x0600016C RID: 364 RVA: 0x00005BBC File Offset: 0x00003DBC
		public int InitialMagic { get; private set; }

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x0600016D RID: 365 RVA: 0x00005BC5 File Offset: 0x00003DC5
		// (set) Token: 0x0600016E RID: 366 RVA: 0x00005BCD File Offset: 0x00003DCD
		public int MagicCost { get; private set; }

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x0600016F RID: 367 RVA: 0x00005BD6 File Offset: 0x00003DD6
		// (set) Token: 0x06000170 RID: 368 RVA: 0x00005BDE File Offset: 0x00003DDE
		public int MaxMagic { get; private set; }

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000171 RID: 369 RVA: 0x00005BE7 File Offset: 0x00003DE7
		// (set) Token: 0x06000172 RID: 370 RVA: 0x00005BEF File Offset: 0x00003DEF
		public Keyword Keywords { get; private set; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000173 RID: 371 RVA: 0x00005BF8 File Offset: 0x00003DF8
		// (set) Token: 0x06000174 RID: 372 RVA: 0x00005C00 File Offset: 0x00003E00
		public IReadOnlyList<string> RelativeEffects { get; private set; }

		// Token: 0x06000175 RID: 373 RVA: 0x00005C09 File Offset: 0x00003E09
		public static IReadOnlyList<DollConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<DollConfig>(DollConfig._data);
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00005C1C File Offset: 0x00003E1C
		public static DollConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			DollConfig dollConfig;
			return (!DollConfig._IdTable.TryGetValue(Id, out dollConfig)) ? null : dollConfig;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00005C48 File Offset: 0x00003E48
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

		// Token: 0x06000178 RID: 376 RVA: 0x00005E9C File Offset: 0x0000409C
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

		// Token: 0x06000179 RID: 377 RVA: 0x00006058 File Offset: 0x00004258
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

		// Token: 0x040000B8 RID: 184
		private static DollConfig[] _data = Array.Empty<DollConfig>();

		// Token: 0x040000B9 RID: 185
		private static Dictionary<string, DollConfig> _IdTable = Enumerable.ToDictionary<DollConfig, string>(DollConfig._data, (DollConfig elem) => elem.Id);
	}
}
