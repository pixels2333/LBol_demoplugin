using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000015 RID: 21
	public sealed class BulletConfig
	{
		// Token: 0x060003D6 RID: 982 RVA: 0x0000C49C File Offset: 0x0000A69C
		private BulletConfig(string Name, string Widget, string Launch, string LaunchSfx, string HitBody, string HitBodySfx, string HitShield, string HitShieldSfx, string HitBlock, string HitBlockSfx, string Graze, string GrazeSfx)
		{
			this.Name = Name;
			this.Widget = Widget;
			this.Launch = Launch;
			this.LaunchSfx = LaunchSfx;
			this.HitBody = HitBody;
			this.HitBodySfx = HitBodySfx;
			this.HitShield = HitShield;
			this.HitShieldSfx = HitShieldSfx;
			this.HitBlock = HitBlock;
			this.HitBlockSfx = HitBlockSfx;
			this.Graze = Graze;
			this.GrazeSfx = GrazeSfx;
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x060003D7 RID: 983 RVA: 0x0000C50C File Offset: 0x0000A70C
		// (set) Token: 0x060003D8 RID: 984 RVA: 0x0000C514 File Offset: 0x0000A714
		public string Name { get; private set; }

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x060003D9 RID: 985 RVA: 0x0000C51D File Offset: 0x0000A71D
		// (set) Token: 0x060003DA RID: 986 RVA: 0x0000C525 File Offset: 0x0000A725
		public string Widget { get; private set; }

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x060003DB RID: 987 RVA: 0x0000C52E File Offset: 0x0000A72E
		// (set) Token: 0x060003DC RID: 988 RVA: 0x0000C536 File Offset: 0x0000A736
		public string Launch { get; private set; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x060003DD RID: 989 RVA: 0x0000C53F File Offset: 0x0000A73F
		// (set) Token: 0x060003DE RID: 990 RVA: 0x0000C547 File Offset: 0x0000A747
		public string LaunchSfx { get; private set; }

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x060003DF RID: 991 RVA: 0x0000C550 File Offset: 0x0000A750
		// (set) Token: 0x060003E0 RID: 992 RVA: 0x0000C558 File Offset: 0x0000A758
		public string HitBody { get; private set; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x060003E1 RID: 993 RVA: 0x0000C561 File Offset: 0x0000A761
		// (set) Token: 0x060003E2 RID: 994 RVA: 0x0000C569 File Offset: 0x0000A769
		public string HitBodySfx { get; private set; }

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x060003E3 RID: 995 RVA: 0x0000C572 File Offset: 0x0000A772
		// (set) Token: 0x060003E4 RID: 996 RVA: 0x0000C57A File Offset: 0x0000A77A
		public string HitShield { get; private set; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x060003E5 RID: 997 RVA: 0x0000C583 File Offset: 0x0000A783
		// (set) Token: 0x060003E6 RID: 998 RVA: 0x0000C58B File Offset: 0x0000A78B
		public string HitShieldSfx { get; private set; }

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x060003E7 RID: 999 RVA: 0x0000C594 File Offset: 0x0000A794
		// (set) Token: 0x060003E8 RID: 1000 RVA: 0x0000C59C File Offset: 0x0000A79C
		public string HitBlock { get; private set; }

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x060003E9 RID: 1001 RVA: 0x0000C5A5 File Offset: 0x0000A7A5
		// (set) Token: 0x060003EA RID: 1002 RVA: 0x0000C5AD File Offset: 0x0000A7AD
		public string HitBlockSfx { get; private set; }

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x060003EB RID: 1003 RVA: 0x0000C5B6 File Offset: 0x0000A7B6
		// (set) Token: 0x060003EC RID: 1004 RVA: 0x0000C5BE File Offset: 0x0000A7BE
		public string Graze { get; private set; }

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x060003ED RID: 1005 RVA: 0x0000C5C7 File Offset: 0x0000A7C7
		// (set) Token: 0x060003EE RID: 1006 RVA: 0x0000C5CF File Offset: 0x0000A7CF
		public string GrazeSfx { get; private set; }

		// Token: 0x060003EF RID: 1007 RVA: 0x0000C5D8 File Offset: 0x0000A7D8
		public static IReadOnlyList<BulletConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<BulletConfig>(BulletConfig._data);
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x0000C5EC File Offset: 0x0000A7EC
		public static BulletConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			BulletConfig bulletConfig;
			return (!BulletConfig._NameTable.TryGetValue(Name, out bulletConfig)) ? null : bulletConfig;
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0000C618 File Offset: 0x0000A818
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{BulletConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Widget=",
				ConfigDataManager.System_String.ToString(this.Widget),
				", Launch=",
				ConfigDataManager.System_String.ToString(this.Launch),
				", LaunchSfx=",
				ConfigDataManager.System_String.ToString(this.LaunchSfx),
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

		// Token: 0x060003F2 RID: 1010 RVA: 0x0000C790 File Offset: 0x0000A990
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				BulletConfig[] array = new BulletConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new BulletConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				BulletConfig._data = array;
				BulletConfig._NameTable = Enumerable.ToDictionary<BulletConfig, string>(BulletConfig._data, (BulletConfig elem) => elem.Name);
			}
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0000C8AC File Offset: 0x0000AAAC
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/BulletConfig");
			if (textAsset != null)
			{
				try
				{
					BulletConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load BulletConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'BulletConfig', please reimport config data");
			}
		}

		// Token: 0x04000220 RID: 544
		private static BulletConfig[] _data = Array.Empty<BulletConfig>();

		// Token: 0x04000221 RID: 545
		private static Dictionary<string, BulletConfig> _NameTable = Enumerable.ToDictionary<BulletConfig, string>(BulletConfig._data, (BulletConfig elem) => elem.Name);
	}
}
