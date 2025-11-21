using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000008 RID: 8
	public sealed class EnemyGroupConfig
	{
		// Token: 0x060000D1 RID: 209 RVA: 0x000046A0 File Offset: 0x000028A0
		private EnemyGroupConfig(string Id, bool IsSub, IReadOnlyList<string> Subs, string Name, string FormationName, IReadOnlyList<string> Enemies, EnemyType EnemyType, bool Hidden, float DebutTime, bool RollBossExhibit, Vector2 PlayerRoot, string PreBattleDialogName, string PostBattleDialogName, string Environment)
		{
			this.Id = Id;
			this.IsSub = IsSub;
			this.Subs = Subs;
			this.Name = Name;
			this.FormationName = FormationName;
			this.Enemies = Enemies;
			this.EnemyType = EnemyType;
			this.Hidden = Hidden;
			this.DebutTime = DebutTime;
			this.RollBossExhibit = RollBossExhibit;
			this.PlayerRoot = PlayerRoot;
			this.PreBattleDialogName = PreBattleDialogName;
			this.PostBattleDialogName = PostBattleDialogName;
			this.Environment = Environment;
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x00004720 File Offset: 0x00002920
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x00004728 File Offset: 0x00002928
		public string Id { get; private set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x00004731 File Offset: 0x00002931
		// (set) Token: 0x060000D5 RID: 213 RVA: 0x00004739 File Offset: 0x00002939
		public bool IsSub { get; private set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00004742 File Offset: 0x00002942
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x0000474A File Offset: 0x0000294A
		public IReadOnlyList<string> Subs { get; private set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x00004753 File Offset: 0x00002953
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x0000475B File Offset: 0x0000295B
		public string Name { get; private set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000DA RID: 218 RVA: 0x00004764 File Offset: 0x00002964
		// (set) Token: 0x060000DB RID: 219 RVA: 0x0000476C File Offset: 0x0000296C
		public string FormationName { get; private set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00004775 File Offset: 0x00002975
		// (set) Token: 0x060000DD RID: 221 RVA: 0x0000477D File Offset: 0x0000297D
		public IReadOnlyList<string> Enemies { get; private set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00004786 File Offset: 0x00002986
		// (set) Token: 0x060000DF RID: 223 RVA: 0x0000478E File Offset: 0x0000298E
		public EnemyType EnemyType { get; private set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00004797 File Offset: 0x00002997
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x0000479F File Offset: 0x0000299F
		public bool Hidden { get; private set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x000047A8 File Offset: 0x000029A8
		// (set) Token: 0x060000E3 RID: 227 RVA: 0x000047B0 File Offset: 0x000029B0
		public float DebutTime { get; private set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x000047B9 File Offset: 0x000029B9
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x000047C1 File Offset: 0x000029C1
		public bool RollBossExhibit { get; private set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x000047CA File Offset: 0x000029CA
		// (set) Token: 0x060000E7 RID: 231 RVA: 0x000047D2 File Offset: 0x000029D2
		public Vector2 PlayerRoot { get; private set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x000047DB File Offset: 0x000029DB
		// (set) Token: 0x060000E9 RID: 233 RVA: 0x000047E3 File Offset: 0x000029E3
		public string PreBattleDialogName { get; private set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060000EA RID: 234 RVA: 0x000047EC File Offset: 0x000029EC
		// (set) Token: 0x060000EB RID: 235 RVA: 0x000047F4 File Offset: 0x000029F4
		public string PostBattleDialogName { get; private set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060000EC RID: 236 RVA: 0x000047FD File Offset: 0x000029FD
		// (set) Token: 0x060000ED RID: 237 RVA: 0x00004805 File Offset: 0x00002A05
		public string Environment { get; private set; }

		// Token: 0x060000EE RID: 238 RVA: 0x0000480E File Offset: 0x00002A0E
		public static IReadOnlyList<EnemyGroupConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<EnemyGroupConfig>(EnemyGroupConfig._data);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00004820 File Offset: 0x00002A20
		public static EnemyGroupConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			EnemyGroupConfig enemyGroupConfig;
			return (!EnemyGroupConfig._IdTable.TryGetValue(Id, out enemyGroupConfig)) ? null : enemyGroupConfig;
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0000484C File Offset: 0x00002A4C
		public override string ToString()
		{
			string[] array = new string[29];
			array[0] = "{EnemyGroupConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", IsSub=";
			array[3] = ConfigDataManager.System_Boolean.ToString(this.IsSub);
			array[4] = ", Subs=[";
			array[5] = string.Join(", ", Enumerable.Select<string, string>(this.Subs, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[6] = "], Name=";
			array[7] = ConfigDataManager.System_String.ToString(this.Name);
			array[8] = ", FormationName=";
			array[9] = ConfigDataManager.System_String.ToString(this.FormationName);
			array[10] = ", Enemies=[";
			array[11] = string.Join(", ", Enumerable.Select<string, string>(this.Enemies, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[12] = "], EnemyType=";
			array[13] = this.EnemyType.ToString();
			array[14] = ", Hidden=";
			array[15] = ConfigDataManager.System_Boolean.ToString(this.Hidden);
			array[16] = ", DebutTime=";
			array[17] = ConfigDataManager.System_Single.ToString(this.DebutTime);
			array[18] = ", RollBossExhibit=";
			array[19] = ConfigDataManager.System_Boolean.ToString(this.RollBossExhibit);
			array[20] = ", PlayerRoot=";
			array[21] = ConfigDataManager.UnityEngine_Vector2.ToString(this.PlayerRoot);
			array[22] = ", PreBattleDialogName=";
			array[23] = ConfigDataManager.System_String.ToString(this.PreBattleDialogName);
			array[24] = ", PostBattleDialogName=";
			array[25] = ConfigDataManager.System_String.ToString(this.PostBattleDialogName);
			array[26] = ", Environment=";
			array[27] = ConfigDataManager.System_String.ToString(this.Environment);
			array[28] = "}";
			return string.Concat(array);
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00004A44 File Offset: 0x00002C44
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				EnemyGroupConfig[] array = new EnemyGroupConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new EnemyGroupConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), (EnemyType)binaryReader.ReadInt32(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				EnemyGroupConfig._data = array;
				EnemyGroupConfig._IdTable = Enumerable.ToDictionary<EnemyGroupConfig, string>(EnemyGroupConfig._data, (EnemyGroupConfig elem) => elem.Id);
			}
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00004BAC File Offset: 0x00002DAC
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/EnemyGroupConfig");
			if (textAsset != null)
			{
				try
				{
					EnemyGroupConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load EnemyGroupConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'EnemyGroupConfig', please reimport config data");
			}
		}

		// Token: 0x04000073 RID: 115
		private static EnemyGroupConfig[] _data = Array.Empty<EnemyGroupConfig>();

		// Token: 0x04000074 RID: 116
		private static Dictionary<string, EnemyGroupConfig> _IdTable = Enumerable.ToDictionary<EnemyGroupConfig, string>(EnemyGroupConfig._data, (EnemyGroupConfig elem) => elem.Id);
	}
}
