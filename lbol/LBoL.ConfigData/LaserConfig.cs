using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000016 RID: 22
	public sealed class LaserConfig
	{
		// Token: 0x060003F7 RID: 1015 RVA: 0x0000C958 File Offset: 0x0000AB58
		private LaserConfig(string Name, string Widget, string LaunchSfx, Vector2 Size, Vector2 Offset, int Start, string HitBody, string HitBodySfx, string HitShield, string HitShieldSfx, string HitBlock, string HitBlockSfx, string Graze, string GrazeSfx)
		{
			this.Name = Name;
			this.Widget = Widget;
			this.LaunchSfx = LaunchSfx;
			this.Size = Size;
			this.Offset = Offset;
			this.Start = Start;
			this.HitBody = HitBody;
			this.HitBodySfx = HitBodySfx;
			this.HitShield = HitShield;
			this.HitShieldSfx = HitShieldSfx;
			this.HitBlock = HitBlock;
			this.HitBlockSfx = HitBlockSfx;
			this.Graze = Graze;
			this.GrazeSfx = GrazeSfx;
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x060003F8 RID: 1016 RVA: 0x0000C9D8 File Offset: 0x0000ABD8
		// (set) Token: 0x060003F9 RID: 1017 RVA: 0x0000C9E0 File Offset: 0x0000ABE0
		public string Name { get; private set; }

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x060003FA RID: 1018 RVA: 0x0000C9E9 File Offset: 0x0000ABE9
		// (set) Token: 0x060003FB RID: 1019 RVA: 0x0000C9F1 File Offset: 0x0000ABF1
		public string Widget { get; private set; }

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x060003FC RID: 1020 RVA: 0x0000C9FA File Offset: 0x0000ABFA
		// (set) Token: 0x060003FD RID: 1021 RVA: 0x0000CA02 File Offset: 0x0000AC02
		public string LaunchSfx { get; private set; }

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x060003FE RID: 1022 RVA: 0x0000CA0B File Offset: 0x0000AC0B
		// (set) Token: 0x060003FF RID: 1023 RVA: 0x0000CA13 File Offset: 0x0000AC13
		public Vector2 Size { get; private set; }

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000400 RID: 1024 RVA: 0x0000CA1C File Offset: 0x0000AC1C
		// (set) Token: 0x06000401 RID: 1025 RVA: 0x0000CA24 File Offset: 0x0000AC24
		public Vector2 Offset { get; private set; }

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06000402 RID: 1026 RVA: 0x0000CA2D File Offset: 0x0000AC2D
		// (set) Token: 0x06000403 RID: 1027 RVA: 0x0000CA35 File Offset: 0x0000AC35
		public int Start { get; private set; }

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000404 RID: 1028 RVA: 0x0000CA3E File Offset: 0x0000AC3E
		// (set) Token: 0x06000405 RID: 1029 RVA: 0x0000CA46 File Offset: 0x0000AC46
		public string HitBody { get; private set; }

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000406 RID: 1030 RVA: 0x0000CA4F File Offset: 0x0000AC4F
		// (set) Token: 0x06000407 RID: 1031 RVA: 0x0000CA57 File Offset: 0x0000AC57
		public string HitBodySfx { get; private set; }

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000408 RID: 1032 RVA: 0x0000CA60 File Offset: 0x0000AC60
		// (set) Token: 0x06000409 RID: 1033 RVA: 0x0000CA68 File Offset: 0x0000AC68
		public string HitShield { get; private set; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x0600040A RID: 1034 RVA: 0x0000CA71 File Offset: 0x0000AC71
		// (set) Token: 0x0600040B RID: 1035 RVA: 0x0000CA79 File Offset: 0x0000AC79
		public string HitShieldSfx { get; private set; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x0600040C RID: 1036 RVA: 0x0000CA82 File Offset: 0x0000AC82
		// (set) Token: 0x0600040D RID: 1037 RVA: 0x0000CA8A File Offset: 0x0000AC8A
		public string HitBlock { get; private set; }

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x0600040E RID: 1038 RVA: 0x0000CA93 File Offset: 0x0000AC93
		// (set) Token: 0x0600040F RID: 1039 RVA: 0x0000CA9B File Offset: 0x0000AC9B
		public string HitBlockSfx { get; private set; }

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000410 RID: 1040 RVA: 0x0000CAA4 File Offset: 0x0000ACA4
		// (set) Token: 0x06000411 RID: 1041 RVA: 0x0000CAAC File Offset: 0x0000ACAC
		public string Graze { get; private set; }

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000412 RID: 1042 RVA: 0x0000CAB5 File Offset: 0x0000ACB5
		// (set) Token: 0x06000413 RID: 1043 RVA: 0x0000CABD File Offset: 0x0000ACBD
		public string GrazeSfx { get; private set; }

		// Token: 0x06000414 RID: 1044 RVA: 0x0000CAC6 File Offset: 0x0000ACC6
		public static IReadOnlyList<LaserConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<LaserConfig>(LaserConfig._data);
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x0000CAD8 File Offset: 0x0000ACD8
		public static LaserConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			LaserConfig laserConfig;
			return (!LaserConfig._NameTable.TryGetValue(Name, out laserConfig)) ? null : laserConfig;
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x0000CB04 File Offset: 0x0000AD04
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{LaserConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Widget=",
				ConfigDataManager.System_String.ToString(this.Widget),
				", LaunchSfx=",
				ConfigDataManager.System_String.ToString(this.LaunchSfx),
				", Size=",
				ConfigDataManager.UnityEngine_Vector2.ToString(this.Size),
				", Offset=",
				ConfigDataManager.UnityEngine_Vector2.ToString(this.Offset),
				", Start=",
				ConfigDataManager.System_Int32.ToString(this.Start),
				", HitBody=",
				ConfigDataManager.System_String.ToString(this.HitBody),
				", HitBodySfx=",
				ConfigDataManager.System_String.ToString(this.HitBodySfx),
				", HitShield=",
				ConfigDataManager.System_String.ToString(this.HitShield),
				", HitShieldSfx=",
				ConfigDataManager.System_String.ToString(this.HitShieldSfx),
				", HitBlock=",
				ConfigDataManager.System_String.ToString(this.HitBlock),
				", HitBlockSfx=",
				ConfigDataManager.System_String.ToString(this.HitBlockSfx),
				", Graze=",
				ConfigDataManager.System_String.ToString(this.Graze),
				", GrazeSfx=",
				ConfigDataManager.System_String.ToString(this.GrazeSfx),
				"}"
			});
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0000CCB4 File Offset: 0x0000AEB4
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				LaserConfig[] array = new LaserConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new LaserConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				LaserConfig._data = array;
				LaserConfig._NameTable = Enumerable.ToDictionary<LaserConfig, string>(LaserConfig._data, (LaserConfig elem) => elem.Name);
			}
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0000CDE4 File Offset: 0x0000AFE4
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/LaserConfig");
			if (textAsset != null)
			{
				try
				{
					LaserConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load LaserConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'LaserConfig', please reimport config data");
			}
		}

		// Token: 0x04000231 RID: 561
		private static LaserConfig[] _data = Array.Empty<LaserConfig>();

		// Token: 0x04000232 RID: 562
		private static Dictionary<string, LaserConfig> _NameTable = Enumerable.ToDictionary<LaserConfig, string>(LaserConfig._data, (LaserConfig elem) => elem.Name);
	}
}
