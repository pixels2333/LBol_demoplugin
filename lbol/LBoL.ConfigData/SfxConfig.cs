using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000003 RID: 3
	public sealed class SfxConfig
	{
		// Token: 0x06000024 RID: 36 RVA: 0x0000261F File Offset: 0x0000081F
		private SfxConfig(string Name, string Folder, string Path, double Rep, float Volume)
		{
			this.Name = Name;
			this.Folder = Folder;
			this.Path = Path;
			this.Rep = Rep;
			this.Volume = Volume;
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000025 RID: 37 RVA: 0x0000264C File Offset: 0x0000084C
		// (set) Token: 0x06000026 RID: 38 RVA: 0x00002654 File Offset: 0x00000854
		public string Name { get; private set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000027 RID: 39 RVA: 0x0000265D File Offset: 0x0000085D
		// (set) Token: 0x06000028 RID: 40 RVA: 0x00002665 File Offset: 0x00000865
		public string Folder { get; private set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000029 RID: 41 RVA: 0x0000266E File Offset: 0x0000086E
		// (set) Token: 0x0600002A RID: 42 RVA: 0x00002676 File Offset: 0x00000876
		public string Path { get; private set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600002B RID: 43 RVA: 0x0000267F File Offset: 0x0000087F
		// (set) Token: 0x0600002C RID: 44 RVA: 0x00002687 File Offset: 0x00000887
		public double Rep { get; private set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002690 File Offset: 0x00000890
		// (set) Token: 0x0600002E RID: 46 RVA: 0x00002698 File Offset: 0x00000898
		public float Volume { get; private set; }

		// Token: 0x0600002F RID: 47 RVA: 0x000026A1 File Offset: 0x000008A1
		public static IReadOnlyList<SfxConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SfxConfig>(SfxConfig._data);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000026B4 File Offset: 0x000008B4
		public static SfxConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			SfxConfig sfxConfig;
			return (!SfxConfig._NameTable.TryGetValue(Name, out sfxConfig)) ? null : sfxConfig;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000026E0 File Offset: 0x000008E0
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{SfxConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Folder=",
				ConfigDataManager.System_String.ToString(this.Folder),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Rep=",
				ConfigDataManager.System_Double.ToString(this.Rep),
				", Volume=",
				ConfigDataManager.System_Single.ToString(this.Volume),
				"}"
			});
		}

		// Token: 0x06000032 RID: 50 RVA: 0x0000278C File Offset: 0x0000098C
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SfxConfig[] array = new SfxConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SfxConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Double.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				SfxConfig._data = array;
				SfxConfig._NameTable = Enumerable.ToDictionary<SfxConfig, string>(SfxConfig._data, (SfxConfig elem) => elem.Name);
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002858 File Offset: 0x00000A58
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SfxConfig");
			if (textAsset != null)
			{
				try
				{
					SfxConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SfxConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SfxConfig', please reimport config data");
			}
		}

		// Token: 0x04000016 RID: 22
		private static SfxConfig[] _data = Array.Empty<SfxConfig>();

		// Token: 0x04000017 RID: 23
		private static Dictionary<string, SfxConfig> _NameTable = Enumerable.ToDictionary<SfxConfig, string>(SfxConfig._data, (SfxConfig elem) => elem.Name);
	}
}
