using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000018 RID: 24
	public sealed class AdventureConfig
	{
		// Token: 0x06000439 RID: 1081 RVA: 0x0000D2C3 File Offset: 0x0000B4C3
		private AdventureConfig(int No, string Id, string HostId, string HostId2, int Music, bool HideUlt, bool TempArt)
		{
			this.No = No;
			this.Id = Id;
			this.HostId = HostId;
			this.HostId2 = HostId2;
			this.Music = Music;
			this.HideUlt = HideUlt;
			this.TempArt = TempArt;
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x0600043A RID: 1082 RVA: 0x0000D300 File Offset: 0x0000B500
		// (set) Token: 0x0600043B RID: 1083 RVA: 0x0000D308 File Offset: 0x0000B508
		public int No { get; private set; }

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x0600043C RID: 1084 RVA: 0x0000D311 File Offset: 0x0000B511
		// (set) Token: 0x0600043D RID: 1085 RVA: 0x0000D319 File Offset: 0x0000B519
		public string Id { get; private set; }

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x0000D322 File Offset: 0x0000B522
		// (set) Token: 0x0600043F RID: 1087 RVA: 0x0000D32A File Offset: 0x0000B52A
		public string HostId { get; private set; }

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000440 RID: 1088 RVA: 0x0000D333 File Offset: 0x0000B533
		// (set) Token: 0x06000441 RID: 1089 RVA: 0x0000D33B File Offset: 0x0000B53B
		public string HostId2 { get; private set; }

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06000442 RID: 1090 RVA: 0x0000D344 File Offset: 0x0000B544
		// (set) Token: 0x06000443 RID: 1091 RVA: 0x0000D34C File Offset: 0x0000B54C
		public int Music { get; private set; }

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06000444 RID: 1092 RVA: 0x0000D355 File Offset: 0x0000B555
		// (set) Token: 0x06000445 RID: 1093 RVA: 0x0000D35D File Offset: 0x0000B55D
		public bool HideUlt { get; private set; }

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000446 RID: 1094 RVA: 0x0000D366 File Offset: 0x0000B566
		// (set) Token: 0x06000447 RID: 1095 RVA: 0x0000D36E File Offset: 0x0000B56E
		public bool TempArt { get; private set; }

		// Token: 0x06000448 RID: 1096 RVA: 0x0000D377 File Offset: 0x0000B577
		public static IReadOnlyList<AdventureConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<AdventureConfig>(AdventureConfig._data);
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x0000D388 File Offset: 0x0000B588
		public static AdventureConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			AdventureConfig adventureConfig;
			return (!AdventureConfig._IdTable.TryGetValue(Id, out adventureConfig)) ? null : adventureConfig;
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x0000D3B4 File Offset: 0x0000B5B4
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{AdventureConfig No=",
				ConfigDataManager.System_Int32.ToString(this.No),
				", Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", HostId=",
				ConfigDataManager.System_String.ToString(this.HostId),
				", HostId2=",
				ConfigDataManager.System_String.ToString(this.HostId2),
				", Music=",
				ConfigDataManager.System_Int32.ToString(this.Music),
				", HideUlt=",
				ConfigDataManager.System_Boolean.ToString(this.HideUlt),
				", TempArt=",
				ConfigDataManager.System_Boolean.ToString(this.TempArt),
				"}"
			});
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x0000D498 File Offset: 0x0000B698
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				AdventureConfig[] array = new AdventureConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new AdventureConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader));
				}
				AdventureConfig._data = array;
				AdventureConfig._IdTable = Enumerable.ToDictionary<AdventureConfig, string>(AdventureConfig._data, (AdventureConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x0000D57C File Offset: 0x0000B77C
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/AdventureConfig");
			if (textAsset != null)
			{
				try
				{
					AdventureConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load AdventureConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'AdventureConfig', please reimport config data");
			}
		}

		// Token: 0x04000248 RID: 584
		private static AdventureConfig[] _data = Array.Empty<AdventureConfig>();

		// Token: 0x04000249 RID: 585
		private static Dictionary<string, AdventureConfig> _IdTable = Enumerable.ToDictionary<AdventureConfig, string>(AdventureConfig._data, (AdventureConfig elem) => elem.Id);
	}
}
