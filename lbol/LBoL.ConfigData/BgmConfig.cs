using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000002 RID: 2
	public sealed class BgmConfig
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private BgmConfig(string ID, int No, string Name, string Folder, string Path, float Volume, float? LoopStart, float? LoopEnd, float? ExtraDelay, string TrackName, string Artist, string Original, string Comment)
		{
			this.ID = ID;
			this.No = No;
			this.Name = Name;
			this.Folder = Folder;
			this.Path = Path;
			this.Volume = Volume;
			this.LoopStart = LoopStart;
			this.LoopEnd = LoopEnd;
			this.ExtraDelay = ExtraDelay;
			this.TrackName = TrackName;
			this.Artist = Artist;
			this.Original = Original;
			this.Comment = Comment;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x000020C8 File Offset: 0x000002C8
		// (set) Token: 0x06000003 RID: 3 RVA: 0x000020D0 File Offset: 0x000002D0
		public string ID { get; private set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4 RVA: 0x000020D9 File Offset: 0x000002D9
		// (set) Token: 0x06000005 RID: 5 RVA: 0x000020E1 File Offset: 0x000002E1
		public int No { get; private set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000006 RID: 6 RVA: 0x000020EA File Offset: 0x000002EA
		// (set) Token: 0x06000007 RID: 7 RVA: 0x000020F2 File Offset: 0x000002F2
		public string Name { get; private set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000020FB File Offset: 0x000002FB
		// (set) Token: 0x06000009 RID: 9 RVA: 0x00002103 File Offset: 0x00000303
		public string Folder { get; private set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000A RID: 10 RVA: 0x0000210C File Offset: 0x0000030C
		// (set) Token: 0x0600000B RID: 11 RVA: 0x00002114 File Offset: 0x00000314
		public string Path { get; private set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000C RID: 12 RVA: 0x0000211D File Offset: 0x0000031D
		// (set) Token: 0x0600000D RID: 13 RVA: 0x00002125 File Offset: 0x00000325
		public float Volume { get; private set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600000E RID: 14 RVA: 0x0000212E File Offset: 0x0000032E
		// (set) Token: 0x0600000F RID: 15 RVA: 0x00002136 File Offset: 0x00000336
		public float? LoopStart { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000010 RID: 16 RVA: 0x0000213F File Offset: 0x0000033F
		// (set) Token: 0x06000011 RID: 17 RVA: 0x00002147 File Offset: 0x00000347
		public float? LoopEnd { get; private set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002150 File Offset: 0x00000350
		// (set) Token: 0x06000013 RID: 19 RVA: 0x00002158 File Offset: 0x00000358
		public float? ExtraDelay { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000014 RID: 20 RVA: 0x00002161 File Offset: 0x00000361
		// (set) Token: 0x06000015 RID: 21 RVA: 0x00002169 File Offset: 0x00000369
		public string TrackName { get; private set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000016 RID: 22 RVA: 0x00002172 File Offset: 0x00000372
		// (set) Token: 0x06000017 RID: 23 RVA: 0x0000217A File Offset: 0x0000037A
		public string Artist { get; private set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000018 RID: 24 RVA: 0x00002183 File Offset: 0x00000383
		// (set) Token: 0x06000019 RID: 25 RVA: 0x0000218B File Offset: 0x0000038B
		public string Original { get; private set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002194 File Offset: 0x00000394
		// (set) Token: 0x0600001B RID: 27 RVA: 0x0000219C File Offset: 0x0000039C
		public string Comment { get; private set; }

		// Token: 0x0600001C RID: 28 RVA: 0x000021A5 File Offset: 0x000003A5
		public static IReadOnlyList<BgmConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<BgmConfig>(BgmConfig._data);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000021B8 File Offset: 0x000003B8
		public static BgmConfig FromID(string ID)
		{
			ConfigDataManager.Initialize();
			BgmConfig bgmConfig;
			return (!BgmConfig._IDTable.TryGetValue(ID, out bgmConfig)) ? null : bgmConfig;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000021E4 File Offset: 0x000003E4
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{BgmConfig ID=",
				ConfigDataManager.System_String.ToString(this.ID),
				", No=",
				ConfigDataManager.System_Int32.ToString(this.No),
				", Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Folder=",
				ConfigDataManager.System_String.ToString(this.Folder),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Volume=",
				ConfigDataManager.System_Single.ToString(this.Volume),
				", LoopStart=",
				(this.LoopStart == null) ? "null" : ConfigDataManager.System_Single.ToString(this.LoopStart.Value),
				", LoopEnd=",
				(this.LoopEnd == null) ? "null" : ConfigDataManager.System_Single.ToString(this.LoopEnd.Value),
				", ExtraDelay=",
				(this.ExtraDelay == null) ? "null" : ConfigDataManager.System_Single.ToString(this.ExtraDelay.Value),
				", TrackName=",
				ConfigDataManager.System_String.ToString(this.TrackName),
				", Artist=",
				ConfigDataManager.System_String.ToString(this.Artist),
				", Original=",
				ConfigDataManager.System_String.ToString(this.Original),
				", Comment=",
				ConfigDataManager.System_String.ToString(this.Comment),
				"}"
			});
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000023E8 File Offset: 0x000005E8
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				BgmConfig[] array = new BgmConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new BgmConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				BgmConfig._data = array;
				BgmConfig._IDTable = Enumerable.ToDictionary<BgmConfig, string>(BgmConfig._data, (BgmConfig elem) => elem.ID);
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002574 File Offset: 0x00000774
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/BgmConfig");
			if (textAsset != null)
			{
				try
				{
					BgmConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load BgmConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'BgmConfig', please reimport config data");
			}
		}

		// Token: 0x0400000E RID: 14
		private static BgmConfig[] _data = Array.Empty<BgmConfig>();

		// Token: 0x0400000F RID: 15
		private static Dictionary<string, BgmConfig> _IDTable = Enumerable.ToDictionary<BgmConfig, string>(BgmConfig._data, (BgmConfig elem) => elem.ID);
	}
}
